using Main.Inclusion.Found;
using Main.Inclusion.Validated.Result;
using Main.Inclusion.Validated.Status;
using Main.Other;


namespace Main.Inclusion.Validated
{
    public interface IValidatedSqlInclusion
    {
        IFoundSqlInclusion Inclusion
        {
            get;
        }

        IValidationStatus Status
        {
            get;
        }

        bool IsProcessed
        {
            get;
        }

        event InclusionStatusChangedDelegate InclusionStatusEvent;

        void ResetToNotStarted(
            );

        void SetStatusNotStarted();

        void SetStatusInProgress(int processedCount, int totalCount);

        void SetStatusInProgress(int processedCount, int totalCount, IValidationResult result);

        void SetStatusProcessed(IValidationResult result);

        void SetStatusProcessedWithNoResultChanged();

        Report GenerateReport(
            );
        
    }
}
