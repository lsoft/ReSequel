namespace Main.Sql.Identifier
{
    public interface IIndexName
    {
        ITableName ParentTable
        {
            get;
        }

        string IndexName
        {
            get;
        }

        string CombinedIndexName
        {
            get;
        }

        bool IsSame(string tableName, string indexName);
    }
}
