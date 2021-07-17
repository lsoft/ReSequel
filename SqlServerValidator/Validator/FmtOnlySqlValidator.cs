using System;
using System.Data.Common;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Main.Sql;
using SqlServerValidator.UndeclaredDeterminer;

namespace SqlServerValidator.Validator
{
    public class FmtOnlySqlValidator :  ISqlValidator
    {
        private const string FullSql = @"
set fmtonly on

{0}

set fmtonly off
";
        private readonly IUndeclaredParameterDeterminerFactory _undeclaredParameterDeterminerFactory;
        private readonly DbConnection _connection;

        public FmtOnlySqlValidator(
            IUndeclaredParameterDeterminerFactory undeclaredParameterDeterminerFactory,
            DbConnection connection
            )
        {
            if (undeclaredParameterDeterminerFactory is null)
            {
                throw new ArgumentNullException(nameof(undeclaredParameterDeterminerFactory));
            }

            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }
            _undeclaredParameterDeterminerFactory = undeclaredParameterDeterminerFactory;
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
                var declarationBlock = await BuildVariableDeclarationBlockAsync(
                    innerSql
                    );

                using (var cmd = _connection.CreateCommand())
                {
                    cmd.CommandText = string.Format(
                        FullSql,
                        declarationBlock + Environment.NewLine + innerSql
                        );
                    cmd.CommandTimeout = 5;

                    using (var reader = cmd.ExecuteReader())
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

        private async Task<string> BuildVariableDeclarationBlockAsync(
            string innerSql
            )
        {
            if (innerSql == null)
            {
                throw new ArgumentNullException(nameof(innerSql));
            }

            var result = new StringBuilder();

            using (var determiner = _undeclaredParameterDeterminerFactory.Create(_connection))
            {
                var dr = await determiner.TryToDetermineParametersAsync(innerSql);
                if (dr.Item1)
                {
                    foreach (var pair in dr.Item2)
                    {
                        var name = pair.Key;
                        var type = pair.Value;

                        result.AppendLine(string.Format("declare {0} {1}", name, type));
                    }
                }
            }

            return
                result.ToString();
        }

    }
}
