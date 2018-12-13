using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main.Sql
{
    public interface IConnectionStringContainer
    {
        string GetConnectionString();
    }

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
