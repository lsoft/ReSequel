using System;
using System.Threading.Tasks;
using Main.Sql;
using Main.Sql.ConnectionString;

namespace SqliteValidator.Executor
{
    public class SqliteExecutorFactory : ISqlExecutorFactory
    {
        public const string PasswordKey = "Password";
        public const string CaseSensitiveKey = "CaseSensitive";

        private readonly IConnectionStringContainer _connectionStringContainer;
        private readonly ISqlValidatorFactory _sqlValidatorFactory;

        public SqlExecutorTypeEnum Type => SqlExecutorTypeEnum.Sqlite;

        public SqliteExecutorFactory(
            IConnectionStringContainer connectionStringContainer,
            ISqlValidatorFactory sqlValidatorFactory
        )
        {
            if (sqlValidatorFactory == null)
            {
                throw new ArgumentNullException(nameof(sqlValidatorFactory));
            }

            if (sqlValidatorFactory == null)
            {
                throw new ArgumentNullException(nameof(sqlValidatorFactory));
            }

            _connectionStringContainer = connectionStringContainer ?? throw new ArgumentNullException(nameof(connectionStringContainer));
            _sqlValidatorFactory = sqlValidatorFactory;
        }

        public ISqlExecutor Create()
        {
            if (!_connectionStringContainer.TryGetParameter(PasswordKey, out var password))
            {
                throw new InvalidOperationException($"No password parameter provided. Provide parameter with name {PasswordKey} and leave it empty");
            }
            if (!_connectionStringContainer.TryGetParameter(CaseSensitiveKey, out var caseSensitiveString))
            {
                throw new InvalidOperationException($"No case sensitive parameter provided. Provide parameter with name {CaseSensitiveKey} and set its value");
            }
            if (!bool.TryParse(caseSensitiveString, out var caseSensitive))
            {
                throw new InvalidOperationException($"Case sensitive parameter should be True or False");
            }

            return
                new SqliteExecutor(
                    _connectionStringContainer.GetConnectionString(),
                    password,
                    caseSensitive,
                    _sqlValidatorFactory
                );
        }

        public async Task<(bool, string)> CheckForConnectionExistsAsync()
        {
            try
            {
                if (!_connectionStringContainer.TryGetParameter(PasswordKey, out var password))
                {
                    var errorMessage = $"No password parameter provided. Provide parameter with name {PasswordKey} and leave it empty";
                    return (false, errorMessage);
                }
                if (!_connectionStringContainer.TryGetParameter(CaseSensitiveKey, out var caseSensitiveString))
                {
                    var errorMessage = $"No case sensitive parameter provided. Provide parameter with name {CaseSensitiveKey} and set its value";
                    return (false, errorMessage);
                }
                if (!bool.TryParse(caseSensitiveString, out var caseSensitive))
                {
                    var errorMessage = $"Case sensitive parameter should be True or False";
                    return (false, errorMessage);
                }

                using (await SqliteExecutor.CreateAndConnectAsync(_connectionStringContainer.GetConnectionString(), password, caseSensitive))
                {

                }

                return (true, string.Empty);
            }
            catch (Exception excp)
            {
                return (false, excp.Message);
            }
        }

    }
}