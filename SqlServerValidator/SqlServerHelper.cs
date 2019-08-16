using System;
using System.Data.SqlClient;
using SqlServerValidator.Visitor;

namespace SqlServerValidator
{
    public static class SqlServerHelper
    {
        public static SqlConnection CreateAndConnect(string connectionString)
        {
            var connection = new SqlConnection(connectionString);
            connection.Open();

            return connection;
        }

        public static bool IsItTableVariable(
            this string objectName
            )
        {
            if (objectName == null)
            {
                throw new ArgumentNullException(nameof(objectName));
            }

            if (objectName.StartsWith(StatementVisitor.SqlServerVariablePrefix))
            {
                return true;
            }
            //if (objectName.StartsWith(StatementVisitor.SqlServerSpecificVariablePrefix))
            //{
            //    return true;
            //}

            return
                false;
        }

        public static bool IsItTempTable(
            this string objectName
            )
        {
            if (objectName == null)
            {
                throw new ArgumentNullException(nameof(objectName));
            }

            if (objectName.StartsWith(StatementVisitor.SqlServerTempTablePrefix1))
            {
                return true;
            }
            if (objectName.StartsWith(StatementVisitor.SqlServerTempTablePrefix2))
            {
                return true;
            }

            return
                false;
        }

    }
}