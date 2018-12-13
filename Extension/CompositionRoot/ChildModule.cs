using Extension.Cache;
using Extension.ConfigurationRelated;
using Extension.Tagging;
using Main;
using Main.Other;
using Main.Sql;
using Main.Sql.SqlServer.Executor;
using Main.Sql.SqlServer.Validator.Factory;
using Main.Progress;
using Main.Validator;
using Main.WorkspaceWrapper;
using Ninject;
using Ninject.Modules;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Main.Helper;

namespace Extension.CompositionRoot
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


            Bind<ISqlExecutorFactory>()
                .To<SqlServerExecutorFactory>()
                .InSingletonScope()
                //.When(request =>
                //{
                //    var result = StringComparer.InvariantCultureIgnoreCase.Compare(
                //         request.ParentContext.Kernel.Get<TaskSqlExecutor>().Type,
                //         nameof(SqlExecutorTypeEnum.SqlServer)
                //         );

                //    return result == 0;
                //})
                ;

            Bind<SqlInclusionCacheBackgroundValidator>()
                .ToSelf()
                .InSingletonScope()
                ;


        }
    }
}
