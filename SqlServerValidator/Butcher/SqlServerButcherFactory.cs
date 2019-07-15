using Main;
using Main.Sql;

namespace SqlServerValidator.Butcher
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
