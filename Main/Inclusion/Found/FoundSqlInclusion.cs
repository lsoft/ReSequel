using System;
using System.Collections.Generic;
using Main.Inclusion.Scanner.Generator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Main.Inclusion.Found
{
    public class FoundSqlInclusion : IFoundSqlInclusion
    {
        private readonly Document _document;
        private readonly Generator _generator;

        public CSharpSyntaxNode TargetSyntax
        {
            get;
        }

        public FileLinePositionSpan Location
        {
            get;
        }

        public string SqlBody
        {
            get;
        }

        public bool IsMuted
        {
            get;
        }

        public string FilePath
        {
            get;
        }

        public LinePosition Start
        {
            get;
        }

        public LinePosition End
        {
            get;
        }


        public IEnumerable<string> FormattedSqlBodies
        {
            get
            {
                if (_generator == null)
                {
                    yield return SqlBody;
                }
                else
                {
                    foreach(var fq in _generator.FormattedQueries)
                    {
                        yield return fq;
                    }
                }
            }
        }


        public FoundSqlInclusion(
            CSharpSyntaxNode targetSyntax,
            string sqlBody,
            bool isMuted
            )
        {
            if (targetSyntax == null)
            {
                throw new ArgumentNullException(nameof(targetSyntax));
            }

            if (sqlBody == null)
            {
                throw new ArgumentNullException(nameof(sqlBody));
            }

            _document = null;
            TargetSyntax = targetSyntax;
            SqlBody = sqlBody;
            IsMuted = isMuted;
            Location = TargetSyntax.GetLocation().GetLineSpan();

            FilePath = Location.Path;
            Start = Location.StartLinePosition;
            End = Location.EndLinePosition;
        }

        public FoundSqlInclusion(
            Document document,
            CSharpSyntaxNode targetSyntax,
            string sqlBody,
            bool isMuted
            )
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (targetSyntax == null)
            {
                throw new ArgumentNullException(nameof(targetSyntax));
            }

            if (sqlBody == null)
            {
                throw new ArgumentNullException(nameof(sqlBody));
            }

            _document = document;
            TargetSyntax = targetSyntax;
            SqlBody = sqlBody;
            IsMuted = isMuted;
            Location = TargetSyntax.GetLocation().GetLineSpan();

            _generator = null;

            FilePath = Location.Path;
            Start = Location.StartLinePosition;
            End = Location.EndLinePosition;
        }

        public FoundSqlInclusion(
            Document document,
            CSharpSyntaxNode targetSyntax,
            Generator generator,
            bool isMuted
            )
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (targetSyntax == null)
            {
                throw new ArgumentNullException(nameof(targetSyntax));
            }

            if (generator == null)
            {
                throw new ArgumentNullException(nameof(generator));
            }


            _document = document;
            TargetSyntax = targetSyntax;
            _generator = generator;
            SqlBody = generator.SqlTemplate;
            IsMuted = isMuted;
            Location = TargetSyntax.GetLocation().GetLineSpan();

            FilePath = Location.Path;
            Start = Location.StartLinePosition;
            End = Location.EndLinePosition;
        }

        public bool TryGetDocument(out Document document)
        {
            document = _document;

            return
                document != null;
        }

    }
}
