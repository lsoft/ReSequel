using Extension.Tagging.SqlQuery;
using Main.Inclusion.Scanner;
using Main.Inclusion.Validated.Status;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using WpfHelpers;

namespace Extension.Tagging.ValidateButton
{
    public class ValidateButtonViewModel : BaseViewModel
    {
        private readonly ISolutionNameProvider _solutionNameProvider;
        private SqlQueryTag _tag;
        private Brush _defaultForeground;
        
        private Brush _errorMessageForeground;
        private string _errorMessageText;
        private Brush _detailsButtonForeground;
        private string _detailsButtonText;

        public Brush ErrorMessageForeground
        {
            get => _errorMessageForeground;
            set
            {
                _errorMessageForeground = value;

                OnPropertyChanged(nameof(ErrorMessageForeground));
            }
        }

        public string ErrorMessageText
        {
            get => _errorMessageText;
            set
            {
                _errorMessageText = value;
                OnPropertyChanged(nameof(ErrorMessageText));
            }
        }

        public Brush DetailsButtonForeground
        {
            get => _detailsButtonForeground;
            set
            {
                _detailsButtonForeground = value;

                OnPropertyChanged(nameof(DetailsButtonForeground));
            }
        }

        public string DetailsButtonText
        {
            get => _detailsButtonText;
            set
            {
                _detailsButtonText = value;

                OnPropertyChanged(nameof(DetailsButtonText));
            }
        }

        public ICommand MuteEverywhereCommand
        {
            get;
        }

        public ICommand MuteHereCommand
        {
            get;
        }

        public ICommand CopyToClipboardCommand
        {
            get;
        }

        public ValidateButtonViewModel(
            ISolutionNameProvider solutionNameProvider,
            SqlQueryTag tag
            )
        {
            if (solutionNameProvider is null)
            {
                throw new ArgumentNullException(nameof(solutionNameProvider));
            }

            if (tag is null)
            {
                throw new ArgumentNullException(nameof(tag));
            }

            _solutionNameProvider = solutionNameProvider;
            _tag = tag;
            
            _tag.TagStatusEvent += TagStatusChanged;

            _detailsButtonText = "Analysis is in progress...";

            MuteEverywhereCommand = new AsyncRelayCommand(
                async o =>
                {
                    var foundInclusion = _tag.Inclusion.Inclusion;

                    if (!foundInclusion.TryGetDocument(out var document))
                    {
                        return;
                    }

                    var text = await document.GetTextAsync();
                    var lines = text
                        .ToString()
                        .Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                        .ToList()
                        ;

                    var origLine = lines[foundInclusion.Start.Line];

                    var shift = Array.FindIndex(
                        origLine.ToCharArray(),
                        j => !char.IsSeparator(j)
                        );

                    if (shift < 0)
                    {
                        shift = 0;
                    }

                    lines.Insert(
                        foundInclusion.Start.Line,
                        new string(' ', shift) + InclusionScanner.MuteEverywhereComment
                        );
                    var newText = string.Join(Environment.NewLine, lines);

                    var newDocument = document.WithText(SourceText.From(newText));
                    var applied = newDocument.Project.Solution.Workspace.TryApplyChanges(newDocument.Project.Solution);
                },
                o =>
                {
                    return !(_tag?.Inclusion?.Inclusion?.IsMuted ?? true);
                }
                );

            MuteHereCommand = new AsyncRelayCommand(
                async o =>
                {
                    var foundInclusion = _tag.Inclusion.Inclusion;

                    if (!foundInclusion.TryGetDocument(out var document))
                    {
                        return;
                    }

                    var text = await document.GetTextAsync();
                    var lines = text
                        .ToString()
                        .Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                        .ToList()
                        ;

                    var origLine = lines[foundInclusion.Start.Line];

                    var shift = Array.FindIndex(
                        origLine.ToCharArray(),
                        j => !char.IsSeparator(j)
                        );

                    if (shift < 0)
                    {
                        shift = 0;
                    }

                    lines.Insert(
                        foundInclusion.Start.Line,
                        new string(' ', shift) + InclusionScanner.MuteAtComment + "*" + new FileInfo(_solutionNameProvider.SolutionName).Name
                        );
                    var newText = string.Join(Environment.NewLine, lines);

                    var newDocument = document.WithText(SourceText.From(newText));
                    var applied = newDocument.Project.Solution.Workspace.TryApplyChanges(newDocument.Project.Solution);
                }
                ,o =>
                {
                    return !(_tag?.Inclusion?.Inclusion?.IsMuted ?? true);
                }
                );

            
            CopyToClipboardCommand = new RelayCommand(
                o =>
                {
                    Clipboard.SetText(
                        _tag.Inclusion.Inclusion.SqlBody
                        );
                }
                //,o =>
                //{
                //    return !(_tag?.Inclusion?.Inclusion?.IsMuted ?? true);
                //}
                );

            DirectUpdate();
        }

        public void UpdateTag(SqlQueryTag tag)
        {
            if (tag is null)
            {
                throw new ArgumentNullException(nameof(tag));
            }

            var oldTag = Interlocked.Exchange(ref _tag, tag);
            var req = ReferenceEquals(oldTag, tag);
            if (oldTag != null && !req)
            {
                oldTag.TagStatusEvent -= TagStatusChanged;
            }

            if (!req)
            {
                tag.TagStatusEvent += TagStatusChanged;
            }

            DirectUpdate();
            OnPropertyChanged();
        }

