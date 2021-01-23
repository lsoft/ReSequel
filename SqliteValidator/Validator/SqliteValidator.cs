using System;
using System.Data.Common;
using System.Diagnostics;
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

        public bool TryCalculateRowCount(string sql, out int rowRead)
        {
            if (sql is null)
            {
                throw new ArgumentNullException(nameof(sql));
            }

            rowRead = 0;

            try
            {
                using (var cmd = _connection.CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.CommandTimeout = 5;

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            rowRead++;
                        }
                    }

                    return true;
                }
            }
            catch (Exception excp)
            {
                Debug.WriteLine(excp.Message);
                Debug.WriteLine(excp.StackTrace);
            }

            rowRead = 0;
            return false;
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
