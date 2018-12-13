using Main.Inclusion.Carved.Result;
using Main.Sql.SqlServer.Visitor;

using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main.Sql.SqlServer.Butcher
{
    public class SqlServerButcher : ISqlButcher
    {
        public ICarveResult Carve(
            string sqlBody
            )
        {
            if (sqlBody == null)
            {
                throw new ArgumentNullException(nameof(sqlBody));
            }

            var parser = new TSql140Parser(
                false
                );

            var visitor = new ButcherVisitor(
                );

            using (var sql = new StringReader(sqlBody))
            {
                IList<ParseError> errors;
                var parseResult = (TSqlScript)parser.Parse(sql, out errors);

                if (errors.Count > 0)
                {
                    throw new InvalidOperationException(
                        string.Join(
                            Environment.NewLine,
                            errors.Select(error => string.Format("{0}:{1}: {2}", error.Line, error.Column, error.Message))
                            )
                        );
                }

                foreach (var batch in parseResult.Batches)
                {
                    foreach (var statement in batch.Statements)
                    {
                        statement.Accept(visitor);
                    }
                }
            }

            return
                visitor;

        }
    }
}
