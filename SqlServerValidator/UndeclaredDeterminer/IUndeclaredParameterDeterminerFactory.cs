using System.Data.Common;

namespace SqlServerValidator.UndeclaredDeterminer
{
    public interface IUndeclaredParameterDeterminerFactory
    {
        IUndeclaredParameterDeterminer Create(
            string connectionString
            );

        IUndeclaredParameterDeterminer Create(
            DbConnection connection
            );
    }
}
