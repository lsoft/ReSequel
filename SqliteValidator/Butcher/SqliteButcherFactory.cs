using Main.Sql;

namespace SqliteValidator.Butcher
{
    public class SqliteButcherFactory : ISqlButcherFactory
    {
        public ISqlButcher Create()
        {
            return 
                new SqliteButcher();
        }
    }
}