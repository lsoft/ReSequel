using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Main.Inclusion.Validated.Result;
using Main.Sql;
using Main.Validator.UnitProvider;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServerValidator.Visitor;

namespace SqlServerValidator.Executor
{
    public class SqlServerExecutor : ISqlExecutor
    {
        private readonly ISqlValidatorFactory _sqlValidatorFactory;
        private readonly IDuplicateProcessor _duplicateProcessor;
        private readonly SqlConnection _connection;

        private readonly TSql150Parser _parser;

        private int _processedUnits;

        private long _disposed = 0L;

        public int ProcessedUnits => _processedUnits;

        public static int AliveConnectionCount = 0;

        public SqlExecutorTypeEnum Type => SqlExecutorTypeEnum.SqlServer;

        public SqlServerExecutor(
            string connectionString,
            ISqlValidatorFactory sqlValidatorFactory,
            IDuplicateProcessor duplicateProcessor
            )
        {
            if (duplicateProcessor == null)
            {
                throw new ArgumentNullException(nameof(duplicateProcessor));
            }

            if (connectionString == null)
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            if (sqlValidatorFactory == null)
            {
                throw new ArgumentNullException(nameof(sqlValidatorFactory));
            }

            _sqlValidatorFactory = sqlValidatorFactory;
            _duplicateProcessor = duplicateProcessor;

            var connection = SqlServerHelper.CreateAndConnect(connectionString);


            Interlocked.Increment(ref AliveConnectionCount);

            _connection = connection;

            _parser = new TSql150Parser(
                false
                );
        }

        public async Task ExecuteAsync(
            IUnitProvider unitProvider
            )
        {
            if (unitProvider == null)
            {
                throw new ArgumentNullException(nameof(unitProvider));
            }

            while(unitProvider.TryRequestNextUnit(out var unit))
            {
                await ExecuteAsync(unit);
            }
        }

        public async Task ExecuteAsync(IValidationUnit unit)
        {
            if (unit == null)
            {
                throw new ArgumentNullException(nameof(unit));
            }

            Interlocked.Increment(ref _processedUnits);

            try
            {
                var result = new ComplexValidationResult();

                using (var sql = new StringReader(unit.SqlBody))
                {
                    IList<ParseError> errors;
                    var parseResult = (TSqlScript) _parser.Parse(sql, out errors);

                    if (errors.Count > 0)
                    {
                        throw new InvalidOperationException(
                            string.Join(
                                Environment.NewLine, 
                                errors.Select(error => string.Format("{0}:{1}: {2}", error.Line, error.Column, error.Message))));
                    }

                    var validator = _sqlValidatorFactory.Create(_connection);
                    var visitor = new StatementVisitor(validator, _duplicateProcessor);

                    var statementCount = 0;
                    foreach (var batch in parseResult.Batches)
                    {
                        foreach (TSqlStatement statement in batch.Statements)
                        {
                            var visitorResult = visitor.ProcessNextStatement(statement);

                            result.Append(visitorResult);
                            statementCount++;
                        }
                    }

                    if (statementCount == 0)
                    {
                        result.Append(
                            ValidationResult.Success(unit.SqlBody, unit.SqlBody)
                            );
                    }
                }

                unit.SetValidationResult(result);
            }
            catch (Exception excp)
            {
                unit.SetValidationResult(new ExceptionValidationResult(unit.SqlBody, excp));
            }
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1L) != 0L)
            {
                return;
            }

            _connection?.Dispose();

            Interlocked.Decrement(ref AliveConnectionCount);
        }
    }

}
