using System;
using System.Data.Common;
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


        public bool TryCheckSql(
            string innerSql,
            out string errorMessage
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
    }
}
