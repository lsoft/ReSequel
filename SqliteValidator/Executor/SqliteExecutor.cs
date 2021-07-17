using System;
using System.Threading;
using System.Threading.Tasks;
using Main.Helper;
using Main.Inclusion.Validated.Result;
using Main.Sql;
using Main.Validator.UnitProvider;
using Microsoft.Data.Sqlite;

namespace SqliteValidator.Executor
{
    public class SqliteExecutor : ISqlExecutor
    {
        private readonly string _connectionString;
        private readonly string _password;
        private readonly ISqlValidatorFactory _sqlValidatorFactory;

        private readonly SqliteConnection _connection;

        private int _processedUnits;

        private long _disposed = 0L;

        public int ProcessedUnits => _processedUnits;

        public static int AliveConnectionCount = 0;

        public SqlExecutorTypeEnum Type => SqlExecutorTypeEnum.Sqlite;

        public SqliteExecutor(
            string connectionString,
            string password,
            bool caseSensitive,
            ISqlValidatorFactory sqlValidatorFactory
            )
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _password = password; //password allowed to be null
            _sqlValidatorFactory = sqlValidatorFactory ?? throw new ArgumentNullException(nameof(sqlValidatorFactory));

            var connection = CreateAndConnect(connectionString, password, caseSensitive);

            Interlocked.Increment(ref AliveConnectionCount);

            _connection = connection;
        }

        public static async Task<SqliteConnection> CreateAndConnectAsync(
            string connectionString, 
            string password, 
            bool caseSensitive
            )
        {
            var connection = CreateConnection(connectionString);

            await connection.OpenAsync();

            PrepareConnection(connection, password, caseSensitive);

            return connection;
        }

        public async Task ExecuteAsync(IUnitProvider unitProvider)
        {
            if (unitProvider == null)
            {
                throw new ArgumentNullException(nameof(unitProvider));
            }

            while (unitProvider.TryRequestNextUnit(out var unit))
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

                var statements = unit.SqlBody.SplitBatch();

                var validator = _sqlValidatorFactory.Create(_connection);

                foreach (var statement in statements)
                {
                    var (checkResult, checkErrorMessage) = await validator.TryCheckSqlAsync(statement);
                    if (!checkResult)
                    {
                        throw new Exception(checkErrorMessage);
                    }

                    result.Append(ValidationResult.Success(unit.SqlBody, unit.SqlBody));
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

        private static SqliteConnection CreateAndConnect(
            string connectionString,
            string password,
            bool caseSensitive)
        {
            var connection = CreateConnection(connectionString);
            
            connection.Open();

            PrepareConnection(connection, password, caseSensitive);

            return connection;
        }

        private static SqliteConnection CreateConnection(string connectionString)
        {
            var connection = new SqliteConnection(connectionString);
            return connection;
        }

        private static void PrepareConnection(SqliteConnection connection, string password, bool caseSensitive)
        {
            //set password for encoded database
            if (!string.IsNullOrEmpty(password))
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = string.Format(@"PRAGMA key = '{0}';", password);
                    command.ExecuteNonQuery();
                }
            }

            //set caseSensitive
            if (caseSensitive)
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"PRAGMA case_sensitive_like = true;";
                    command.ExecuteNonQuery();
                }
            }
            else
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"PRAGMA case_sensitive_like = false;";
                    command.ExecuteNonQuery();
                }
            }
        }

    }
}
