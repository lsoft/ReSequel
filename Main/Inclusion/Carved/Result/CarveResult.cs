
using Main.Sql.Identifier;

using System;
using System.Collections.Generic;
using System.Linq;
using Main.Sql.VariableRef;

namespace Main.Inclusion.Carved.Result
{
    public class CarveResult : ICarveResult
    {
        private readonly List<ICarveResult> _results;

        private readonly List<ITableName> _tableList;
        private readonly List<ITableName> _tempTableList;
        private readonly List<ITableName> _tableVariableList;
        private readonly List<IVariableRef> _variableReferenceList;
        private readonly List<ITableName> _cteList;

        private bool _isStarReferenced;
        private readonly List<IColumnName> _columnList;

        private readonly List<IIndexName> _indexList;
        private readonly List<IFunctionName> _functionList;

        public IReadOnlyCollection<ICarveResult> Results => _results;

        public IReadOnlyList<ITableName> TableList => _tableList;

        public IReadOnlyList<ITableName> TempTableList => _tempTableList;

        public IReadOnlyList<ITableName> TableVariableList => _tableVariableList;

        public IReadOnlyList<ITableName> CteList => _cteList;

        public IReadOnlyCollection<IVariableRef> VariableReferenceList => _variableReferenceList;


        /// <summary>
        /// did * used in select queries?
        /// </summary>
        public bool IsStarReferenced => _isStarReferenced;

        public IReadOnlyList<IColumnName> ColumnList => _columnList;

        public IReadOnlyList<IIndexName> IndexList => _indexList;

        public IReadOnlyList<IFunctionName> FunctionList => _functionList;

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

        public string IndexNames
        {
            get
            {
                if (_indexList.Count == 0)
                {
                    return
                        "No index references";
                }

                return
                    string.Join(" , ", _indexList.Select(j => j.CombinedIndexName).Distinct());
            }
        }


        public string FunctionNames
        {
            get
            {
                if (_indexList.Count == 0)
                {
                    return
                        "No function references";
                }

                return
                    string.Join(" , ", _functionList.Select(j => j.FullFunctionName).Distinct());
            }
        }


        public CarveResult(
            )
        {
            _results = new List<ICarveResult>();

            _tableList = new List<ITableName>();
            _tempTableList = new List<ITableName>();
            _tableVariableList = new List<ITableName>();
            _variableReferenceList = new List<IVariableRef>();
            _cteList = new List<ITableName>();

            _isStarReferenced = false;
            _columnList = new List<IColumnName>();

            _indexList = new List<IIndexName>();
            _functionList = new List<IFunctionName>();
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

        public bool IsColumnReferenced(string columnName, bool isAlias = false)
        {
            if (columnName == null)
            {
                throw new ArgumentNullException(nameof(columnName));
            }

            return
                _columnList.Any(j => j.IsSame(columnName, isAlias));
        }

        public bool IsVariableReferenced(string variableName)
        {
            if (variableName == null)
            {
                throw new ArgumentNullException(nameof(variableName));
            }

            return
                _variableReferenceList.Any(j => j.IsSame(variableName));
        }

        public bool IsIndexReferenced(string tableName, string indexName)
        {
            if (tableName is null)
            {
                throw new ArgumentNullException(nameof(tableName));
            }

            if (indexName == null)
            {
                throw new ArgumentNullException(nameof(indexName));
            }

            return
                _indexList.Any(i => i.IsSame(tableName, indexName));
        }

        public bool IsFunctionReferenced(string fullFunctionName)
        {
            if (fullFunctionName is null)
            {
                throw new ArgumentNullException(nameof(fullFunctionName));
            }

            return
                _functionList.Any(i => i.IsSame(fullFunctionName));
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
            _variableReferenceList.AddRange(result.VariableReferenceList);
            _cteList.AddRange(result.CteList);

            _isStarReferenced |= result.IsStarReferenced;
            _columnList.AddRange(result.ColumnList);

            _indexList.AddRange(result.IndexList);
            _functionList.AddRange(result.FunctionList);
        }

    }
}
