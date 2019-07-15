using System;

namespace Main.Sql.ConnectionString
{
    public class ConstantConnectionStringContainer : IConnectionStringContainer
    {
        private readonly string _connectionString;
        private readonly string _parameters;

        public SqlExecutorTypeEnum ExecutorType
        {
            get;
        }

        public ConstantConnectionStringContainer(SqlExecutorTypeEnum executorType, string connectionString, string parameters)
        {
            ExecutorType = executorType;
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        }


        public string GetConnectionString()
        {
            return
                _connectionString;
        }

        public bool TryGetParameter(string parameterName, out string parameterValue)
        {
            if (string.IsNullOrEmpty(_parameters))
            {
                parameterValue = string.Empty;
                return false;
            }

            var pairs = _parameters.Split(';');
            foreach (var pair in pairs)
            {
                var parts = pair.Split('=');
                if (parts.Length == 2)
                {
                    if (parts[0] == parameterName)
                    {
                        parameterValue = parts[1];
                        return true;
                    }
                }
            }

            parameterValue = string.Empty;
            return false;
        }
    }
}