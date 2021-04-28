using System;
using Main.Validator.UnitProvider;

namespace Main.Sql
{
    public interface ISqlExecutor : IDisposable
    {
        SqlExecutorTypeEnum Type
        {
            get;
        }

        void Execute(
            IUnitProvider unitProvider
            );

        void Execute(IValidationUnit unit);

        int ProcessedUnits
        {
            get;
        }
    }

}
