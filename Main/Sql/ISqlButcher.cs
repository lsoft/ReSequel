using Main.Inclusion.Carved.Result;

namespace Main.Sql
{
    public interface ISqlButcher
    {
        ICarveResult Carve(
            string sqlBody
            );
    }

}
