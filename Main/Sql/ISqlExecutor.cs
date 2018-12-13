using Main.Inclusion.Validated.Result;
using System.Collections.Generic;

namespace Main.Sql
{
    public interface ISqlExecutor
    {
        IComplexValidationResult Execute(
            string sqlBody
            );
    }

}
