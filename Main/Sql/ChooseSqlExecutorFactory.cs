using System;
using System.Linq;
using System.Threading.Tasks;
using Main.Sql.ConnectionString;

namespace Main.Sql
{
    public class ChooseSqlExecutorFactory : ISqlExecutorFactory
    {
        private readonly IConnectionStringContainer _connectionStringContainer;
        private readonly ISqlExecutorFactory[] _factories;

        public SqlExecutorTypeEnum Type
        {
            get
            {
                var factory = GetFactory();

                return
                    factory.Type;
            }
        }

        public ChooseSqlExecutorFactory(
            IConnectionStringContainer connectionStringContainer,
            ISqlExecutorFactory[] factories
            )
        {
            _connectionStringContainer = connectionStringContainer ?? throw new ArgumentNullException(nameof(connectionStringContainer));
            _factories = factories ?? throw new ArgumentNullException(nameof(factories));
        }

        public ISqlExecutor Create()
        {
            var factory = GetFactory();

            var result = factory.Create();

            return result;
        }

        public async Task<(bool, string)> CheckForConnectionExistsAsync()
        {
            var factory = GetFactory();

            var result = await factory.CheckForConnectionExistsAsync();

            return result;
        }

        private ISqlExecutorFactory GetFactory()
        {
            var executorType = _connectionStringContainer.ExecutorType;

            var factory = _factories.First(k => k.Type == executorType);
            return factory;
        }
    }
}