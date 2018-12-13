using System;
using Main.WorkspaceWrapper;
using Microsoft.CodeAnalysis.FindSymbols;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Main;
using Main.Inclusion.Scanner;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Main.Helper;
using Main.Inclusion;
using Main.ScanRelated;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Main.Inclusion.Found;
using Main.Inclusion.Butcher;
using Main.Inclusion.Carved;
using Main.Sql;

namespace Main.Inclusion.Scanner
{
    public sealed class InclusionButcher : IInclusionButcher
    {
        private readonly IInclusionScanner _scanner;
        private readonly ISqlButcher _sqlButcher;

        public InclusionButcher(
            IInclusionScanner scanner,
            ISqlButcher sqlButcher
            )
        {
            if (scanner == null)
            {
                throw new ArgumentNullException(nameof(scanner));
            }

            if (sqlButcher == null)
            {
                throw new ArgumentNullException(nameof(sqlButcher));
            }

            _scanner = scanner;
            _sqlButcher = sqlButcher;
        }

        public List<ICarvedSqlInclusion> SearchForColumn(
            Microsoft.CodeAnalysis.Document document,
            string columnName
            )
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            var foundSqlInclusionList = _scanner.Scan(document);

            var result = FilterForColumnName(
                foundSqlInclusionList,
                columnName
                );

            return
                result;
        }

        public List<ICarvedSqlInclusion> SearchForColumn(
            IWorkspaceWrapper subjectWorkspace,
            string columnName
            )
        {
            if (subjectWorkspace == null)
            {
                throw new ArgumentNullException(nameof(subjectWorkspace));
            }

            var foundSqlInclusionList = _scanner.Scan(subjectWorkspace);

            var result = FilterForColumnName(
                foundSqlInclusionList,
                columnName
                );

            return
                result;
        }

        public List<ICarvedSqlInclusion> SearchForTable(
            Microsoft.CodeAnalysis.Document document,
            string tableName
            )
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            var foundSqlInclusionList = _scanner.Scan(document);

            var result = FilterForTableName(
                foundSqlInclusionList,
                tableName
                );

            return
                result;
        }

        public List<ICarvedSqlInclusion> SearchForTable(
            IWorkspaceWrapper subjectWorkspace,
            string tableName
            )
        {
            if (subjectWorkspace == null)
            {
                throw new ArgumentNullException(nameof(subjectWorkspace));
            }

            var foundSqlInclusionList = _scanner.Scan(subjectWorkspace);

            var result = FilterForTableName(
                foundSqlInclusionList,
                tableName
                );

            return
                result;
        }


        private List<ICarvedSqlInclusion> FilterForColumnName(
            IEnumerable<IFoundSqlInclusion> foundSqlInclusionList,
            string columnName
            )
        {
            return
                (from found in foundSqlInclusionList
                 let carveResult = _sqlButcher.Carve(found.SqlBody)
                 where carveResult.IsColumnReferenced(columnName)
                 select (ICarvedSqlInclusion)new CarvedSqlInclusion(found, carveResult)
                 ).ToList();
        }

        private List<ICarvedSqlInclusion> FilterForTableName(
            IEnumerable<IFoundSqlInclusion> foundSqlInclusionList,
            string tableName
            )
        {
            return
                (from found in foundSqlInclusionList
                 let carveResult = _sqlButcher.Carve(found.SqlBody)
                 where carveResult.IsTableReferenced(tableName)
                 select (ICarvedSqlInclusion)new CarvedSqlInclusion(found, carveResult)
                 ).ToList();
        }


    }

}
