using System;
using Main.Inclusion.Validated.Result;
using System.Collections.Generic;
using Main.Inclusion.Found;
using Main.Sql.SqlServer.Executor;
using Main.Validator.UnitProvider;

namespace Main.Sql
{
    public interface ISqlExecutor : IDisposable
    {
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
