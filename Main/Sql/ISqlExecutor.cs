using Main.Inclusion.Validated.Result;
using System.Collections.Generic;

namespace Main.Sql
{
    public interface ISqlExecutor
    {
        IEnumerable<IComplexValidationResult> Execute(
            IEnumerable<string> sqlBodies
            );
    }

}
