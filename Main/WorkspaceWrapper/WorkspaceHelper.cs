using Project = Microsoft.CodeAnalysis.Project;
using Microsoft.CodeAnalysis;
using System.Threading;
using System;

namespace Main.WorkspaceWrapper
{
    public static class WorkspaceHelper
    {
        public static void ApplyChanges(
            this Workspace workspace,
            Project project
            )
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            if (project == null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            while (!workspace.TryApplyChanges(project.Solution))
            {
                Thread.Sleep(1000);
            }

        }

    }


}
