using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Main.WorkspaceWrapper
{
    public interface IWorkspaceFactory
    {
        IWorkspaceWrapper Open(
            string pathToSubjectSolution
            );

        IWorkspaceWrapper CreateWorkspace(
            List<PortableExecutableReference> metadataReferences,
            string projectName
            );
    }



}
