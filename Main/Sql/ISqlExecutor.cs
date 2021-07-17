using System;
using System.Threading.Tasks;
using Main.Validator.UnitProvider;

namespace Main.Sql
{
    public interface ISqlExecutor : IDisposable
    {
        int ProcessedUnits
        {
            get;
        }

        SqlExecutorTypeEnum Type
        {
            get;
        }

        Task ExecuteAsync(
            IUnitProvider unitProvider
            );

        Task ExecuteAsync(IValidationUnit unit);
    }

}
