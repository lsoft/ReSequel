
using Extension.Tagging.SqlQuery;
using Main.Inclusion.Found;
using Main.Inclusion.Scanner;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Shell;
using Ninject;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Extension.Tagging.ValidateButton
{
    /// <summary>
    /// Interaction logic for ValidateButtonAdornment.xaml
    /// </summary>
    public partial class ValidateButtonAdornment : UserControl
    {
        private readonly object _locker = new object();

        private SqlQueryTag _tag;
        private readonly Brush _defaultForeground;

        public ValidateButtonAdornment(
            SqlQueryTag tag
            )
        {
            if (tag == null)
            {
                throw new ArgumentNullException(nameof(tag));
            }

            _tag = tag;

            InitializeComponent();

            _defaultForeground = DetailsButton.Foreground;

            DirectUpdateControl();
        }

        public void UpdateTag(
            SqlQueryTag tag
            )
        {
            if(tag==null)
            {
                throw new InvalidOperationException("Incoming tag is lost");
            }

            if(ReferenceEquals(_tag, tag))
            {
                return;
            }

            _tag.TagStatusEvent -= UpdateControl;
            _tag = tag;
            _tag.TagStatusEvent += UpdateControl;

            UpdateControl();
        }

        private void UpdateControl(
            )
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(async delegate
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                try
                {
                    DirectUpdateControl();
                }
                catch (Exception excp)
                {
                    Debug.WriteLine(excp.Message);
                    Debug.WriteLine(excp.StackTrace);
                }
            });
        }

        private void DirectUpdateControl(
            )
        {
            lock (_locker)
            {
                this.MuteButton.IsEnabled = !_tag.Inclusion.Inclusion.IsMuted;

                if (!_tag.Inclusion.IsProcessed)
                {
                    this.ErrorMessageLabel.Foreground = _defaultForeground;

                    //this.ErrorMessageLabel.Text = "Analysis is in progress..." + Environment.NewLine + Environment.NewLine + _tag.Inclusion.Inclusion.SqlBody;
                    this.ErrorMessageLabel.Text = string.Format(
                        "{1}{0}{0}{2}",
                        Environment.NewLine,
                        "Analysis is in progress...",
                        _tag.Inclusion.Inclusion.SqlBody
                        );

                    this.DetailsButton.Content = "Waiting to validate...";
                    this.DetailsButton.Foreground = _defaultForeground;
                }
                else
                {
                    if (_tag.Inclusion.Result.IsSuccess)
                    {
                        this.ErrorMessageLabel.Foreground = Brushes.Green;

                        if (_tag.Inclusion.Result.CarveResult != null)
                        {
                            this.ErrorMessageLabel.Text = string.Format(
                                "{1}{0}{0}{2}{0}{3}{0}{0}{4}",
                                Environment.NewLine,
                                "No problem found",
                                _tag.Inclusion.Result.CarveResult.TableNames,
                                _tag.Inclusion.Result.CarveResult.ColumnNames,
                                _tag.Inclusion.Inclusion.SqlBody
                                );
                        }
                        else
                        {
                            this.ErrorMessageLabel.Text = string.Format(
                                "{1}{0}{0}{2}",
                                Environment.NewLine,
                                "No problem found",
                                _tag.Inclusion.Inclusion.SqlBody
                                );
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
                    else
                    {
                        this.ErrorMessageLabel.Foreground = Brushes.Red;

                        if (_tag.Inclusion.Result.CarveResult != null)
                        {
                            this.ErrorMessageLabel.Text =
                                string.Format(
                                    "{1}{0}{0}{2}{0}{3}{0}{0}{4}",
                                    Environment.NewLine,
                                    _tag.Inclusion.Result.WarningOrErrorMessage,
                                    _tag.Inclusion.Result.CarveResult.TableNames,
                                    _tag.Inclusion.Result.CarveResult.ColumnNames,
                                    _tag.Inclusion.Inclusion.SqlBody
                                    );
                        }
                        else
                        {
                            this.ErrorMessageLabel.Text =
                                string.Format(
                                    "{1}{0}{0}{2}",
                                    Environment.NewLine,
                                    _tag.Inclusion.Result.WarningOrErrorMessage,
                                    _tag.Inclusion.Inclusion.SqlBody
                                    );
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
                }
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

        private async void Mute_Click(object sender, RoutedEventArgs e)
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
                j => !Char.IsSeparator(j)
                );

            if(shift < 0)
            {
                shift = 0;
            }

            lines.Insert(
                foundInclusion.Start.Line,
                new string(' ', shift) + InclusionScanner.MuteComment
                );
            var newText = string.Join(Environment.NewLine, lines);

            var newDocument = document.WithText(SourceText.From(newText));
            newDocument.Project.Solution.Workspace.TryApplyChanges(newDocument.Project.Solution);
        }
    }

}
