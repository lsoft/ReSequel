
using Main.Sql.Identifier;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Main.Inclusion.Carved.Result
{
    public class ComplexCarveResult : ICarveResult
    {
        private readonly List<ICarveResult> _results;

        private readonly List<ITableName> _tableList;
        private readonly List<ITableName> _tempTableList;
        private readonly List<ITableName> _tableVariableList;

        private bool _isStarReferenced;
        private readonly List<IColumnName> _columnList;



        public IReadOnlyCollection<ICarveResult> Results => _results;

        public IReadOnlyList<ITableName> TableList => _tableList;

        public IReadOnlyList<ITableName> TempTableList => _tempTableList;

        public IReadOnlyList<ITableName> TableVariableList => _tableVariableList;

        /// <summary>
        /// did * used in select queries?
        /// </summary>
        public bool IsStarReferenced => _isStarReferenced;

        public IReadOnlyList<IColumnName> ColumnList => _columnList;

        public string TableNames
        {
            get
            {
                if (_tableList.Count == 0)
                {
                    return
                        "No table references";
                }

                return
                    string.Join(" , ", _tableList.Select(j => j.FullTableName).Distinct());
            }
        }

        public string ColumnNames
        {
            get
            {
                if (_columnList.Count == 0)
                {
                    return
                        "No column references";
                }

                return
                    string.Join(" , ", _columnList.Select(j => j.ColumnName).Distinct());
            }
        }



        public ComplexCarveResult(
            )
        {
            _results = new List<ICarveResult>();

            _tableList = new List<ITableName>();
            _tempTableList = new List<ITableName>();
            _tableVariableList = new List<ITableName>();

            _isStarReferenced = false;
            _columnList = new List<IColumnName>();
        }


        public bool IsTableReferenced(string tableName)
        {
            if (tableName == null)
            {
                throw new ArgumentNullException(nameof(tableName));
            }

            return
                _tableList.Any(j => j.IsSame(tableName));
        }

        public bool IsColumnReferenced(string columnName)
        {
            if (columnName == null)
            {
                throw new ArgumentNullException(nameof(columnName));
            }

            return
                _columnList.Any(j => j.IsSame(columnName));
        }


        public void Append(
            ICarveResult result
            )
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            _results.Add(result);

            _tableList.AddRange(result.TableList);
            _tempTableList.AddRange(result.TempTableList);
            _tableVariableList.AddRange(result.TableVariableList);

            _isStarReferenced |= result.IsStarReferenced;
            _columnList.AddRange(result.ColumnList);
        }

    }
}
