
using Main.Sql.Identifier;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
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

        IReadOnlyCollection<IVariableRef> VariableReferenceList
        {
            get;
        }

        /// <summary>
        /// did * used in select queries?
        /// </summary>
        bool IsStarReferenced
        {
            get;
        }

        IReadOnlyList<IColumnName> ColumnList
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

        bool IsTableReferenced(string tableName);

        bool IsColumnReferenced(string columnName);

        bool IsVariableReferenced(string variableName);
    }
}
