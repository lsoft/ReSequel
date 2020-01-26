using Ninject.Extensions.Factory;
using Ninject.Modules;
using Ninject.Parameters;
using Main;
using Main.WorkspaceWrapper;
using System;
using System.Linq;
using Main.Inclusion.Scanner;

using TestConsole.TaskRelated;
using Main.Logger;
using Main.Progress;
using Main.Validator;
using Main.Other;
using Main.ScanRelated;
using System.IO;
using Main.Helper;
using Main.SolutionValidator;
using Main.Sql;
using SqlServerValidator.Validator.Factory;

namespace TestConsole.CompositionRoot
{
    internal sealed class CommonComponentsModule : NinjectModule
    {
        private readonly WorkingTask _task;
        private readonly WorkingTaskSqlExecutor _sqlExecutor;
        private readonly string _pathToXmlScanSchema;

        public CommonComponentsModule(
            WorkingTask task
            )
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            _task = task;
            _sqlExecutor = task.SqlExecutor;
            _pathToXmlScanSchema = task.ScanScheme;
        }



        public override void Load()
        {
            Bind<IProcessLogger>()
                .To<ConsoleProcessLogger>()
                .InSingletonScope()
                ;

            Bind<ValidationProgressFactory>()
                .ToSelf()
                .InSingletonScope()
                ;

            Bind<ValidationProgress>()
                .ToSelf()
                //not a singleton scope
                ;

            Bind<WorkingTaskSqlExecutor>()
                .ToConstant(_sqlExecutor)
                ;

            Bind<Compiler>()
                .To<Compiler>()
                .InSingletonScope()
                ;

            Bind<IValidatorFactory>()
                .To<ComplexValidatorFactory>() 
                .InSingletonScope()
                //.WithConstructorArgument(
                //    "batchSize",
                //    BatchSize
                //    )
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

            Bind<ISolutionNameProvider>()
                .To<ConsoleSolutionNameProvider>()
                .WithConstructorArgument(
                    typeof(string),
                    new FileInfo(_task.TargetSolution).Name
                );

            Bind<Scan>()
                .ToMethod(
                    c =>
                    {
                         var filePath = _pathToXmlScanSchema.GetFullPathToFile();

                        var scan = filePath.ReadXml<Scan>();

                        return
                            scan;
                    })
                .InTransientScope()
                ;

            Bind<IInclusionScannerFactory>()
                .To<InclusionScannerFactory>()
                .InSingletonScope()
                ;

            Bind<IInclusionScanner>()
                .To<InclusionScanner>()
                .InTransientScope()
                ;

            Bind<ISolutionValidatorFactory>()
                .To<DefaultSolutionValidatorFactory>()
                .InSingletonScope()
                ;

            Bind<ISolutionValidator>()
                .To<DefaultSolutionValidator>()
                //.InSingletonScope()
                //not a singleton
                ;

        }
    }

    public class ConsoleSolutionNameProvider : ISolutionNameProvider
    {
        public string SolutionName
        {
            get;
        }

        public ConsoleSolutionNameProvider(
            string solutionName
            )
        {
            if (solutionName == null)
            {
                throw new ArgumentNullException(nameof(solutionName));
            }

            SolutionName = solutionName;
        }
    }


}
