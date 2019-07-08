using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Inclusion.Validated;
using Main.Inclusion.Validated.Result;

namespace Main.Validator.UnitProvider.Bag
{
    public interface IInclusionBag
    {
        IValidatedSqlInclusion Inclusion
        {
            get;
        }

        IValidationResult Result
        {
            get;
        }

        int TotalResultReceived
        {
            get;
        }

        void SetValidationResult(IValidationResult result);

        void FixResultIntoInclusion(bool prematurelyStopped);
    }
}
