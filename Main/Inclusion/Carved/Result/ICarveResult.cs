
using Main.Sql.Identifier;
using System.Collections.Generic;
using Main.Sql.VariableRef;

namespace Main.Inclusion.Carved.Result
{
    public interface ICarveResult
    {
        IReadOnlyList<ITableName> TableList
        {
            get;
        }

        IReadOnlyList<ITableName> TempTableList
        {
            get;
        }

        IReadOnlyList<ITableName> TableVariableList
        {
            get;
        }

        IReadOnlyList<ITableName> CteList
        {
            get;
        }

        IReadOnlyList<IColumnName> ColumnList
        {
            get;
        }

        IReadOnlyList<IIndexName> IndexList
        {
            get;
        }

        IReadOnlyList<IFunctionName> FunctionList
        {
            get;
        }

        IReadOnlyCollection<IVariableRef> VariableReferenceList
        {
            get;
        }

        /// <summary>
        /// did * used in the select queries?
        /// </summary>
        bool IsStarReferenced
        {
            get;
        }

        string TableNames
        {
            get;
        }

        string ColumnNames
        {
            get;
        }

        string IndexNames
        {
            get;
        }

        bool IsTableReferenced(string tableName);

        bool IsColumnReferenced(string columnName, bool isAlias = false);

        bool IsVariableReferenced(string variableName);

        bool IsIndexReferenced(string tableName, string indexName);
    }
}
