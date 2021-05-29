using SqlServerValidator.Visitor;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;

namespace SqlServerValidator.UndeclaredDeterminer
{
    public class SqlXmlUndeclaredParameterDeterminer : IUndeclaredParameterDeterminer
    {
        private readonly DbConnection _connection;
        private readonly bool _shouldCleanupConnection;

        public SqlXmlUndeclaredParameterDeterminer(
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

        public SqlXmlUndeclaredParameterDeterminer(
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
                var sqlXmlVariableExtractor = new SqlXmlVariableExtractor();
                foreach (var variableName in sqlXmlVariableExtractor.ExtractNames(innerSql))
                {
                    dict.Add(variableName, "int");
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
