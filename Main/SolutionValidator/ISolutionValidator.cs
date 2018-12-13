using System.Collections.Generic;
using Main.Progress;
using Main.Inclusion.Validated;
using System.Threading.Tasks;

namespace Main.SolutionValidator
{
    public interface ISolutionValidator
    {
        ValidationProgress Progress
        {
            get;
        }

        List<IValidatedSqlInclusion> Execute(
            string pathToSubjectSolution
            );
    }

}
