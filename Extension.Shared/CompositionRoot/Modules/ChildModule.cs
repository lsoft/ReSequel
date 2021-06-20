using Main.Other;
using Main.Progress;
using Main.Sql;
using Main.Validator;
using Main.WorkspaceWrapper;
using Ninject.Modules;

namespace Extension.CompositionRoot.Modules
{
    internal sealed class ChildModule : NinjectModule
    {
        public override void Load()
        {
            Bind<ValidationProgressFactory>()
                .ToSelf()
                .InSingletonScope()
                ;

            Bind<ValidationProgress>()
                .ToSelf()
                //not a singleton scope
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

            Bind<ISqlExecutorFactory>()
               .To<ChooseSqlExecutorFactory>()
               .When(req =>
                {
                    if (req?.Target?.Member?.DeclaringType == null)
                    {
                        return true;
                    }

                    var result = (req.Target.Member.DeclaringType) != typeof(ChooseSqlExecutorFactory);

                    return result;
                })
               .InSingletonScope()
                ;
        }
    }
}
