using Main.Inclusion.Validated.Result;
using Main.Sql.SqlServer.Validator.Factory;
using Main.Sql.SqlServer.Visitor;

using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Inclusion.Found;

namespace Main.Sql.SqlServer.Executor
{
    public class SqlServerExecutor :  ISqlExecutor
    {
        private readonly ISqlValidatorFactory _sqlValidatorFactory;

        public string ConnectionString
        {
            get;
        }

        public SqlServerExecutor(
            string connectionString,
            ISqlValidatorFactory sqlValidatorFactory
            )
        {
            if (connectionString == null)
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            if (sqlValidatorFactory == null)
            {
                throw new ArgumentNullException(nameof(sqlValidatorFactory));
            }

            ConnectionString = connectionString;
            _sqlValidatorFactory = sqlValidatorFactory;
        }

        public IEnumerable<IComplexValidationResult> Execute(
            IFoundSqlInclusion inclusion
            )
        {
            if (inclusion == null)
            {
                throw new ArgumentNullException(nameof(inclusion));
            }

            var index = 0;
            var total = inclusion.GetFormattedQueriesCount();
            var before = DateTime.Now;

            var parser = new TSql140Parser(
                false
                );

            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                foreach (var sqlBody in inclusion.FormattedSqlBodies)
                {
                    var result = new ComplexValidationResult();

                    var validator = _sqlValidatorFactory.Create(connection);


                    var visitor = new StatementVisitor(validator);

                    using (var sql = new StringReader(sqlBody))
                    {
                        IList<ParseError> errors;
                        var parseResult = (TSqlScript) parser.Parse(sql, out errors);

                        if (errors.Count > 0)
                        {
                            throw new InvalidOperationException(
                                string.Join(Environment.NewLine,
                                errors.Select(error => string.Format("{0}:{1}: {2}", error.Line, error.Column, error.Message)))
                                );
                        }

                        foreach (var batch in parseResult.Batches)
                        {
                            foreach (TSqlStatement statement in batch.Statements)
                            {
                                var visitorResult = visitor.ProcessNextStatement(statement);

                                result.Append(visitorResult);
                            }
                        }
                    }

                    yield return result;
                    index++;
                }
            }

            var after = DateTime.Now;
            Debug.WriteLine("Inclusion validation checks {0} variants, takes {1}", index, (after - before));
        }
    }

}
