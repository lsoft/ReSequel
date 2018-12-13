using Main.Inclusion.Carved.Result;
using Main.Inclusion.Found;
using System;
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

    public class CarvedSqlInclusion : ICarvedSqlInclusion
    {
        public IFoundSqlInclusion Inclusion
        {
            get;
        }

        public ICarveResult Result
        {
            get;
        }

        public CarvedSqlInclusion(
            IFoundSqlInclusion inclusion,
            ICarveResult result
            )
        {
            if (inclusion == null)
            {
                throw new ArgumentNullException(nameof(inclusion));
            }

            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            Inclusion = inclusion;
            Result = result;
        }
    }

}
