using System;
using System.Data.Common;
using Main.Sql;
using Microsoft.Data.Sqlite;

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

                    // temporary workaround this issue: https://github.com/aspnet/EntityFrameworkCore/issues/16647
                    var lCommand = command as SqliteCommand;
                    lCommand.AddDummyParameters();

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
