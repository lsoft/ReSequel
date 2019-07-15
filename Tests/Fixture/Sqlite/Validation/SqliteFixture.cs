using System.IO;
using Microsoft.Data.Sqlite;
using Tests.CompositionRoot;

namespace Tests.Fixture.Sqlite.Validation
{
    public class SqliteFixture : InfrastructureFixture
    {
        protected static void ClassInit()
        {
            //MSBuildLocator.RegisterDefaults();

            using (var connection = OpenConnection())
            {
                connection.ExecuteBatch(
                    string.Format(
                        Tests.Sqlite.Default.CreateDatabaseScript,
                        Tests.Sqlite.Default.DatabaseFileName
                    )
                );
            }

            Root = new Root();
            Root.BindCommon();
            Root.BindSqlite();
        }

        private static SqliteConnection OpenConnection(
        )
        {
            var pathToDatabase = Tests.Sqlite.Default.PathToDatabaseFile;
            var databaseFileName = Tests.Sqlite.Default.DatabaseFileName;
            var path = Path.Combine(pathToDatabase, databaseFileName);

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            var connection = new SqliteConnection($"Data Source={path};");
            connection.Open();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"PRAGMA case_sensitive_like = false;";
                command.ExecuteNonQuery();
            }

            return
                connection;
        }
    }
}