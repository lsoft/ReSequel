using System.Threading.Tasks;

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

        Task<(bool, string)> CheckForConnectionExistsAsync();
    }
}
