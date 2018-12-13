using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Extension.Cache;
using Extension.CompositionRoot;
using Extension.ExtensionStatus;
using Extension.Tagging.SqlQuery;
using Main;
using Main.Inclusion.Scanner;
using Main.Inclusion;
using Main.Inclusion.Validated;
using Main.Progress;
using Main.Validator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using System.Threading.Tasks;
using Main.Inclusion.Found;
using Microsoft.VisualStudio.Shell;

namespace Extension.Tagging.Extractor
{
    public class TimeoutTagExtractor : IVsSolutionEventsExt, ITimeoutTagExtractor, IDisposable
    {
        private const long NotStarted = 0L;
        private const long Started = 1L;
        private const long Disposed = 2L;

        private readonly IExtensionStatus _extensionStatus;
        private readonly IInclusionScannerFactory _inclusionScannerFactory;
        private readonly SqlInclusionCache _cache;
        private readonly TimeSpan _timeout;
        private readonly AutoResetEvent _changesExistsSignal = new AutoResetEvent(false);
        private readonly ManualResetEvent _stopSignal = new ManualResetEvent(false);

        private readonly ConcurrentDictionary<string, DocumentDescriptor> _descriptors = new ConcurrentDictionary<string, DocumentDescriptor>(StringComparer.InvariantCultureIgnoreCase);

        private Thread _t;

        private long _status = NotStarted;
        private readonly object _extractLocker = new object();

        public uint Cookie
        {
            get;
            set;
        }

        public TimeoutTagExtractor(
            IExtensionStatus extensionStatus,
            IInclusionScannerFactory inclusionScannerFactory,
            SqlInclusionCache cache,
            TimeSpan timeout
            )
        {
            if (extensionStatus == null)
            {
                throw new ArgumentNullException(nameof(extensionStatus));
            }

            if (inclusionScannerFactory == null)
            {
                throw new ArgumentNullException(nameof(inclusionScannerFactory));
            }

            if (cache == null)
            {
                throw new ArgumentNullException(nameof(cache));
            }

            _extensionStatus = extensionStatus;
            _inclusionScannerFactory = inclusionScannerFactory;
            _cache = cache;
            _timeout = timeout;
        }

        public List<ITagSpan<SqlQueryTag>> GetTags(
            ITagUpdater tagUpdater,
            ITextBuffer buffer
            )
        {
            if (tagUpdater == null)
            {
                throw new ArgumentNullException(nameof(tagUpdater));
            }

            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if(!_extensionStatus.IsEnabled)
            {
                return
                    new List<ITagSpan<SqlQueryTag>>();
            }

            var textContainer = buffer.AsTextContainer();
            if (!Workspace.TryGetWorkspace(textContainer, out var workspace))
            {
                return
                    new List<ITagSpan<SqlQueryTag>>();
            }

            ITextSnapshot currentSnapshot = buffer.CurrentSnapshot;
            if (currentSnapshot == null)
            {
                return
                    new List<ITagSpan<SqlQueryTag>>();
            }

            var result = ExtractTags(
                tagUpdater, 
                textContainer, 
                workspace, 
                currentSnapshot
                );

            return
                result;
        }

        private List<ITagSpan<SqlQueryTag>> ExtractTags(
            ITagUpdater tagUpdater, 
            SourceTextContainer textContainer, 
            Workspace workspace, 
            ITextSnapshot currentSnapshot
            )
        {
            //with every change (or just scrolling)
            //we return tags from its cache
            //cache is updated from background thread

            var result = new List<ITagSpan<SqlQueryTag>>();

            var origDocumentId = workspace.GetDocumentIdInCurrentContext(textContainer);
            var origDocument = workspace.CurrentSolution.GetDocument(origDocumentId);

            var fileCache = _cache.GetOrCreateFileCache(
                origDocument.FilePath
                );

            var isChangesExists = false;
            lock (_extractLocker)
            {
                isChangesExists = !_descriptors.TryGetValue(origDocument.FilePath, out var lastDescriptor) || lastDescriptor.LastSnapshot != currentSnapshot;

                if (isChangesExists)
                {
                    //2 posibilities exists
                    //1) no descriptor with this filepath exists
                    //2) there are changes against the last descriptor
                    //so, save current (new) decriptor and send an appropriate signal to the thread

                    fileCache.Clear();

                    _descriptors[origDocument.FilePath] = new DocumentDescriptor(
                        origDocument,
                        origDocument.FilePath,
                        tagUpdater,
                        currentSnapshot,
                        DateTime.Now
                        );

                    _changesExistsSignal.Set();
                }
            }

            if (!isChangesExists)
            {
                //nothing changed
                //return existing inclusions

                var inclusionsList = fileCache.GetAll(
                    );

                result.AddRange(
                    inclusionsList.ConvertAll(j => j.CreateTagSpan(currentSnapshot))
                    );
            }

            return result;
        }

        public void AsyncStart()
        {
            if (Interlocked.CompareExchange(ref _status, Started, NotStarted) != NotStarted)
            {
                return;
            }

            _t = new Thread(DoWork);
            _t.IsBackground = true;
            _t.Start();
        }

