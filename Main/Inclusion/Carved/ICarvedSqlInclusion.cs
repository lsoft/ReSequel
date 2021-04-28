using Main.Inclusion.Carved.Result;
using Main.Inclusion.Found;

namespace Main.Inclusion.Carved
{
    public interface ICarvedSqlInclusion
    {
        IFoundSqlInclusion Inclusion
        {
            get;
        }

        ICarveResult Result
        {
            get;
        }
    }
}
