using System.Data.Common;

namespace Main.Sql
{
    public interface ISqlValidatorFactory
    {
        ISqlValidator Create(
            DbConnection connection
            );
    }

}
