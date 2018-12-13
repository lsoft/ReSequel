using Main.Sql.SqlServer.Executor;
using Main.Sql.SqlServer.Validator.Factory;

using System;
using System.Reflection;

namespace Main.Sql.SqlServer.Executor
{
    public class SqlServerExecutorFactory : ISqlExecutorFactory
    {
        private readonly IConnectionStringContainer _connectionStringContainer;
        private readonly ISqlValidatorFactory _sqlValidatorFactory;

        public SqlServerExecutorFactory(
            IConnectionStringContainer connectionStringContainer,
            ISqlValidatorFactory sqlValidatorFactory
            )
        {
            if (connectionStringContainer == null)
            {
                throw new ArgumentNullException(nameof(connectionStringContainer));
            }

            if (sqlValidatorFactory == null)
            {
                throw new ArgumentNullException(nameof(sqlValidatorFactory));
            }

            _connectionStringContainer = connectionStringContainer;
            _sqlValidatorFactory = sqlValidatorFactory;
        }

        public ISqlExecutor Create(
            )
        {
            return
                new SqlServerExecutor(
                    _connectionStringContainer.GetConnectionString(),
                    _sqlValidatorFactory
                    );
        }
    }


}
