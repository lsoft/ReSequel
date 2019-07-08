using Main.Inclusion.Validated.Result;

namespace Main.Inclusion.Validated.Status
{
    public class ValidationStatus : IValidationStatus
    {
        public ValidationStatusEnum Status
        {
            get;
            private set;
        }

        public int ProcessedCount
        {
            get;
            private set;
        }

        public int TotalCount
        {
            get;
            private set;
        }

        public IValidationResult Result
        {
            get;
            private set;
        }

        public bool IsSuccess 
        {
            get
            {
                if (Status != ValidationStatusEnum.Processed)
                {
                    return false;
                }

                return
                    Result?.IsSuccess ?? false;
            }
        }

        public bool IsFailed
        {
            get
            {
                if (Status != ValidationStatusEnum.Processed)
                {
                    return false;
                }

                return
                    Result?.IsFailed ?? false;
            }
        }
        
        private ValidationStatus(ValidationStatusEnum status, IValidationResult result, int processedCount, int totalCount)
        {
            Status = status;
            Result = result;
            ProcessedCount = processedCount;
            TotalCount = totalCount;
        }

        public void ResetToNotStarted()
        {
            Status = ValidationStatusEnum.NotStarted;
            ProcessedCount = 0;
            TotalCount = 0;
            Result = null;
        }

        public static IValidationStatus NotStarted() => new ValidationStatus(ValidationStatusEnum.NotStarted, null, 0, 0);
        public static IValidationStatus InProgress(int processedCount, int totalCount, IValidationResult result) => new ValidationStatus(ValidationStatusEnum.InProgress, result, processedCount, totalCount);
        public static IValidationStatus Processed(IValidationResult result) => new ValidationStatus(ValidationStatusEnum.Processed, result, 0, 0);
    }
}