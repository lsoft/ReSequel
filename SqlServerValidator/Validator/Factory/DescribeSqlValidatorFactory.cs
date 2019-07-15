using System.Data.Common;
using Main.Sql;

namespace SqlServerValidator.Validator.Factory
{
    public class DescribeSqlValidatorFactory :  ISqlValidatorFactory
    {
        public ISqlValidator Create(DbConnection connection)
        {
            return
                new DescribeSqlValidator(connection);
        }
    }

}
