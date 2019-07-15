using System;
using System.Data.Common;
using System.Text;
using Main.Sql;

namespace SqlServerValidator.Validator
{
    public class FmtOnlySqlValidator :  ISqlValidator
    {
        private const string FullSql = @"
set fmtonly on

{0}

set fmtonly off
";


        private readonly DbConnection _connection;

        public FmtOnlySqlValidator(
            DbConnection connection
            )
        {
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            _connection = connection;
        }


        public bool TryCheckSql(
            string innerSql,
            out string errorMessage
            )
        {
            try
            {
                var declaratinBlock = BuildVariableDeclarationBlock(
                    innerSql
                    );

                using (var cmd = _connection.CreateCommand())
                {
                    cmd.CommandText = string.Format(
                        FullSql,
                        declaratinBlock + Environment.NewLine + innerSql
                        );
                    cmd.CommandTimeout = 5;

                    using (var reader = cmd.ExecuteReader())
                    {
                        errorMessage = string.Empty;
                        return true;
                    }
                }
            }
            catch (Exception excp)
            {
                errorMessage = excp.Message;// + Environment.NewLine + excp.StackTrace;
            }

            return false;
        }

        private string BuildVariableDeclarationBlock(
            string innerSql
            )
        {
            if (innerSql == null)
            {
                throw new ArgumentNullException(nameof(innerSql));
            }

            var result = new StringBuilder();

            using (var command = _connection.CreateCommand())
            {
                var sql = string.Format(
                    @"execute sp_describe_undeclared_parameters @tsql = N'{0}'",
                    innerSql.Replace("'", "''")
                    );

                command.CommandText = sql;
                command.CommandTimeout = 10;

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var name = (string)reader["name"];
                        var type = (string)reader["suggested_system_type_name"];

                        type = FilterType(type);

                        result.AppendLine( string.Format("declare {0} {1}", name, type) );
                    }

                }
            }

            return
                result.ToString();
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
