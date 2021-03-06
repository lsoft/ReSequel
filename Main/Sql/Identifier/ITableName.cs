namespace Main.Sql.Identifier
{

    public interface ITableName
    {
        string FullTableName
        {
            get;
        }

        bool IsRegularTable
        {
            get;
        }

        bool IsTempTable
        {
            get;
        }

        bool IsTableVariable
        {
            get;
        }

        bool IsCte
        {
            get;
        }

        bool IsSame(
            string otherTableName
            );
    }
}
