using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;

namespace Main.Inclusion.Found
{
    public interface IFoundSqlInclusion
    {
        CSharpSyntaxNode TargetSyntax
        {
            get;
        }

        FileLinePositionSpan Location
        {
            get;
        }

        string FilePath
        {
            get;
        }

        LinePosition Start
        {
            get;
        }

        LinePosition End
        {
            get;
        }

        string SqlBody
        {
            get;
        }

        bool IsMuted
        {
            get;
        }

        IEnumerable<string> FormattedSqlBodies
        {
            get;
        }

        int GetFormattedQueriesCount();

        bool TryGetDocument(out Document document);
    }
}
