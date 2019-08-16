using Extension.Cache;
using Main.Sql;
using Ninject.Modules;
using SqlServerValidator;
using SqlServerValidator.Executor;
using SqlServerValidator.Validator.Factory;

namespace Extension.CompositionRoot.Modules
{
    internal sealed class SqlServerModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IDuplicateProcessor>()
                .To<DuplicateProcessor>()
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
                .WhenInjectedExactlyInto<SqlServerExecutorFactory>()
                .InSingletonScope()
                ;


            Bind<ISqlExecutorFactory>()
                .To<SqlServerExecutorFactory>()
                .WhenInjectedExactlyInto<ChooseSqlExecutorFactory>()
                .InSingletonScope()
                ;

            Bind<SqlInclusionCacheBackgroundValidator>()
                .ToSelf()
                .InSingletonScope()
                ;
        }
    }
}