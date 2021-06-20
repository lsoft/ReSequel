using Main.Other;
using Main.Logger;
using Main.SolutionValidator;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TaskStatusCenter;
using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using WpfHelpers;
using Extension.Other;
using Microsoft.VisualStudio.Threading;
using Extension.ExtensionStatus;
using Main.Helper;
using Task = System.Threading.Tasks.Task;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ComponentModelHost;

namespace Extension.Wpf.InclusionList
{
    public sealed class InclusionListViewModel : BaseViewModel
    {
        private readonly ISolutionValidatorFactory _solutionValidatorFactory;
        private readonly ILastMessageProcessLogger _processLogger;
        private readonly IExtensionStatus _extensionStatus;
        private InclusionWrapper _selectedInclusion;

        private ICommand _doScanCommand;
        private ICommand _navigateCommand;
        private ICommand _clearResultCommand;
        private ICommand _filterResultCommand;
        private ICommand _openHelpCommand;

        private List<string> _tables;
        private string _tableNames;
        private bool _tableParseResult;

        private List<string> _columns;
        private string _columnNames;
        private bool _columnParseResult;

        private List<EnteredIndexName> _indexes;
        private string _indexNames;
        private bool _indexParseResult;

        private long _scanningIsInProgress;

        private string _searchingMessage;
        private string _errorMessage;

        private List<InclusionWrapper> _foundInclusionList;
        private bool _includeWithStar;
        private bool _showGreen = true;
        private bool _showRed = true;
        private bool _showMuted = true;

        private ConcurrentDictionary<object, int> _filterIndexDictionary = new ConcurrentDictionary<object, int>( /* here should be reference equals! */ );

        public bool ScanningIsInProgress
        {
            get
            {
                return
                    Interlocked.Read(ref _scanningIsInProgress) != 0L;
            }
        }

        public string ScanButtonCaption
        {
            get
            {
                if (ScanningIsInProgress)
                {
                    return
                        "Scanning is in progress...";
                }
                
                return
                    "Scan solution";
            }
        }

        public string InclusionListCaption
        {
            get
            {
                if (ScanningIsInProgress)
                {
                    return
                        "Scanning is in progress...";
                }
                return
                    string.Format(
                        "Total found {0}, filtered {1} SQL queries",
                        _foundInclusionList.Count,
                        _filterIndexDictionary.Count
                        );
            }
        }

