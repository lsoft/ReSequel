
using ProjectId = Microsoft.CodeAnalysis.ProjectId;
using Project = Microsoft.CodeAnalysis.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using Microsoft.CodeAnalysis.MSBuild;
using System.Collections.Generic;
using Main.Other;

namespace Main.WorkspaceWrapper
{
    public class WorkspaceFactory : IWorkspaceFactory
    {

        public WorkspaceFactory(
            )
        {
        }

        public Workspace Open(
            string pathToSubjectSolution
            )
        {
            var workspace = MSBuildWorkspace.Create();
            try
            {
                workspace.WorkspaceFailed += Workspace_WorkspaceFailed;

                var targetSolution = workspace.OpenSolutionAsync(
                    pathToSubjectSolution
                    ).Result;

                return
                    workspace;
            }
            catch
            {
                workspace.Dispose();
                throw;
            }
        }

        private void Workspace_WorkspaceFailed(object sender, WorkspaceDiagnosticEventArgs e)
        {
            if (e.Diagnostic.Kind == WorkspaceDiagnosticKind.Failure)
            {
                throw new InvalidOperationException(
                    string.Format(
                        "This instance of MSBuild cannnot open the workspace: {0} {1}",
                        e.Diagnostic.Kind,
                        e.Diagnostic.Message
                        )
                    );
            }
        }

        public Workspace CreateWorkspace(
            List<PortableExecutableReference> metadataReferences,
            string projectName
            )
        {

            if (metadataReferences == null)
            {
                throw new ArgumentNullException(nameof(metadataReferences));
            }

            if (projectName == null)
            {
                throw new ArgumentNullException(nameof(projectName));
            }

            var workspace = new AdhocWorkspace();
            try
            {
                var projectId = ProjectId.CreateNewId();
                var versionStamp = VersionStamp.Create();

                var projectInfo = ProjectInfo.Create(
                    projectId,
                    versionStamp,
                    projectName,
                    projectName,
                    LanguageNames.CSharp,
                    compilationOptions: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                    );

                Project executorProject = workspace.AddProject(projectInfo);

                executorProject = executorProject.AddMetadataReferences(
                    metadataReferences
                    );

                workspace.ApplyChanges(executorProject);

                return
                    workspace;
            }
            catch
            {
                workspace.Dispose();
                throw;
            }
        }
    }

}
