using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Main.Inclusion.Carved.Result;
using Main.Sql;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServerValidator.Visitor;

namespace SqlServerValidator.Butcher
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

            var parser = new TSql150Parser(
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
