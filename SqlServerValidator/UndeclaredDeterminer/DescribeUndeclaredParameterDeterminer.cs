using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;

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

        public bool TryToDetermineParameters(
            string innerSql,
            out IReadOnlyDictionary<string, string> result
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

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var name = (string) reader["name"];
                            var type = (string) reader["suggested_system_type_name"];

                            type = FilterType(type);

                            dict.Add(name, type);
                        }

                    }
                }

                result = dict;
                return
                    true;
            }
            catch (Exception excp)
            {
                Debug.WriteLine(excp.Message);
                Debug.WriteLine(excp.StackTrace);
            }

            result = null;
            return
                false;
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
