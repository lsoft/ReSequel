using EnvDTE;
using EnvDTE80;
using Extension.Cache;
using Extension.ExtensionStatus.FullyLoaded;
using Extension.Tagging.Extractor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Ninject;
using System;
using System.Threading;
using Extension.CompositionRoot.Modules;
using Main.Other;
using Extension.Helper;

namespace Extension.CompositionRoot
{
    public sealed class Root : IDisposable
    {
        public const string ConfigurationFileName = "Configuration.xml";
        public const string ScanSchemeFileName = "ScanDescription.xml";

        internal static Root CurrentRoot;

        static Root()
        {
            //ThreadHelper.ThrowIfNotOnUIThread(nameof(Root));

            ReflectionHelper.ExtractEmbeddedResource(ConfigurationFileName.GetFullPathToFile(), "Extension." + ConfigurationFileName);

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

            var ssm = new SqlServerModule();
            _kernel.Load(ssm);

            var slm = new SqliteModule();
            _kernel.Load(slm);


            ThreadHelper.ThrowIfNotOnUIThread(nameof(BindAll));

            //Microsoft.VisualStudio.Shell.Events.onbe
            _dteEvents = ((Events2)defmod._dte.Events).DTEEvents;
            _dteEvents.OnBeginShutdown += DTEEvents_OnBeginShutdown;

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
