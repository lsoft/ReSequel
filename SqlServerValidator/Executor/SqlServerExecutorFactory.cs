using System;
using Main.Sql;
using Main.Sql.ConnectionString;

namespace SqlServerValidator.Executor
{
    public class SqlServerExecutorFactory : ISqlExecutorFactory
    {
        private readonly IConnectionStringContainer _connectionStringContainer;
        private readonly ISqlValidatorFactory _sqlValidatorFactory;

        public SqlExecutorTypeEnum Type => SqlExecutorTypeEnum.SqlServer;

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

        public bool CheckForConnectionExists(out string errorMessage)
        {
            try
            {
                using (SqlServerExecutor.CreateAndConnect(_connectionStringContainer.GetConnectionString()))
                {

                }

                errorMessage = string.Empty;
                return true;
            }
            catch (Exception excp)
            {
                errorMessage = excp.Message;
                return false;
            }
        }
    }


}
