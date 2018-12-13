using System;
using System.Data.SqlClient;

namespace Main.Sql.SqlServer.Validator.Factory
{
    public class DescribeSqlValidatorFactory :  ISqlValidatorFactory
    {
        public ISqlValidator Create(SqlConnection connection)
        {
            return
                new DescribeSqlValidator(connection);
        }
    }

}
