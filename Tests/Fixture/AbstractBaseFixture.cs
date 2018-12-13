using Microsoft.Build.Locator;
using System.Data.SqlClient;

namespace Tests.Fixture
{
    public abstract class AbstractBaseFixture
    {
        //static AbstractBaseFixture()
        //{
        //    MSBuildLocator.RegisterDefaults();
        //}

        protected static SqlConnection OpenConnection(
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
