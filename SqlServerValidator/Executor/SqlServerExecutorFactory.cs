using System;
using Main.Sql;
using Main.Sql.ConnectionString;

namespace SqlServerValidator.Executor
{
    public class SqlServerExecutorFactory : ISqlExecutorFactory
    {
        private readonly IConnectionStringContainer _connectionStringContainer;
        private readonly ISqlValidatorFactory _sqlValidatorFactory;
        private readonly IDuplicateProcessor _duplicateProcessor;

        public SqlExecutorTypeEnum Type => SqlExecutorTypeEnum.SqlServer;

        public SqlServerExecutorFactory(
            IConnectionStringContainer connectionStringContainer,
            ISqlValidatorFactory sqlValidatorFactory,
            IDuplicateProcessor duplicateProcessor
            )
        {
            if (duplicateProcessor == null)
            {
                throw new ArgumentNullException(nameof(duplicateProcessor));
            }

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
            _duplicateProcessor = duplicateProcessor;
        }


        public ISqlExecutor Create(
            )
        {
            return
                new SqlServerExecutor(
                    _connectionStringContainer.GetConnectionString(),
                    _sqlValidatorFactory,
                    _duplicateProcessor
                    );
        }

        public bool CheckForConnectionExists(out string errorMessage)
        {
            try
            {
                using (SqlServerHelper.CreateAndConnect(_connectionStringContainer.GetConnectionString()))
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
