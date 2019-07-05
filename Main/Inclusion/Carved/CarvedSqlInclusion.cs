using System;
using Main.Inclusion.Carved.Result;
using Main.Inclusion.Found;

namespace Main.Inclusion.Carved
{
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