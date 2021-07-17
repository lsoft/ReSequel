using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SqlServerValidator.UndeclaredDeterminer
{
    public class DescribeUndeclaredParameterDeterminer : IUndeclaredParameterDeterminer
    {
        private readonly DbConnection _connection;
        private readonly bool _shouldCleanupConnection;

        public DescribeUndeclaredParameterDeterminer(
            DbConnection connection
            )
        {
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            _connection = connection;
            _shouldCleanupConnection = false;
        }

        public DescribeUndeclaredParameterDeterminer(
            string connectionString
            )
        {
            if (connectionString == null)
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            _connection = SqlServerHelper.CreateAndConnect(connectionString);
            _shouldCleanupConnection = true;
        }

        public async Task<(bool, IReadOnlyDictionary<string, string>)> TryToDetermineParametersAsync(
            string innerSql
            )
        {
            if (innerSql == null)
            {
                throw new ArgumentNullException(nameof(innerSql));
            }

            var dict = new Dictionary<string, string>();

            try
            {
                using (var command = _connection.CreateCommand())
                {
                    var sql = string.Format(@"
execute sp_describe_undeclared_parameters @tsql = N'
{0}
'
",
                        innerSql.Replace("'", "''")
                        );

                    command.CommandText = sql;
                    command.CommandTimeout = 10;

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var name = (string) reader["name"];
                            var type = (string) reader["suggested_system_type_name"];

                            type = FilterType(type);

                            dict.Add(name, type);
                        }

                    }
                }

                return (true, dict);
            }
            catch (Exception excp)
            {
                Debug.WriteLine(excp.Message);
                Debug.WriteLine(excp.StackTrace);
            }

            return (false, null);
        }


        public void Dispose()
        {
            if (_shouldCleanupConnection)
            {
                _connection.Close();
                _connection.Dispose();
            }
        }


        private string FilterType(
            string type
            )
        {
            if (StringComparer.InvariantCultureIgnoreCase.Compare(type, "ntext") == 0)
            {
                return
                    "varchar(10)";
            }
            if (StringComparer.InvariantCultureIgnoreCase.Compare(type, "text") == 0)
            {
                return
                    "varchar(10)";
            }
            if (StringComparer.InvariantCultureIgnoreCase.Compare(type, "image") == 0)
            {
                return
                    "varbinary(10)";
            }

            return
                type;
        }
    }
}
