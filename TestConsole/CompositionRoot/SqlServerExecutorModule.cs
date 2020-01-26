using Ninject;
using Ninject.Modules;
using Main;
using System;
using Main.Sql;
using Main.Sql.ConnectionString;
using SqlServerValidator.Executor;
using TestConsole.TaskRelated;
using SqlServerValidator;

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
                    "executorType",
                    SqlExecutorTypeEnum.Sqlite
                )
                .WithConstructorArgument(
                    "connectionString",
                    c => c.Kernel.Get<WorkingTaskSqlExecutor>().ConnectionString
                    )
               .WithConstructorArgument(
                    "parameters",
                    string.Empty
                )
                ;


            Bind<IDuplicateProcessor>()
                .To<DuplicateProcessor>()
                //not a singleton
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
