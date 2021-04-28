using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Main.WorkspaceWrapper
{
    public interface IWorkspaceFactory
    {
        Workspace Open(
            string pathToSubjectSolution
            );

        Workspace CreateWorkspace(
            List<PortableExecutableReference> metadataReferences,
            string projectName
            );
    }



}
