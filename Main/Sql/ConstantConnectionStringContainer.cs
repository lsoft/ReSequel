using System;

namespace Main.Sql
{
    public class ConstantConnectionStringContainer : IConnectionStringContainer
    {
        private string _connectionString;

        public ConstantConnectionStringContainer(string connectionString)
        {
            if (connectionString == null)
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            _connectionString = connectionString;
        }

        public string GetConnectionString()
        {
            return
                _connectionString;
        }
    }
}