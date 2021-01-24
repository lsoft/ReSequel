namespace Main.Sql
{
    public interface ISqlValidator
    {
        bool TryCalculateRowCount(
            string sql,
            out int rowRead
            );

        bool TryCheckSql(
            string innerSql,
            out string errorMessage
            );
    }
}
