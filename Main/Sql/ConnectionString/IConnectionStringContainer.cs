namespace Main.Sql.ConnectionString
{
    public interface IConnectionStringContainer
    {
        SqlExecutorTypeEnum ExecutorType
        {
            get;
        }

        string GetConnectionString();

        bool TryGetParameter(string parameterName, out string parameterValue);
    }
}
