using Main.Sql;
using Ninject.Modules;
using SqliteValidator.Butcher;
using SqliteValidator.Executor;
using SqliteValidator.Validator;

namespace Extension.CompositionRoot.Modules
{
    internal sealed class SqliteModule : NinjectModule
    {
        public override void Load()
        {
            Bind<ISqlValidatorFactory>()
               .To<SqliteValidatorFactory>()
               .WhenInjectedExactlyInto<SqliteExecutorFactory>()
               .InSingletonScope()
                ;

            Bind<ISqlExecutorFactory>()
               .To<SqliteExecutorFactory>()
               .WhenInjectedExactlyInto<ChooseSqlExecutorFactory>()
               .InSingletonScope()
                ;

            Bind<ISqlButcherFactory>()
               .To<SqliteButcherFactory>()
               .InSingletonScope()
                ;
        }
    }
}