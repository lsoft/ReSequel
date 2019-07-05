using Main.Inclusion.Validated.Result;
using Main.Sql.SqlServer.Validator.Factory;
using Main.Sql.SqlServer.Visitor;

using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Main.Inclusion.Found;
using Main.Validator.UnitProvider;

namespace Main.Sql.SqlServer.Executor
{
    public class SqlServerExecutor : ISqlExecutor
    {
        private readonly ISqlValidatorFactory _sqlValidatorFactory;
        private readonly SqlConnection _connection;

        private readonly TSql140Parser _parser;

        private int _processedUnits;

        private long _disposed = 0L;
        public int ProcessedUnits => _processedUnits;

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

            _sqlValidatorFactory = sqlValidatorFactory;

            var connection = new SqlConnection(connectionString);
            connection.Open();

            _connection = connection;

            _parser = new TSql140Parser(
                false
                );
        }

        public void Execute(
            IUnitProvider unitProvider
            )
        {
            if (unitProvider == null)
            {
                throw new ArgumentNullException(nameof(unitProvider));
            }

            while(unitProvider.TryRequestNextUnit(out var unit))
            {
                Execute(unit);
            }
        }

        public void Execute(IValidationUnit unit)
        {
            if (unit == null)
            {
                throw new ArgumentNullException(nameof(unit));
            }

            Interlocked.Increment(ref _processedUnits);

            var result = new ComplexValidationResult();

            using (var sql = new StringReader(unit.SqlBody))
            {
                IList<ParseError> errors;
                var parseResult = (TSqlScript) _parser.Parse(sql, out errors);

                if (errors.Count > 0)
                {
                    throw new InvalidOperationException(string.Join(Environment.NewLine,
                        errors.Select(error => string.Format("{0}:{1}: {2}", error.Line, error.Column, error.Message))));
                }

                var validator = _sqlValidatorFactory.Create(_connection);
                var visitor = new StatementVisitor(validator);

                foreach (var batch in parseResult.Batches)
                {
                    foreach (TSqlStatement statement in batch.Statements)
                    {
                        var visitorResult = visitor.ProcessNextStatement(statement);

                        result.Append(visitorResult);
                    }
                }
            }

            unit.SetValidationResult(result);
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1L) != 0L)
            {
                return;
            }

            _connection?.Dispose();
        }
    }

}
