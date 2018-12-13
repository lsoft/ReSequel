using Extension.Cache;
using Extension.ConfigurationRelated;
using Microsoft.VisualStudio.Shell;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using WpfHelpers;

namespace Extension.Wpf.ChooseDefaultExecutor
{
    public sealed class ChooseDefaultExecutorViewModel : BaseViewModel
    {
        private readonly IConfigurationProvider _configurationProvider;
        private readonly SqlInclusionCache _cache;
        private Configuration _configuration;
        private ExecutorWrapper _selectedWrapper;
        private ICommand _setDefaultCommand;

        public ObservableCollection2<ExecutorWrapper> ExecutorList
        {
            get;
        }

        public bool ConfigurationExists
        {
            get;
            private set;
        }

        public string ErrorMessage
        {
            get;
            private set;
        }

        public Visibility ControlVisibility
        {
            get
            {
                return
                    ConfigurationExists ? Visibility.Visible : Visibility.Hidden;
            }
        }

        public ExecutorWrapper SelectedWrapper
        {
            get => _selectedWrapper;
            set
            {
                _selectedWrapper = value;

                OnPropertyChanged(string.Empty);
            }
        }


        public ICommand SetDefaultCommand
        {
            get
            {
                if (_setDefaultCommand == null)
                {
                    _setDefaultCommand = new RelayCommand(
                        a =>
                        {
                            foreach (var wrapper in ExecutorList)
                            {
                                wrapper.Executor.IsDefault =
                                    ReferenceEquals(wrapper, SelectedWrapper)
                                    ;
                            }

                            _configurationProvider.Save(
                                _configuration
                                );

                            ReReadConfiguration();

                            _cache.CleanupProcessedStatus();
                        },
                        a =>
                        {
                            return
                                SelectedWrapper != null && !SelectedWrapper.Executor.IsDefault;
                        });
                }

                return
                    _setDefaultCommand;
            }
        }

        public ChooseDefaultExecutorViewModel(
            Dispatcher dispatcher,
            IConfigurationProvider configurationProvider,
            SqlInclusionCache cache
            ) : base(dispatcher)
        {
            if (configurationProvider == null)
            {
                throw new System.ArgumentNullException(nameof(configurationProvider));
            }

            if (cache == null)
            {
                throw new ArgumentNullException(nameof(cache));
            }

            _configurationProvider = configurationProvider;
            _cache = cache;
            ExecutorList = new ObservableCollection2<ExecutorWrapper>();

            ReReadConfiguration();

            configurationProvider.ConfigurationFileChangedEvent += ReReadConfiguration;
        }

        private void ReReadConfiguration(
            )
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(async delegate
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                try
                {
                    DirectReReadConfiguration();
                }
                catch (Exception excp)
                {
                    Debug.WriteLine(excp.Message);
                    Debug.WriteLine(excp.StackTrace);
                }
            });

            //_dispatcher.BeginInvoke(
            //    new Action(
            //        () =>
            //        {
            //            DirectReReadConfiguration();
            //        }
            //    ));
        }

        private void DirectReReadConfiguration()
        {
            ExecutorList.Clear();
            SelectedWrapper = null;

            if (_configurationProvider.TryRead(out _configuration))
            {
                ConfigurationExists = true;
                ErrorMessage = string.Empty;

                foreach (var executor in _configuration.SqlExecutors.SqlExecutor)
                {
                    ExecutorList.Add(
                        new ExecutorWrapper(
                            executor
                            )
                        );
                }
            }
            else
            {
                ConfigurationExists = false;
                ErrorMessage = "Configuration file is corrupted.";
            }

            OnPropertyChanged(string.Empty);
            OnCommandInvalidate();
        }
    }

}