using Main;
using Main.Sql;
using Main.Sql.ConnectionString;
using Ninject.Modules;
using SqlServerValidator;
using SqlServerValidator.Butcher;
using SqlServerValidator.Executor;
using SqlServerValidator.Validator.Factory;

namespace Tests.CompositionRoot
{
    internal sealed class SqlServerModule : NinjectModule
    {
        public override void Load()
        {
            Bind<DescribeSqlValidatorFactory>()
               .To<DescribeSqlValidatorFactory>()
               .InSingletonScope()
                ;

            Bind<FmtOnlySqlValidatorFactory>()
               .To<FmtOnlySqlValidatorFactory>()
               .InSingletonScope()
                ;

            Bind<ISqlValidatorFactory>()
               .To<DetectSqlValidatorFactory>()
               .InSingletonScope()
                ;


            Bind<IConnectionStringContainer>()
               .To<ConstantConnectionStringContainer>()
               .InSingletonScope()
               .WithConstructorArgument(
                    "executorType",
                    SqlExecutorTypeEnum.SqlServer
                )
               .WithConstructorArgument(
                    "connectionString",
                    string.Format(TestSettings.Default.ConnectionString, TestSettings.Default.DatabaseName)
                )
               .WithConstructorArgument(
                    "parameters",
                    string.Empty
                )
                ;


            Bind<ISqlExecutorFactory>()
               .To<SqlServerExecutorFactory>()
               .InSingletonScope()
                //.WithConstructorArgument(
                //     "oneQueryProcessTimeoutInSeconds",
                //     5
                // )
                ;

            Bind<ISqlButcherFactory>()
               .To<SqlServerButcherFactory>()
               .InSingletonScope()
                ;
        }
    }
}