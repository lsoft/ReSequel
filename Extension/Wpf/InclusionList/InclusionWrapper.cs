using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Main.Inclusion.Validated;
using Main.Inclusion.Validated.Status;

namespace Extension.Wpf.InclusionList
{
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