using Main.Inclusion.Validated.Result;
using System.Collections.Generic;
using Main.Inclusion.Found;

namespace Main.Sql
{
    public interface ISqlExecutor
    {
        IEnumerable<IComplexValidationResult> Execute(
            IFoundSqlInclusion inclusion
            );
    }

}