        public void SyncStop(
            )
        {
            Dispose();
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _status, Disposed) == Disposed)
            {
                return;
            }

            _stopSignal.Set();

            if (_t != null)
            {
                _t.Join();
            }

            _stopSignal.Dispose();
            _changesExistsSignal.Dispose();
        }

        private void DoWork(
            )
        {
            bool? successDescriptorProcess = null;
            while(true)
            {
                var now = DateTime.Now;

                DocumentDescriptor oldestDescriptor = null;

                var waitTimeout =
                    //TimeSpan.FromMilliseconds(-1); //infinity
                    TimeSpan.FromSeconds(10); //some kind of guarantee that descriptors will be processed even the _changesExistsSignal is lost somehow

                if(successDescriptorProcess.GetValueOrDefault(true))
                //if (!successDescriptorProcess.HasValue || successDescriptorProcess.Value)
                {
                    if (_descriptors.Count > 0)
                    {
                        oldestDescriptor =
                            (from v in _descriptors.Values
                             where !v.IsProcessed
                             orderby v.LastChangedDate ascending
                             select v).FirstOrDefault();

                        if (oldestDescriptor != null)
                        {
                            waitTimeout = oldestDescriptor.LastChangedDate.Add(_timeout) - now;
                        }
                    }
                }

                if (waitTimeout > TimeSpan.Zero)
                {
                    var waitResult = WaitHandle.WaitAny(
                        new WaitHandle[]
                        {
                            _changesExistsSignal,
                            _stopSignal
                        },
                        waitTimeout
                        );

                    if (waitResult == 1)
                    {
                        //exit signal
                        return;
                    }

                    if (waitResult == 0)
                    {
                        //new changes exists
                        //repeat!
                        continue;
                    }
                }

                if (oldestDescriptor != null)
                {
                    //awaked by timeout
                    //we need to process choosen descriptor
                    successDescriptorProcess = ProcessDescriptor(oldestDescriptor);
                }
                else
                {
                    successDescriptorProcess = null;
                }
            }
        }

        private bool ProcessDescriptor(
            DocumentDescriptor descriptor
            )
        {
            if (descriptor == null)
            {
                throw new ArgumentNullException(nameof(descriptor));
            }

            try
            {
                var document = descriptor.Document;

                if (document == null)
                {
                    return false;
                }

                var inclusionScanner = _inclusionScannerFactory.Create(
                    );

                var foundInclusionList = inclusionScanner.Scan(
                    document
                    );

                var validationInclusionList = foundInclusionList.ConvertAll(j => (IValidatedSqlInclusion)new ValidatedSqlInclusion(j));

                var fileCache = _cache.GetOrCreateFileCache(
                    document.FilePath
                    );
                
                fileCache.Update(
                    validationInclusionList
                    );

                var start = 0;
                var end = descriptor.LastSnapshot.Length;

                if(foundInclusionList.Count > 0)
                {
                    start = foundInclusionList.Min(j => j.TargetSyntax.Span.Start);
                    end = foundInclusionList.Max(j => j.TargetSyntax.Span.End);
                }

                descriptor.Raise(
                    start,
                    end
                    );

                return true;
            }
            catch (Exception excp)
            {
                Debug.WriteLine(excp.Message);
                Debug.WriteLine(excp.StackTrace);
            }

            return false;
        }

        private void ClearDescriptors()
        {
            lock (_extractLocker)
            {
                _descriptors.Clear();
            }
        }

        #region IVsSolutionEvents

        public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
        {
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
        {
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
        {
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
        {
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
        {
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
        {
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        {
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
        {
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        public int OnBeforeCloseSolution(object pUnkReserved)
        {
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        public int OnAfterCloseSolution(object pUnkReserved)
        {
            ClearDescriptors();

            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        #endregion

        private class DocumentDescriptor
        {
            public Document Document
            {
                get;
            }

            public string DocumentPath
            {
                get;
            }

            public ITagUpdater Updater
            {
                get;
            }

            public ITextSnapshot LastSnapshot
            {
                get;
            }

            public DateTime LastChangedDate
            {
                get;
            }

            public bool IsProcessed
            {
                get;
                private set;
            }

            public DocumentDescriptor(
                Document document,
                string documentPath,
                ITagUpdater updater,
                ITextSnapshot lastSnapshot,
                DateTime lastChangedDate
                )
            {
                if (document == null)
                {
                    throw new ArgumentNullException(nameof(document));
                }

                if (documentPath == null)
                {
                    throw new ArgumentNullException(nameof(documentPath));
                }

                if (updater == null)
                {
                    throw new ArgumentNullException(nameof(updater));
                }

                if (lastSnapshot == null)
                {
                    throw new ArgumentNullException(nameof(lastSnapshot));
                }

                Document = document;
                DocumentPath = documentPath;
                Updater = updater;
                LastSnapshot = lastSnapshot;
                LastChangedDate = lastChangedDate;
            }


            public void Raise(
                int start,
                int end
                )
            {
                Updater.Raise(
                    LastSnapshot,
                    start,
                    end
                    );

                IsProcessed = true;
            }
        }
    }
}
