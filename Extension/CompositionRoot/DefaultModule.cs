using EnvDTE;
using Extension.Cache;
using Extension.ConfigurationRelated;
using Extension.ExtensionStatus;
using Main.Other;
using Extension.Tagging;
using Extension.Tagging.Extractor;
using Main;
using Main.Helper;
using Main.Inclusion.Scanner;
using Main.Logger;
using Main.ScanRelated;
using Main.SolutionValidator;
using Main.Sql;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Ninject;
using Ninject.Extensions.Factory;
using Ninject.Modules;

using System;
using System.IO;
using Extension.ExtensionStatus.FullyLoaded;

namespace Extension.CompositionRoot
{
    public class DefaultModule : NinjectModule
    {
        private readonly string _pathToXmlConfigurationFile;


        internal EnvDTE.DTE _dte; //do not remove! https://social.msdn.microsoft.com/Forums/en-US/eb6cc3eb-422a-48b1-86da-7a81d3edbddc/events-not-captured-afte-a-window-is-opened?forum=vsx    Your solution events aren't firing because the objects are getting collected



        public DefaultModule(
            string pathToXmlConfigurationFile
            )
        {
            if (pathToXmlConfigurationFile == null)
            {
                throw new ArgumentNullException(nameof(pathToXmlConfigurationFile));
            }

            _pathToXmlConfigurationFile = pathToXmlConfigurationFile;
        }

        public override void Load()
        {
            Bind<ILastMessageProcessLogger, IProcessLogger>()
                .To<LastMessageProcessLogger>()
                .InSingletonScope()
                .WithConstructorArgument(
                    "discret",
                    25
                    )
                ;

            Bind<ConfigurationFilePath>()
                .ToSelf()
                .InSingletonScope()
                .WithConstructorArgument(
                    "filePath",
                    _pathToXmlConfigurationFile
                    )
                ;

            Bind<IConfigurationProvider, IConnectionStringContainer>()
                .To<ConfigurationProvider>()
                .InSingletonScope()
                ;

            Bind<Scan>()
                .ToMethod(
                    c =>
                    {
                        var configurationProvider = c.Kernel.Get<IConfigurationProvider>();

                        ConfigurationRelated.Configuration configuration;
                        if (!configurationProvider.TryRead(out configuration))
                        {
                            throw new InvalidOperationException("Cannot read configuration file");
                        }

                        var filePath =  configuration.ScanScheme.GetFullPathToFile();

                        var scan = filePath.ReadXml<Scan>();

                        return
                            scan;
                    })
                .InTransientScope()
                ;


            Bind<IInclusionScannerFactory>()
                .To<InclusionScannerFactory>()
                .InSingletonScope()
                .WithConstructorArgument(
                    "scanFunc",
                    c => new Func<Scan>(() => c.Kernel.Get<Scan>())
                    )
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

            Bind<IVsSolutionEventsExt, SqlInclusionCache>()
                .To<SqlInclusionCache>()
                .InSingletonScope()
                ;

            Bind<IVsSolutionEventsExt, ITimeoutTagExtractor, ITagExtractor>()
                .To<TimeoutTagExtractor>()
                .InSingletonScope()
                .WithConstructorArgument(
                    "timeout",
                    TimeSpan.FromSeconds(3)
                    )
                ;






            ThreadHelper.ThrowIfNotOnUIThread(nameof(DefaultModule));

            var solution = AsyncPackage.GetGlobalService(typeof(SVsSolution)) as IVsSolution;

            Bind<IVsSolution>()
                .ToConstant(solution)
                .InSingletonScope()
                ;


            Bind<IExtensionStatus, IVsSolutionEventsExt>()
                .To<ExtensionStatusContainer>()
                .InSingletonScope()
                ;


            //_kernel
            //    .Bind<MessageBox>()
            //    .ToSelf()
            //    .InSingletonScope()
            //    ;

            _dte = AsyncPackage.GetGlobalService(typeof(EnvDTE.DTE)) as EnvDTE.DTE;

            Bind<EnvDTE.DTE>()
                .ToConstant(_dte)
                .InSingletonScope()
                ;

            Bind<IFullyLoadedStatusContainer, IFullyLoadedStatusProvider>()
                .To<FullyLoadedStatusContainer>()
                .InSingletonScope()
                ;
        }
    }
}
