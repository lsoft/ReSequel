using Main.Inclusion.Carved.Result;

namespace Main.Inclusion.Validated.Result
{
    public interface IModifiedValidationResult : IValidationResult
    {
        IModifiedValidationResult WithNewFullSqlBody(
            string fullSqlBody
            );

        IModifiedValidationResult WithCarveResult(
            ICarveResult carveResult
            );
    }


}
