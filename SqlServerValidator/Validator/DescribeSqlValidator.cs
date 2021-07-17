using System;
using System.Data.Common;
using System.Diagnostics;
using System.Threading.Tasks;
using Main.Sql;

namespace SqlServerValidator.Validator
{
    public class DescribeSqlValidator :  ISqlValidator
    {
        private const string FullSql = @"
sp_describe_first_result_set @tsql = N'

{0}

'
";

        private readonly DbConnection _connection;

        public DescribeSqlValidator(
            DbConnection connection
            )
        {
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            _connection = connection;
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
                using (var cmd = _connection.CreateCommand())
                {
                    cmd.CommandText = string.Format(
                        FullSql,
                        innerSql.Replace("'", "''")
                        );
                    cmd.CommandTimeout = 5;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        return (true, string.Empty);
                    }
                }
            }
            catch (Exception excp)
            {
                return (false, excp.Message);
            }
        }
    }
}
