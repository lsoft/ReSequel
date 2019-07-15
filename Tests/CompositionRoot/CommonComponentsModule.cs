using Ninject.Modules;
using Main.WorkspaceWrapper;
using Main.Logger;
using Main.Progress;
using Main.Other;
using Main.Validator;

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
        }
    }


}
