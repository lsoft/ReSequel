using System.Data.Common;
using Main.Sql;

namespace SqlServerValidator.Validator.Factory
{
    public class FmtOnlySqlValidatorFactory :  ISqlValidatorFactory
    {
        public ISqlValidator Create(DbConnection connection)
        {
            return
                new FmtOnlySqlValidator(connection);
        }
    }

}
