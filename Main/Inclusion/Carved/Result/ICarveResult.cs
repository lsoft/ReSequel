
using Main.Sql.Identifier;
using Main.Sql.SqlServer.Visitor;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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
    }
}
