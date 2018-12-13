namespace Main.Sql.SqlServer.Validator
{
    public interface ISqlValidator
    {
        bool TryCheckSql(
            string innerSql,
            out string errorMessage
            );
    }
}
