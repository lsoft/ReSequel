using Ninject;
using Ninject.Extensions.Factory;
using Ninject.Modules;
using Ninject.Parameters;
using Main;
using Main.WorkspaceWrapper;
using System.Linq;
using Main.Sql.SqlServer.Validator.Factory;
using Main.Sql;
using Main.Logger;
using Main.Progress;
using Main.Other;
using Main.Validator;
using Main.Sql.SqlServer.Executor;
using Main.Sql.SqlServer.Butcher;

namespace Tests.CompositionRoot
{
    internal sealed class CommonComponentsModule : NinjectModule
    {
        public CommonComponentsModule(
            )
        {
        }



        public override void Load()
        {
            Bind<IProcessLogger>()
                .To<DebugProcessLogger>()
                .InSingletonScope()
                ;

            Bind<ValidationProgressFactory>()
                .ToSelf()
                .InSingletonScope()
                ;

            Bind<ValidationProgress>()
                .ToSelf()
                .InTransientScope()
                ;

            Bind<Compiler>()
                .To<Compiler>()
                .InSingletonScope()
                ;

            Bind<IValidatorFactory>()
                .To<ComplexValidatorFactory>()
                .InSingletonScope()
                ;


            Bind<IWorkspaceFactory>()
                .To<WorkspaceFactory>()
                .InSingletonScope()
                ;



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
                    "connectionString",
                    string.Format(TestSettings.Default.ConnectionString, TestSettings.Default.DatabaseName)
                    )
                ;


            Bind<ISqlExecutorFactory>()
                .To<SqlServerExecutorFactory>()
                .InSingletonScope()
                .WithConstructorArgument(
                    "oneQueryProcessTimeoutInSeconds",
                    5
                    )
                ;

            Bind<ISqlButcherFactory>()
                .To<SqlServerButcherFactory>()
                .InSingletonScope()
                ;
            
        }
    }


}