        public string SearchingMessage
        {
            get => _searchingMessage;
            private set
            {
                _searchingMessage = value;

                OnPropertyChanged(nameof(SearchingMessage));
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            private set
            {
                if(_errorMessage == value)
                {
                    return;
                }

                _errorMessage = value;

                OnPropertyChanged(nameof(ErrorMessage));
            }
        }

        public ICollectionView InclusionView
        {
            get;
        }

        public string TableNames
        {
            get => _tableNames;
            set
            {
                _tableNames = value;

                _tableParseResult = TrySplitTableNames(out _tables);

                OnPropertyChanged();
            }
        }

        public string ColumnNames
        {
            get => _columnNames;
            set
            {
                _columnNames = value;

                _columnParseResult = TrySplitColumnNames(out _columns);

                OnPropertyChanged();
            }
        }

        public bool IncludeWithStar
        {
            get => _includeWithStar;
            set
            {
                _includeWithStar = value;

                _columnParseResult = TrySplitColumnNames(out _columns);

                OnPropertyChanged();
            }
        }

        public string IndexNames
        {
            get => _indexNames;
            set
            {
                _indexNames = value;

                _indexParseResult = TrySplitIndexNames(out _indexes);

                OnPropertyChanged();
            }
        }

        public bool FilterCheckBoxesEnabled
        {
            get
            {
                return
                    !ScanningIsInProgress;
            }
        }

        public bool ShowGreen
        {
            get => _showGreen;
            set
            {
                _showGreen = value;

                OnFilterUpdate();
            }
        }

        public bool ShowRed
        {
            get => _showRed;
            set
            {
                _showRed = value;

                OnFilterUpdate();
            }
        }

        public bool ShowMuted
        {
            get => _showMuted;
            set
            {
                _showMuted = value;

                OnFilterUpdate();
            }
        }

        public InclusionWrapper SelectedInclusion
        {
            get => _selectedInclusion;
            set
            {
                _selectedInclusion = value;

                OnPropertyChanged();
            }
        }

        public ICommand DoScanCommand
        {
            get
            {
                if (_doScanCommand == null)
                {
                    _doScanCommand = new AsyncRelayCommand(
                        async a =>
                        {
                            _processLogger.ResetCounter();

                            try
                            {
                                if (Interlocked.Exchange(ref _scanningIsInProgress, 1L) != 0L)
                                {
                                    return;
                                }

                                SearchingMessage = "Init scanner...";
                                ErrorMessage = string.Empty;

                                ClearInclusionList();

                                List<InclusionWrapper> wrappers = null;
                                try
                                {
                                    var solutionName = _extensionStatus.SolutionName;

                                    await TaskScheduler.Default;

                                    wrappers = await PerformScanningAsync(solutionName)/*.ConfigureAwait(false)*/;
                                    //wrappers = await System.Threading.Tasks.Task.Run(async () => await PerformScanningAsync(solutionName));

                                    ErrorMessage = string.Empty;
                                }
                                catch (Exception excp)
                                {
                                    ErrorMessage = excp.AggregateMessages();
                                }
                                finally
                                {
                                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                                }

                                SearchingMessage = string.Empty;

                                if (wrappers != null)
                                {
                                    AddToInclusionList(wrappers);
                                }
                            }
                            catch (Exception excp)
                            {
                                ErrorMessage = excp.AggregateMessages();
                            }
                            finally
                            {
                                Interlocked.Exchange(ref _scanningIsInProgress, 0L);

                                OnPropertyChanged();
                            }
                        },
                        a =>
                        {
                            ThreadHelper.ThrowIfNotOnUIThread(nameof(DoScanCommand));

                            //if (_dte.Solution == null)
                            //{
                            //    return false;
                            //}

                            //if (_dte.Solution.Projects.Count == 0)
                            //{
                            //    return false;
                            //}

                            if(!_extensionStatus.IsEnabled)
                            {
                                return false;
                            }

                            if (ScanningIsInProgress)
                            {
                                return false;
                            }

                            return
                                true;
                        }
                        );
                }

                return
                    _doScanCommand;
            }
        }

        public ICommand NavigateCommand
        {
            get
            {
                if (_navigateCommand == null)
                {
                    _navigateCommand = new RelayCommand(
                        a =>
                        {
                            var inclusionWrapper = a as InclusionWrapper;

                            if (inclusionWrapper == null)
                            {
                                return;
                            }

                            ThreadHelper.ThrowIfNotOnUIThread(nameof(InclusionListViewModel.NavigateCommand));

                            VisualStudioHelper.OpenAndNavigate(
                                inclusionWrapper.Inclusion.Inclusion.FilePath,
                                inclusionWrapper.Inclusion.Inclusion.Start.Line,
                                inclusionWrapper.Inclusion.Inclusion.Start.Character,
                                inclusionWrapper.Inclusion.Inclusion.End.Line,
                                inclusionWrapper.Inclusion.Inclusion.End.Character

                                );
                        },
                        a =>
                        {
                            var inclusionWrapper = a as InclusionWrapper;

                            return
                                inclusionWrapper != null;
                        });
                }

                return
                    _navigateCommand;
            }
        }

        public ICommand ClearResultCommand
        {
            get
            {
                if (_clearResultCommand == null)
                {
                    _clearResultCommand = new RelayCommand(
                        a =>
                        {
                            ThreadHelper.ThrowIfNotOnUIThread(nameof(ClearResultCommand));

                            ClearInclusionList();
                        },
                        a => !ScanningIsInProgress
                        );
                }

                return
                    _clearResultCommand;
            }
        }

        public ICommand FilterResultCommand
        {
            get
            {
                if (_filterResultCommand == null)
                {
                    _filterResultCommand = new RelayCommand(
                        a =>
                        {
                            ThreadHelper.ThrowIfNotOnUIThread(nameof(FilterResultCommand));

                            OnFilterUpdate();
                        },
                        a => !ScanningIsInProgress && _foundInclusionList.Count > 0
                        );
                }

                return
                    _filterResultCommand;
            }
        }

        public ICommand OpenHelpCommand
        {
            get
            {
                if (_openHelpCommand == null)
                {
                    _openHelpCommand = new RelayCommand(
                        a =>
                        {
                            var p = Process.Start(
                                "notepad.exe",
                                "SolutionWideScannerReadme.txt".GetFullPathToFile()
                                );
                        }
                        );
                }

                return
                    _openHelpCommand;
            }
        }

        public InclusionListViewModel(
            ISolutionValidatorFactory solutionValidatorFactory,
            ILastMessageProcessLogger processLogger,
            IExtensionStatus extensionStatus
            )
        {
            if (solutionValidatorFactory == null)
            {
                throw new ArgumentNullException(nameof(solutionValidatorFactory));
            }
            if (processLogger == null)
            {
                throw new ArgumentNullException(nameof(processLogger));
            }
            if (extensionStatus == null)
            {
                throw new ArgumentNullException(nameof(extensionStatus));
            }

            _foundInclusionList = new List<InclusionWrapper>();
            
            InclusionView = CollectionViewSource.GetDefaultView(_foundInclusionList);
            InclusionView.Filter = FilterWrapper;

            _solutionValidatorFactory = solutionValidatorFactory;
            _processLogger = processLogger;
            _extensionStatus = extensionStatus;
            processLogger.NewProcessLoggerMessageEvent += ProcessLogger_NewProcessLoggerMessageEvent;
        }

        private void ProcessLogger_NewProcessLoggerMessageEvent(string newMessage)
        {
            if (ScanningIsInProgress)
            {
                SearchingMessage = newMessage;
            }
        }

        private void AddToInclusionList(
            List<InclusionWrapper> wrappers
            )
        {
            _foundInclusionList.AddRange(wrappers);

            OnFilterUpdate();
        }

        private bool FilterWrapper(
            object w
            )
        {
            try
            {
                var wrapper = w as InclusionWrapper;

                if (wrapper == null)
                {
                    return false;
                }

                var inclusion = wrapper.Inclusion;

                if (inclusion.Status.Result == null || inclusion.Status.Result.CarveResult == null)
                {
                    return false;
                }


                if (wrapper.Inclusion.Inclusion.IsMuted)
                {
                    if (!_showMuted)
                    {
                        return false;
                    }
                }
                else
                {
                    if (inclusion.Status.IsSuccess && !_showGreen)
                    {
                        return false;
                    }

                    if (!inclusion.Status.IsSuccess && !_showRed)
                    {
                        return false;
                    }
                }

                if (_tables != null)
                {
                    if (_tables.Any(table => !inclusion.Status.Result.CarveResult.IsTableReferenced(table)))
                    {
                        return false;
                    }
                }

                if (_columns != null)
                {
                    if (_columns.Any(column => !inclusion.Status.Result.CarveResult.IsColumnReferenced(column)))
                    {
                        return false;
                    }
                }

                if (_indexes != null)
                {
                    if (_indexes.Any(index => !inclusion.Status.Result.CarveResult.IsIndexReferenced(index.TableName, index.IndexName)))
                    {
                        return false;
                    }
                }

                return
                    true;
            }
            catch(Exception excp)
            {
                ErrorMessage = excp.Message;
                Debug.WriteLine(excp.Message);
                //Debug.Write(excp.StackTrace);
            }

            return false;
        }

        private void ClearInclusionList()
        {
            SelectedInclusion = null;
            _foundInclusionList.Clear();

            OnFilterUpdate();
        }

        private void OnFilterUpdate()
        {
            InclusionView.Refresh();

            //determine the filtered item indexes
            _filterIndexDictionary.Clear();
            var index = 0;
            foreach(var i in InclusionView)
            {
                _filterIndexDictionary[i] = index;
                index++;
            }

            OnPropertyChanged();
        }

        private async Task<List<InclusionWrapper>> PerformScanningAsync(
            string solutionFullPath
            )
        {
            if (solutionFullPath == null)
            {
                throw new ArgumentNullException(nameof(solutionFullPath));
            }

            var componentModel = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
            var workspace = (Workspace)componentModel.GetService<VisualStudioWorkspace>();

            var solutionValidator = _solutionValidatorFactory.Create(
                );

            var validationInclusionList = await solutionValidator.ExecuteAsync(
                workspace
                )/*.ConfigureAwait(false)*/;

            var wrappers = validationInclusionList
                .ConvertAll(j => new InclusionWrapper(this._filterIndexDictionary, j))
                ;

            return wrappers;
        }

        private bool TrySplitTableNames(out List<string> result)
        {
            if (string.IsNullOrWhiteSpace(_tableNames))
            {
                result = new List<string>();

                return
                    false;
            }

            var parts = _tableNames.Split(' ', ',');

            result = parts
                .Select(j => j.Trim())
                .Where(j => !string.IsNullOrWhiteSpace(j))
                .ToList()
                ;

            return
                result.Count > 0;
        }

        private bool TrySplitColumnNames(out List<string> result)
        {
            result = new List<string>();

            if (!string.IsNullOrWhiteSpace(_columnNames))
            {
                var parts = _columnNames.Split(' ', ',');

                result.AddRange(
                    parts
                        .Select(j => j.Trim())
                        .Where(j => !string.IsNullOrWhiteSpace(j))
                    );
            }

            if (IncludeWithStar)
            {
                if (!result.Contains("*"))
                {
                    result.Add("*");
                }
            }

            return
                result.Count > 0;
        }

        private bool TrySplitIndexNames(out List<EnteredIndexName> result)
        {
            result = new List<EnteredIndexName>();

            if (!string.IsNullOrWhiteSpace(_indexNames))
            {
                var parts = _indexNames.Split(' ', ',');

                var entereds =
                    from part in parts
                    let trimmed = part.Trim()
                    where !string.IsNullOrWhiteSpace(trimmed)
                    let lastDotIndex = trimmed.LastIndexOf('.')
                    let tableName = lastDotIndex > 0 ? trimmed.Substring(0, lastDotIndex) : string.Empty
                    let indexName = lastDotIndex > 0 ? trimmed.Substring(lastDotIndex + 1) : trimmed
                    select new EnteredIndexName(tableName, indexName);

                result.AddRange(entereds);
            }

            return
                result.Count > 0;
        }


        private class EnteredIndexName
        {
            public string TableName
            {
                get;
            }
            public string IndexName
            {
                get;
            }

            public EnteredIndexName(
                string tableName,
                string indexName
                )
            {
                if (tableName is null)
                {
                    throw new ArgumentNullException(nameof(tableName));
                }

                if (indexName is null)
                {
                    throw new ArgumentNullException(nameof(indexName));
                }

                TableName = tableName;
                IndexName = indexName;
            }

        }
    }
}