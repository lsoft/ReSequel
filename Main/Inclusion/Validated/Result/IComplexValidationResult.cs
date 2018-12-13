using System.Collections.Generic;

namespace Main.Inclusion.Validated.Result
{
    public interface IComplexValidationResult : IValidationResult
    {
        IReadOnlyCollection<IValidationResult> InternalResults
        {
            get;
        }
    }
}
