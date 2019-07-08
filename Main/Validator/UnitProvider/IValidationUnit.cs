using Main.Inclusion.Validated;
using Main.Inclusion.Validated.Result;

namespace Main.Validator.UnitProvider
{
    public interface IValidationUnit
    {
        IValidatedSqlInclusion Inclusion
        {
            get;
        }

        string SqlBody
        {
            get;
        }

        IValidationResult Result
        {
            get;
        }

        void SetValidationResult(IValidationResult result);
    }
}
