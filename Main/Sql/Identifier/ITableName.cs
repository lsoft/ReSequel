namespace Main.Sql.Identifier
{

    public interface ITableName
    {
        string FullTableName
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

        bool IsSame(
            string otherTableName
            );
    }
}
