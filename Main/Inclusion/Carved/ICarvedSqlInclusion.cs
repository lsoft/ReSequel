using Main.Inclusion.Carved.Result;
using Main.Inclusion.Found;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
