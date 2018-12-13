using Main.Inclusion;
using Main.Inclusion.Found;
using Main.Logger;
using Main.WorkspaceWrapper;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Main.Inclusion.Scanner
{
    public interface IInclusionScanner
    {
        List<IFoundSqlInclusion> Scan(
            Microsoft.CodeAnalysis.Document document
            );

        List<IFoundSqlInclusion> Scan(
            IWorkspaceWrapper subjectWorkspace,
            IProcessLogger processLogger
            );
    }

}
