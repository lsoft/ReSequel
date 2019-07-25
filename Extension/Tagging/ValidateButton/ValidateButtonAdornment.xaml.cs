
using Extension.Tagging.SqlQuery;
using Main.Inclusion.Found;
using Main.Inclusion.Scanner;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Shell;
using Ninject;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Main.Inclusion.Validated;
using Main.Inclusion.Validated.Status;

namespace Extension.Tagging.ValidateButton
{
    /// <summary>
    /// Interaction logic for ValidateButtonAdornment.xaml
    /// </summary>
    public partial class ValidateButtonAdornment : UserControl
    {
        private readonly ISolutionNameProvider _solutionNameProvider;

        private readonly object _locker = new object();

        private SqlQueryTag _tag;
        private TagStatusChangedDelegate _invokeAction;

        private readonly Brush _defaultForeground;

        private readonly long _border = TimeSpan.FromMilliseconds(1000).Ticks; //max 1 refresh per second, no need to refresh faster
        private long _nextRefreshTime = DateTime.Now.Ticks;
        private volatile int _refreshIsInProgress = 0;

        public ValidateButtonAdornment(
            ISolutionNameProvider solutionNameProvider,
            SqlQueryTag tag
            )
        {
            if (solutionNameProvider == null)
            {
                throw new ArgumentNullException(nameof(solutionNameProvider));
            }

            if (tag == null)
            {
                throw new ArgumentNullException(nameof(tag));
            }

            _solutionNameProvider = solutionNameProvider;

            ReplaceTag(tag);

            InitializeComponent();

            _defaultForeground = DetailsButton.Foreground;

            DirectUpdateControl();
        }

        public void UpdateTag(
            SqlQueryTag newTag
            )
        {
            if(newTag == null)
            {
                throw new InvalidOperationException("Incoming tag is lost");
            }

            if(ReferenceEquals(_tag, newTag))
            {
                return;
            }

            ReplaceTag(newTag);

            DirectUpdateControl();
        }

        private void ReplaceTag(SqlQueryTag newTag)
        {
            lock (_locker)
            {
                if (_tag != null)
                {
                    _tag.TagStatusEvent -= _invokeAction;
                }

                _tag = newTag;

                TagStatusChangedDelegate invokeAction = () => TimedUpdateControl(_tag);
                _invokeAction = invokeAction;
                _tag.TagStatusEvent += invokeAction;
            }
        }

        private void TimedUpdateControl(
            SqlQueryTag contextTag
            )
        {
            if (!IsNeedToRefreshInProgressState(contextTag))
            {
                return;
            }

            InvokeUpdateControl(contextTag);
        }

        private bool IsNeedToRefreshInProgressState(
            SqlQueryTag tag
            )
        {
            if (tag == null)
            {
                return true;
            }

            if (tag.Inclusion.Status.Status != ValidationStatusEnum.InProgress)
            {
                return true;
            }

            var now = DateTime.Now.Ticks;
            var next = Interlocked.Read(ref _nextRefreshTime);

            if (now < next)
            {
                return false;
            }

            if (Interlocked.CompareExchange(ref _refreshIsInProgress, 1, 0) == 1)
            {
                return false;
            }

            return true;
        }

