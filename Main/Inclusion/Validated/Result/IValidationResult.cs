using Main.Inclusion.Carved.Result;

namespace Main.Inclusion.Validated.Result
{
    public interface IValidationResult
    {
        string FullSqlBody
        {
            get;
        }

        string CheckedSqlBody
        {
            get;
        }

        ICarveResult CarveResult
        {
            get;
        }

        ValidationResultEnum Result
        {
            get;
        }

        bool IsSuccess
        {
            get;
        }

        bool IsFailed
        {
            get;
        }

        string WarningOrErrorMessage
        {
            get;
        }
    }


}
