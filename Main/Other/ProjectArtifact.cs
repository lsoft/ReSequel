using System;
using Project = Microsoft.CodeAnalysis.Project;
using Compilation = Microsoft.CodeAnalysis.Compilation;

namespace Main.Other
{
    public sealed class ProjectArtifact
    {
        public Project Project
        {
            get;
        }

        public Compilation Compilation
        {
            get;
        }

        public ProjectArtifact(Project project, Compilation compilation)
        {
            if (project == null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            if (compilation == null)
            {
                throw new ArgumentNullException(nameof(compilation));
            }

            Project = project;
            Compilation = compilation;
        }
    }


}
