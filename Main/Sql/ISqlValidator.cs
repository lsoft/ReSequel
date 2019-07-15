namespace Main.Sql
{
    public interface ISqlValidator
    {
        bool TryCheckSql(
            string innerSql,
            out string errorMessage
            );
    }
}