        private void InvokeUpdateControl(
            SqlQueryTag contextTag
            )
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(async delegate
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                try
                {
                    TagUpdateControl(contextTag);
                }
                catch (Exception excp)
                {
                    Debug.WriteLine(excp.Message);
                    Debug.WriteLine(excp.StackTrace);
                }
            });
        }

        private void TagUpdateControl(
            SqlQueryTag contextTag
            )
        {
            if (!ReferenceEquals(_tag, contextTag))
            {
                return;
            }

            DirectUpdateControl();
        }

        private void DirectUpdateControl()
        {
            this.MuteEverywhereButton.IsEnabled = !_tag.Inclusion.Inclusion.IsMuted;
            this.MuteHereButton.IsEnabled = !_tag.Inclusion.Inclusion.IsMuted;

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

            var now = DateTime.Now.Ticks;
            var next = now + _border;

            Interlocked.Exchange(ref _nextRefreshTime, next);
            Interlocked.Exchange(ref _refreshIsInProgress, 0);
        }

        private void ProcessNotStarted()
        {
            this.ErrorMessageLabel.Foreground = _defaultForeground;

            this.ErrorMessageLabel.Text = string.Format(
                "{1}{0}{0}{2}",
                Environment.NewLine,
                "Analysis has not started yet...",
                _tag.Inclusion.Inclusion.SqlBody
            );

            this.DetailsButton.Content = "Not started yet...";
            this.DetailsButton.Foreground = _defaultForeground;
        }

        private void ProcessInProgress()
        {
            this.ErrorMessageLabel.Foreground = _defaultForeground;

            this.ErrorMessageLabel.Text = string.Format(
                "{0}/{1} {2}{3}{3}{4}",
                _tag.Inclusion.Status.ProcessedCount,
                _tag.Inclusion.Status.TotalCount,
                "Analysis is in progress...",
                Environment.NewLine,
                _tag.Inclusion.Inclusion.SqlBody
                );

            this.DetailsButton.Content = string.Format(
                "{0}/{1} Waiting for validation...",
                _tag.Inclusion.Status.ProcessedCount,
                _tag.Inclusion.Status.TotalCount
                );
            this.DetailsButton.Foreground = _defaultForeground;
        }

        private void ProcessProcessed()
        {
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
            this.ErrorMessageLabel.Foreground = Brushes.Red;

            if (_tag.Inclusion.Status.Result.CarveResult != null)
            {
                this.ErrorMessageLabel.Text =
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
                this.ErrorMessageLabel.Text =
                    string.Format(
                        "{1}{0}{0}{2}",
                        Environment.NewLine,
                        _tag.Inclusion.Status.Result.WarningOrErrorMessage,
                        _tag.Inclusion.Inclusion.SqlBody);
            }

            if (_tag.Inclusion.Inclusion.IsMuted)
            {
                this.DetailsButton.Content = "MUTED";
                this.DetailsButton.Foreground = _defaultForeground;
            }
            else
            {
                this.DetailsButton.Content = "FAIL (click for details)";
                this.DetailsButton.Foreground = Brushes.Red;
            }
        }

        private void ProcessSuccess()
        {
            this.ErrorMessageLabel.Foreground = Brushes.Green;

            if (_tag.Inclusion.Status.Result.CarveResult != null)
            {
                this.ErrorMessageLabel.Text = string.Format(
                    "{1}{0}{0}{2}{0}{3}{0}{0}{4}",
                    Environment.NewLine,
                    "No problem found",
                    _tag.Inclusion.Status.Result.CarveResult.TableNames,
                    _tag.Inclusion.Status.Result.CarveResult.ColumnNames,
                    _tag.Inclusion.Inclusion.SqlBody);
            }
            else
            {
                this.ErrorMessageLabel.Text = string.Format(
                    "{1}{0}{0}{2}",
                    Environment.NewLine,
                    "No problem found",
                    _tag.Inclusion.Inclusion.SqlBody);
            }

            if (_tag.Inclusion.Inclusion.IsMuted)
            {
                this.DetailsButton.Content = "MUTED";
                this.DetailsButton.Foreground = _defaultForeground;
            }
            else
            {
                this.DetailsButton.Content = "SUCCESS";
                this.DetailsButton.Foreground = Brushes.Green;
            }
        }




        private void DetailsButton_Click(object sender, RoutedEventArgs e)
        {
            DetailsButton.IsOpen = true;
        }

        private void CopyToCliboard_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(
                _tag.Inclusion.Inclusion.SqlBody
                );
        }

        private async void MuteHere_Click(object sender, RoutedEventArgs e)
        {
            var foundInclusion = _tag.Inclusion.Inclusion;

            Document document;
            if (!foundInclusion.TryGetDocument(out document))
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
                new string(' ', shift) + InclusionScanner.MuteAtComment  + "*" + new FileInfo(_solutionNameProvider.SolutionName).Name
                );
            var newText = string.Join(Environment.NewLine, lines);

            var newDocument = document.WithText(SourceText.From(newText));
            var applied = newDocument.Project.Solution.Workspace.TryApplyChanges(newDocument.Project.Solution);
        }

        private async void MuteEverywhere_Click(object sender, RoutedEventArgs e)
        {
            var foundInclusion = _tag.Inclusion.Inclusion;

            Document document;
            if(!foundInclusion.TryGetDocument(out document))
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

            if(shift < 0)
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

            #region код, завернутый сюда, хоть и выглядит Roslyn-style, но не работает корректно в некоторых ситуациях; простой метод выше работает правильно, пусть и остается

            //var editor = await Microsoft.CodeAnalysis.Editing.DocumentEditor.CreateAsync(document);

            //var prefix = new string(' ', foundInclusion.TargetSyntax.GetLocation().GetMappedLineSpan().StartLinePosition.Character);
            //var prefixedComment = prefix + InclusionScanner.MuteComment;

            //var leadings = foundInclusion.TargetSyntax.GetLeadingTrivia();
            ////leadings = leadings.Add(SyntaxFactory.Comment(headedComment));
            //leadings = leadings.Add(SyntaxFactory.Comment(InclusionScanner.MuteComment));
            //leadings = leadings.Add(SyntaxFactory.LineFeed);
            //leadings = leadings.Add(SyntaxFactory.Comment(prefix));
            ////leadings = leadings.AddRange(foundInclusion.TargetSyntax.GetLeadingTrivia());

            ////var formatted = Formatter.Format(
            ////    foundInclusion.TargetSyntax.WithLeadingTrivia(
            ////        SyntaxFactory.Comment(InclusionScanner.MuteComment),
            ////        SyntaxFactory.LineFeed
            ////        ),
            ////    document.Project.Solution.Workspace
            ////    );

            ////var formatted2 = Formatter.Format(
            ////    foundInclusion.TargetSyntax.WithLeadingTrivia(leadings),
            ////    document.Project.Solution.Workspace
            ////    );

            //editor.ReplaceNode(
            //    foundInclusion.TargetSyntax,
            //    foundInclusion.TargetSyntax.WithLeadingTrivia(leadings)
            //    );

            //var newText123 = (await editor.GetChangedDocument().GetTextAsync()).ToString();

            #endregion
        }
    }

}
