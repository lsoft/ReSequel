using Extension.Cache;
using Extension.ConfigurationRelated;
using Main.Other;
using Main;
using Main.Inclusion.Found;
using Main.Inclusion.Scanner;
using Main.Inclusion.Validated;
using Main.Logger;
using Main.SolutionValidator;
using Main.WorkspaceWrapper;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using WpfHelpers;
using Extension.Other;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.Threading;
using Extension.ExtensionStatus;
using Main.Helper;
using Main.Inclusion.Validated.Status;

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
                OnCommandInvalidate();
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
                                    //ThreadHelper.ThrowIfNotOnUIThread(nameof(DoScanCommand));

                                    var solutionName = _extensionStatus.SolutionName;

                                    wrappers = await System.Threading.Tasks.Task.Run(() => PerformScanning(solutionName));

                                    ErrorMessage = string.Empty;
                                }
                                catch (Exception excp)
                                {
                                    ErrorMessage = excp.AggregateMessages();
                                }
                                //finally
                                //{
                                //    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                                //}

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
                                OnCommandInvalidate();
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
            Dispatcher dispatcher,
            ISolutionValidatorFactory solutionValidatorFactory,
            ILastMessageProcessLogger processLogger,
            IExtensionStatus extensionStatus
            ) : base(dispatcher)
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

        private List<InclusionWrapper> PerformScanning(
            string solutionFullPath
            )
        {
            if (solutionFullPath == null)
            {
                throw new ArgumentNullException(nameof(solutionFullPath));
            }

            var solutionValidator = _solutionValidatorFactory.Create(
                );

            var validationInclusionList = solutionValidator.Execute(
                solutionFullPath
                );

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
    }

    public class InclusionWrapper : INotifyPropertyChanged
    {
        private readonly IReadOnlyDictionary<object, int> _filterIndexDictionary;

        public event PropertyChangedEventHandler PropertyChanged;

        public IValidatedSqlInclusion Inclusion
        {
            get;
        }

        public int InclusionIndex
        {
            get
            {
                int index;
                if (_filterIndexDictionary.TryGetValue(this, out index))
                {
                    return
                        index;
                }

                return
                    0;
            }
        }

        public string FullPath
        {
            get
            {
                return
                    string.Format(
                        "{0}({1}):",
                        Inclusion.Inclusion.FilePath.Trim(),
                        Inclusion.Inclusion.Start.Line + 1
                        );
            }
        }

        public string FullSql
        {
            get
            {
                return
                    Inclusion.Inclusion.SqlBody.Trim();
            }
        }

        public string ValidationMessage
        {
            get
            {
                if (Inclusion.Status.Result == null || Inclusion.Status.Status != ValidationStatusEnum.Processed)
                {
                    return
                        string.Empty;
                }

                return
                    Inclusion.Status.Result.WarningOrErrorMessage;
            }
        }

        public string TableReferences
        {
            get
            {
                if (Inclusion.Status == null || Inclusion.Status.Status != ValidationStatusEnum.Processed)
                {
                    return
                        string.Empty;
                }

                if (Inclusion.Status.Result.CarveResult.TableList.Count == 0)
                {
                    return
                        Inclusion.Status.Result.CarveResult.TableNames;
                }

                return
                    string.Format(
                        "Table references: {0}",
                        Inclusion.Status.Result.CarveResult.TableNames
                        );
            }
        }

        public string ColumnReferences
        {
            get
            {
                if (Inclusion.Status == null || Inclusion.Status.Status != ValidationStatusEnum.Processed)
                {
                    return
                        string.Empty;
                }

                if (Inclusion.Status.Result.CarveResult.ColumnList.Count == 0)
                {
                    return
                        Inclusion.Status.Result.CarveResult.ColumnNames;
                }

                return
                    string.Format(
                        "Column references: {0}",
                        Inclusion.Status.Result.CarveResult.ColumnNames
                        );
            }
        }

        public Brush Foreground
        {
            get
            {
                if (Inclusion.Inclusion.IsMuted)
                {
                    return
                        Brushes.DarkGray;
                }

                if (Inclusion.Status == null || Inclusion.Status.Status != ValidationStatusEnum.Processed)
                {
                    return
                        Brushes.Yellow;
                }

                if (Inclusion.Status.Result.IsSuccess)
                {
                    return
                        Brushes.Green;
                }

                return
                    Brushes.Red;
            }
        }

        public Visibility FullBodyVisibility
        {
            get;
            private set;
        }

        public string PartialSql
        {
            get;
            private set;
        }

        public InclusionWrapper(
            IReadOnlyDictionary<object, int> filterIndexDictionary,
            IValidatedSqlInclusion inclusion
            )
        {
            if (filterIndexDictionary == null)
            {
                throw new ArgumentNullException(nameof(filterIndexDictionary));
            }

            if (inclusion == null)
            {
                throw new ArgumentNullException(nameof(inclusion));
            }

            _filterIndexDictionary = filterIndexDictionary;

            Inclusion = inclusion;
            inclusion.InclusionStatusEvent += Inclusion_InclusionStatusEvent;

            //working with SQL body

            const int MaxRowCount = 8;

            var rows = inclusion.Inclusion.SqlBody.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            var rowCount = rows.Length;
            var isSqlHuge = rowCount > MaxRowCount;

            FullBodyVisibility = isSqlHuge ? Visibility.Visible : Visibility.Collapsed;

            PartialSql = string.Join(Environment.NewLine, rows.Take(MaxRowCount));

            if (isSqlHuge)
            {
                PartialSql += Environment.NewLine + "...";
            }
        }

        private void Inclusion_InclusionStatusEvent()
        {
            OnPropertyChanged(nameof(Foreground));
            OnPropertyChanged(nameof(ValidationMessage));
        }

        /// <summary>
        /// Активация евента изменения бинденого свойства
        /// </summary>
        /// <param name="propertyName"></param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }


    }

}