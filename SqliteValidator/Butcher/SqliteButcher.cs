using Main.Inclusion.Carved.Result;
using Main.Sql;

namespace SqliteValidator.Butcher
{
    public class SqliteButcher : ISqlButcher
    {
        public ICarveResult Carve(string sqlBody)
        {
            var result = new ComplexCarveResult();

            //no 'legal' way to parse Sqlite statement

            return result;
        }
    }
}
