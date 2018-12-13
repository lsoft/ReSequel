using Ninject;
using Ninject.Modules;
using Main;
using System;
using Main.Sql;
using TestConsole.TaskRelated;
using Main.Sql.SqlServer.Executor;

namespace TestConsole.CompositionRoot
{
    public class SqlServerExecutorModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IConnectionStringContainer>()
                .To<ConstantConnectionStringContainer>()
                .InSingletonScope()
                .WithConstructorArgument(
                    "connectionString",
                    c => c.Kernel.Get<WorkingTaskSqlExecutor>().ConnectionString
                    )
                ;


            Bind<ISqlExecutorFactory>()
                .To<SqlServerExecutorFactory>()
                //not a singleton
                .When(request =>
                {
                    var result = StringComparer.InvariantCultureIgnoreCase.Compare(
                         request.ParentContext.Kernel.Get<WorkingTaskSqlExecutor>().Type,
                         nameof(SqlExecutorTypeEnum.SqlServer)
                         );

                    return result == 0;
                })
                ;
        }
    }

}
