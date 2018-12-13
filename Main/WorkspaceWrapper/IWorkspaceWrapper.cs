using Main.Other;
using Main.ScanRelated;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Main.WorkspaceWrapper
{
    public interface IWorkspaceWrapper : IDisposable
    {
        Workspace Workspace
        {
            get;
        }

        Task<List<ProjectArtifact>> CompileAsync(
            bool fixEncodingIssue
            );


        //ProjectArtifact GetArtifact(
        //    string projectName
        //    );
    }
}
