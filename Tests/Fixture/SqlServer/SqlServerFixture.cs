using System.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tests.CompositionRoot;

namespace Tests.Fixture.SqlServer
{
    public class SqlServerFixture : InfrastructureFixture
    {

        protected static void ClassInit()
        {
            //MSBuildLocator.RegisterDefaults();

            using (var connection = OpenConnection("master"))
            {
                connection.ExecuteBatch(
                    string.Format(
                        TestSettings.Default.CreateDatabaseScript,
                        TestSettings.Default.DatabaseName
                    )
                );
            }

            Root = new Root();
            Root.BindCommon();
            Root.BindSqlServer();
        }

        private static SqlConnection OpenConnection(
            string databaseName = null
            )
        {
            databaseName = databaseName ?? TestSettings.Default.DatabaseName;

            var connection = new SqlConnection(string.Format(TestSettings.Default.ConnectionString, databaseName));

            connection.Open();

            return
                connection;
        }

    }
}
