using System;
using System.Data.Common;
using System.Diagnostics;
using System.Threading.Tasks;
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

        public async Task<(bool, int)> TryCalculateRowCountAsync(string sql)
        {
            if (sql is null)
            {
                throw new ArgumentNullException(nameof(sql));
            }

            var rowRead = 0;

            try
            {
                using (var cmd = _connection.CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.CommandTimeout = 5;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            rowRead++;
                        }
                    }

                    return (true, rowRead);
                }
            }
            catch (Exception excp)
            {
                Debug.WriteLine(excp.Message);
                Debug.WriteLine(excp.StackTrace);
            }

            return (false, 0);
        }


        public async Task<(bool, string)> TryCheckSqlAsync(
            string innerSql
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
                    await command.ExecuteNonQueryAsync();
                }

                return (true, string.Empty);
            }
            catch (Exception excp)
            {
                return (false, excp.Message);
            }
        }
    }
}
