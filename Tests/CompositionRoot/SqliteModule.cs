using System.IO;
using Main.Sql;
using Main.Sql.ConnectionString;
using Ninject.Modules;
using SqliteValidator;
using SqliteValidator.Butcher;
using SqliteValidator.Executor;
using SqliteValidator.Validator;

namespace Tests.CompositionRoot
{
    internal sealed class SqliteModule : NinjectModule
    {
        public override void Load()
        {
            Bind<ISqlValidatorFactory>()
               .To<SqliteValidatorFactory>()
               .InSingletonScope()
                ;


            var pathToDatabase = Tests.Sqlite.Default.PathToDatabaseFile;
            var databaseFileName = Tests.Sqlite.Default.DatabaseFileName;
            var connectionString = $"Data Source={Path.Combine(pathToDatabase, databaseFileName)};";


            Bind<IConnectionStringContainer>()
               .To<ConstantConnectionStringContainer>()
               .InSingletonScope()
               .WithConstructorArgument(
                    "executorType",
                    SqlExecutorTypeEnum.Sqlite
                )
               .WithConstructorArgument(
                    "connectionString",
                    connectionString
                    )
               .WithConstructorArgument(
                    "parameters",
                    $"{SqliteExecutorFactory.PasswordKey}=;{SqliteExecutorFactory.CaseSensitiveKey}=False"
                    )
                ;




            Bind<ISqlExecutorFactory>()
               .To<SqliteExecutorFactory>()
               .InSingletonScope()
               //.WithConstructorArgument(
               //     "connectionString",
               //     connectionString
               // )
               //.WithConstructorArgument(
               //     "password",
               //     string.Empty
               // )
               //.WithConstructorArgument(
               //     "caseSensitive",
               //     false
               // )
                ;

            Bind<ISqlButcherFactory>()
               .To<SqliteButcherFactory>()
               .InSingletonScope()
                ;
        }
    }
}