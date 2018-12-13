using EnvDTE;
using EnvDTE80;
using Extension.Cache;
using Extension.ExtensionStatus.FullyLoaded;
using Extension.Tagging.Extractor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Ninject;
using Ninject.Extensions.ChildKernel;
using Ninject.Parameters;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Extension.CompositionRoot
{
    public sealed class Root : IDisposable
    {
        internal static Root CurrentRoot;

        static Root()
        {
            //ThreadHelper.ThrowIfNotOnUIThread(nameof(Root));

            CurrentRoot = new Root(
                "Configuration.xml"
                );

            CurrentRoot.BindAll(
                );

            CurrentRoot.AsyncStart(
                );
        }


        private readonly StandardKernel _kernel;
        private readonly string _pathToXmlConfigurationFile;

        private long _disposed = 0L;

        //private EnvDTE.DTE _dte; //do not remove! https://social.msdn.microsoft.com/Forums/en-US/eb6cc3eb-422a-48b1-86da-7a81d3edbddc/events-not-captured-afte-a-window-is-opened?forum=vsx    Your solution events aren't firing because the objects are getting collected
        private DTEEvents _dteEvents;


        public IKernel Kernel
        {
            get
            {
                return
                    _kernel;
            }
        }

        public Root(
            string pathToXmlConfigurationFile
            )
        {
            if (pathToXmlConfigurationFile == null)
            {
                throw new ArgumentNullException(nameof(pathToXmlConfigurationFile));
            }

            _pathToXmlConfigurationFile = pathToXmlConfigurationFile;

            _kernel = new StandardKernel();
        }

        public void BindAll(
            )
        {
            var defmod = new DefaultModule(
                _pathToXmlConfigurationFile
                );
            _kernel.Load(defmod);

            var childmod = new ChildModule(
                );
            _kernel.Load(childmod);


            ThreadHelper.ThrowIfNotOnUIThread(nameof(BindAll));

            _dteEvents = ((Events2)defmod._dte.Events).DTEEvents;
            _dteEvents.OnBeginShutdown += DTEEvents_OnBeginShutdown;
            //((Events2)_dte.Events).WindowEvents.WindowClosing += WindowEvents_WindowClosing;

            //bind to solution events
            var solution = _kernel.Get<IVsSolution>();
            var sEventsExt = _kernel.GetAll<IVsSolutionEventsExt>();
            foreach (var sEventExt in sEventsExt)
            {
                uint cookie;
                solution.AdviseSolutionEvents(sEventExt, out cookie);

                sEventExt.Cookie = cookie;
            }

        }

        public void AsyncStart()
        {
            var validator = _kernel.Get<SqlInclusionCacheBackgroundValidator>();
            validator.AsyncStart();

            var extractor = _kernel.Get<ITimeoutTagExtractor>();
            extractor.AsyncStart();

            var flsContainer = _kernel.Get<IFullyLoadedStatusContainer>();
            flsContainer.AsyncStart();
        }


        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1L) != 0L)
            {
                return;
            }

            ThreadHelper.ThrowIfNotOnUIThread(nameof(Root.Dispose));

            //in reverse order!
            var flsContainer = _kernel.Get<IFullyLoadedStatusContainer>();
            flsContainer.SyncStop();

            var extractor = _kernel.Get<ITimeoutTagExtractor>();
            extractor.SyncStop();

            var validator = _kernel.Get<SqlInclusionCacheBackgroundValidator>();
            validator.SyncStop();

            //unbind from solution events
            var solution = _kernel.Get<IVsSolution>();
            var sEventsExt = _kernel.GetAll<IVsSolutionEventsExt>();
            foreach (var sEventExt in sEventsExt)
            {
                solution.UnadviseSolutionEvents(sEventExt.Cookie);
            }

            //kill the kernel
            _kernel.Dispose();
        }

        private void DTEEvents_OnBeginShutdown()
        {
            Root.CurrentRoot.Dispose();
        }
    }

}
