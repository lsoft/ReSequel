using Main.Inclusion.Carved;
using Main.WorkspaceWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main.Inclusion.Butcher
{
    public interface IInclusionButcher
    {
        List<ICarvedSqlInclusion> SearchForColumn(
            Microsoft.CodeAnalysis.Document document,
            string columnName
            );

        List<ICarvedSqlInclusion> SearchForColumn(
            IWorkspaceWrapper subjectWorkspace,
            string columnName
            );

        List<ICarvedSqlInclusion> SearchForTable(
            Microsoft.CodeAnalysis.Document document,
            string tableName
            );

        List<ICarvedSqlInclusion> SearchForTable(
            IWorkspaceWrapper subjectWorkspace,
            string tableName
            );
    }
}
