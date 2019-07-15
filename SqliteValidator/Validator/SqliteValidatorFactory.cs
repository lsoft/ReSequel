using System.Data.Common;
using Main.Sql;

namespace SqliteValidator.Validator
{
    public class SqliteValidatorFactory : ISqlValidatorFactory
    {
        public ISqlValidator Create(DbConnection connection)
        {
            var result = new SqliteValidator(connection);

            return
                result;
        }
    }
}