        internal void SetDefaultForeground(Brush defaultForeground)
        {
            if (defaultForeground is null)
            {
                throw new ArgumentNullException(nameof(defaultForeground));
            }

            _defaultForeground = defaultForeground;
            _errorMessageForeground = defaultForeground;
            _detailsButtonForeground = defaultForeground;
        }

        private void TagStatusChanged()
        {
            if (_tag == null)
            {
                //it is rare case
                return;
            }

            //ErrorMessageText = DateTime.Now.ToString();
            //DetailsButtonText = DateTime.Now.ToString();

            DirectUpdate();
        }

        private void DirectUpdate()
        {
            if (_tag == null)
            {
                return;
            }

            switch (_tag.Inclusion.Status.Status)
            {
                case ValidationStatusEnum.NotStarted:
                    ProcessNotStarted();
                    break;
                case ValidationStatusEnum.InProgress:
                    ProcessInProgress();
                    break;
                case ValidationStatusEnum.Processed:
                    ProcessProcessed();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            //var now = DateTime.Now.Ticks;
            //var next = now + _border;

            //Interlocked.Exchange(ref _nextRefreshTime, next);
            //Interlocked.Exchange(ref _refreshIsInProgress, 0);
        }

        private void ProcessNotStarted()
        {
            ErrorMessageForeground = _defaultForeground;

            ErrorMessageText = string.Format(
                "{1}{0}{0}{2}",
                Environment.NewLine,
                "Analysis has not started yet...",
                _tag.Inclusion.Inclusion.SqlBody
            );

            DetailsButtonText = "Not started yet...";
            DetailsButtonForeground = _defaultForeground;
        }

        private void ProcessInProgress()
        {
            ErrorMessageForeground = _defaultForeground;

            ErrorMessageText = string.Format(
                "{0}/{1} {2}{3}{3}{4}",
                _tag.Inclusion.Status.ProcessedCount,
                _tag.Inclusion.Status.TotalCount,
                "Analysis is in progress...",
                Environment.NewLine,
                _tag.Inclusion.Inclusion.SqlBody
                );

            DetailsButtonText = string.Format(
                "{0}/{1} Waiting for validation...",
                _tag.Inclusion.Status.ProcessedCount,
                _tag.Inclusion.Status.TotalCount
                );
            DetailsButtonForeground = _defaultForeground;
        }

        private void ProcessProcessed()
        {
            if (_tag == null || _tag.Inclusion == null || _tag.Inclusion.Status == null)
            {
                return;
            }

            if (_tag.Inclusion.Status.IsSuccess)
            {
                ProcessSuccess();
            }
            else
            {
                ProcessNotSuccess();
            }
        }

        private void ProcessNotSuccess()
        {
            ErrorMessageForeground = Brushes.Red;

            if (_tag.Inclusion.Status.Result is null)
            {
                //this happened few times for unknown reason
                ErrorMessageText =
                    string.Format(
                        "{1}{0}{0}{2}",
                        Environment.NewLine,
                        "Unknown error",
                        _tag.Inclusion.Inclusion.SqlBody);
            }
            else if (_tag.Inclusion.Status.Result.CarveResult != null)
            {
                ErrorMessageText =
                    string.Format(
                        "{1}{0}{0}{2}{0}{3}{0}{0}{4}",
                        Environment.NewLine,
                        _tag.Inclusion.Status.Result.WarningOrErrorMessage,
                        _tag.Inclusion.Status.Result.CarveResult.TableNames,
                        _tag.Inclusion.Status.Result.CarveResult.ColumnNames,
                        _tag.Inclusion.Inclusion.SqlBody);
            }
            else
            {
                ErrorMessageText =
                    string.Format(
                        "{1}{0}{0}{2}",
                        Environment.NewLine,
                        _tag.Inclusion.Status.Result.WarningOrErrorMessage,
                        _tag.Inclusion.Inclusion.SqlBody);
            }

            if (_tag.Inclusion.Inclusion.IsMuted)
            {
                DetailsButtonText = "MUTED";
                DetailsButtonForeground = _defaultForeground;
            }
            else
            {
                DetailsButtonText = "FAIL (click for details)";
                DetailsButtonForeground = Brushes.Red;
            }
        }

        private void ProcessSuccess()
        {
            ErrorMessageForeground = Brushes.Green;

            if (_tag.Inclusion.Status.Result.CarveResult != null)
            {
                ErrorMessageText = string.Format(
                    "{1}{0}{0}{2}{0}{3}{0}{0}{4}",
                    Environment.NewLine,
                    "No problem found",
                    _tag.Inclusion.Status.Result.CarveResult.TableNames,
                    _tag.Inclusion.Status.Result.CarveResult.ColumnNames,
                    _tag.Inclusion.Inclusion.SqlBody);
            }
            else
            {
                ErrorMessageText = string.Format(
                    "{1}{0}{0}{2}",
                    Environment.NewLine,
                    "No problem found",
                    _tag.Inclusion.Inclusion.SqlBody);
            }

            if (_tag.Inclusion.Inclusion.IsMuted)
            {
                DetailsButtonText = "MUTED";
                DetailsButtonForeground = _defaultForeground;
            }
            else
            {
                DetailsButtonText = "SUCCESS";
                DetailsButtonForeground = Brushes.Green;
            }
        }



    }
}
