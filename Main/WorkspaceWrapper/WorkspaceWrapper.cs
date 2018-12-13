using System.Linq;
using Microsoft.CodeAnalysis;
using System.Diagnostics;
using System.Threading;
using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.FindSymbols;

using System.Collections.Immutable;
using Main.Other;
using Microsoft.CodeAnalysis.MSBuild;
using Main.ScanRelated;
using Main.Helper;
using System.Threading.Tasks;

namespace Main.WorkspaceWrapper
{
    public sealed class WorkspaceWrapper : IWorkspaceWrapper
    {
        private long _disposed = 0L;
        private readonly Compiler _compiler;

        //private List<ProjectArtifact> _artifacts;

        public Workspace Workspace
        {
            get;
        }

        public WorkspaceWrapper(
            Compiler compiler,
            Workspace workspace
            )
        {
            if (compiler == null)
            {
                throw new ArgumentNullException(nameof(compiler));
            }

            if (workspace == null)
            {
                throw new System.ArgumentNullException(nameof(workspace));
            }

            _compiler = compiler;
            Workspace = workspace;
        }

        public async Task<List<ProjectArtifact>> CompileAsync(
            bool fixEncodingIssue
            )
        {
            var artifacts = await _compiler.CompileSolutionAsync(
                Workspace.CurrentSolution,
                fixEncodingIssue
                );

            return
                artifacts;

            //if (_artifacts == null)
            //{
            //    _artifacts = await _compiler.CompileSolutionAsync(
            //        Workspace.CurrentSolution,
            //        fixEncodingIssue
            //        );
            //}

            //return
            //    _artifacts;
        }

        //public ProjectArtifact GetArtifact(
        //    string projectName
        //    )
        //{
        //    var artifact = Artifacts.FirstOrDefault(ar => string.Compare(ar.Project.Name, projectName, StringComparison.InvariantCultureIgnoreCase) == 0);

        //    return
        //        artifact;
        //}

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1L) != 0L)
            {
                return;
            }

            Workspace.Dispose();
        }

    }

}
