using System;
using System.Data.Common;
using System.Diagnostics;
using System.Text;
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
                var declarationBlock = BuildVariableDeclarationBlock(
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
                        errorMessage = string.Empty;
                        return true;
                    }

                }
            }
            catch (Exception excp)
            {
                errorMessage = excp.Message;
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

            using (var determiner = _undeclaredParameterDeterminerFactory.Create(_connection))
            {
                if (determiner.TryToDetermineParameters(innerSql, out var dict))
                {
                    foreach (var pair in dict)
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
