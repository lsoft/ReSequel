using Main.Inclusion.Validated.Result;

namespace Main.Inclusion.Validated.Status
{
    public interface IValidationStatus
    {
        ValidationStatusEnum Status
        { 
            get;
        }

        int ProcessedCount
        {
            get;
        }

        int TotalCount
        {
            get;
        }

        IValidationResult Result
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

        void ResetToNotStarted();

    }
}