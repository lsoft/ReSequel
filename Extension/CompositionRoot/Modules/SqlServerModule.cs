using System;
using Extension.Cache;
using Main.Sql;
using Main.Validator;
using Ninject;
using Ninject.Modules;
using SqlServerValidator.Executor;
using SqlServerValidator.Validator.Factory;

namespace Extension.CompositionRoot
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