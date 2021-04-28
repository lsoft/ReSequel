using System.Collections.Generic;
using Main.Progress;
using Main.Inclusion.Validated;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using Microsoft.CodeAnalysis;

namespace Main.SolutionValidator
{
    public interface ISolutionValidator
    {
        ValidationProgress Progress
        {
            get;
        }

        Task<List<IValidatedSqlInclusion>> ExecuteAsync(
            Workspace subjectWorkspace
            );
    }

}
