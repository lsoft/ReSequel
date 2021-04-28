namespace Main.Sql
{
    public interface ISqlExecutorFactory
    {
        SqlExecutorTypeEnum Type
        {
            get;
        }

        ISqlExecutor Create(
            );

        bool CheckForConnectionExists(out string errorMessage);
    }
}
