using System.Threading.Tasks;

namespace Main.Sql
{
    public interface ISqlValidator
    {
        Task<(bool, int)> TryCalculateRowCountAsync(
            string sql
            );

        Task<(bool, string)> TryCheckSqlAsync(
            string innerSql
            );
    }
}
