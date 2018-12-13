using Main.Sql.SqlServer.Validator.Factory;

using System;
using System.Reflection;

namespace Main.Sql.SqlServer.Butcher
{
    public class SqlServerButcherFactory : ISqlButcherFactory
    {
        public SqlServerButcherFactory(
            )
        {
        }

        public ISqlButcher Create(
            )
        {
            return
                new SqlServerButcher(
                    );
        }
    }

}
