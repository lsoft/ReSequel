using System;
using System.Data.Common;
using Main.Sql;

namespace SqliteValidator.Validator
{
    public class SqliteValidator : ISqlValidator
    {
        private readonly DbConnection _connection;

        public SqliteValidator(
            DbConnection connection
            )
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        public bool TryCheckSql(
            string innerSql, 
            out string errorMessage
            )
        {
            try
            {
                var mstatement = "EXPLAIN " + innerSql;

                using (var command = _connection.CreateCommand())
                {
                    command.CommandText = mstatement;
                    command.CommandTimeout = 5;
                    command.ExecuteNonQuery();
                }

                errorMessage = string.Empty;

                return true;
            }
            catch (Exception excp)
            {
                errorMessage = excp.Message;
            }

            return false;
        }
    }
}
