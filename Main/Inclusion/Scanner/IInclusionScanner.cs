using Main.Inclusion.Found;
using Main.Logger;
using Main.WorkspaceWrapper;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace Main.Inclusion.Scanner
{
    public interface IInclusionScanner
    {
        Task<List<IFoundSqlInclusion>> ScanAsync(
            Microsoft.CodeAnalysis.Document document
            );

        Task<List<IFoundSqlInclusion>> ScanAsync(
            Workspace subjectWorkspace,
            IProcessLogger processLogger
            );
    }

}
