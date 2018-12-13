using System;
using System.Data.SqlClient;

namespace Main.Sql.SqlServer.Validator.Factory
{
    public class FmtOnlySqlValidatorFactory :  ISqlValidatorFactory
    {
        public ISqlValidator Create(SqlConnection connection)
        {
            return
                new FmtOnlySqlValidator(connection);
        }
    }

}
