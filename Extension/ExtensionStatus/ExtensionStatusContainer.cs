using EnvDTE;
using Extension.CompositionRoot;
using Extension.ConfigurationRelated;
using Extension.ExtensionStatus.FullyLoaded;
using Main.Helper;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Extension.ExtensionStatus
{
    public class ExtensionStatusContainer : IVsSolutionEventsExt, IExtensionStatus
    {
        private readonly IConfigurationProvider _configurationProvider;
        private readonly IFullyLoadedStatusProvider _flsProvider;
        private readonly DTE _dte;

        private volatile bool _configurationExists;
        private volatile bool _isSolutionExists;
        private volatile bool _solutionNameValid;
        private volatile string _solutionName;

        public bool IsSolutionExists => _isSolutionExists;

        public string SolutionName => _solutionName;

        public bool IsEnabled
        {
            get
            {
                return
                    _configurationExists
                    && _isSolutionExists
                    && _solutionNameValid
                    && _flsProvider.IsSolutionFullyLoaded
                    ;
            }
        }

        public uint Cookie
        {
            get;
            set;
        }

        public ExtensionStatusContainer(
            IConfigurationProvider configurationProvider,
            IFullyLoadedStatusProvider flsProvider,
            EnvDTE.DTE dte
            )
        {
            if (configurationProvider == null)
            {
                throw new ArgumentNullException(nameof(configurationProvider));
            }
            if (flsProvider == null)
            {
                throw new ArgumentNullException(nameof(flsProvider));
            }
            if (dte == null)
            {
                throw new ArgumentNullException(nameof(dte));
            }

            _configurationProvider = configurationProvider;
            _flsProvider = flsProvider;
            _dte = dte;

            _configurationProvider.ConfigurationFileChangedEvent += ProcessStatus;

            ProcessStatus();
        }

        public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
        {
            ProcessStatus();
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
        {
            ProcessStatus();
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
        {
            ProcessStatus();
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
        {
            ProcessStatus();
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
        {
            ProcessStatus();
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
        {
            ProcessStatus();
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        {
            ProcessStatus();
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
        {
            ProcessStatus();
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        public int OnBeforeCloseSolution(object pUnkReserved)
        {
            ProcessStatus();
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        public int OnAfterCloseSolution(object pUnkReserved)
        {
            ProcessStatus();
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        private void ProcessStatus()
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(async delegate
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                try
                {
                    InternalProcessStatus();
                }
                catch (Exception excp)
                {
                    Debug.WriteLine(excp.Message);
                    Debug.WriteLine(excp.StackTrace);
                }
            });

        }
        private void InternalProcessStatus()
        {
            ThreadHelper.ThrowIfNotOnUIThread(nameof(ExtensionStatusContainer.InternalProcessStatus));

            ConfigurationRelated.Configuration configuration;
            if (!_configurationProvider.TryRead(out configuration))
            {
                _configurationExists = false;
                return;
            }

            _configurationExists = true;

            try
            {
                if (_dte.Solution == null || string.IsNullOrEmpty(_dte.Solution.FullName) || _dte.Solution.Projects.Count == 0)
                {
                    _isSolutionExists = false;
                    _solutionName = string.Empty;
                    return;
                }

                _isSolutionExists = true;
                _solutionName = _dte.Solution.FullName;

                foreach (var solution in configuration.Solutions.Solution)
                {
                    var match = Regex.Match(_solutionName, solution.WildCardToRegular());
                    if (match.Success)
                    {
                        _solutionNameValid = true;
                        return;
                    }
                }

                _solutionNameValid = false;
            }
            catch (Exception excp)
            {
                Debug.WriteLine(excp.Message);
                Debug.WriteLine(excp.StackTrace);
            }
        }
    }

}
