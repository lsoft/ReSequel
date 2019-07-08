using System;
using Main.Inclusion.Validated;
using Main.Inclusion.Validated.Result;
using Main.Inclusion.Validated.Status;

namespace Main.Validator.UnitProvider.Bag
{
    public class InclusionBag : IInclusionBag
    {
        private readonly object _locker = new object();

        public IValidatedSqlInclusion Inclusion
        {
            get;
        }

        public IValidationResult Result => Inclusion.Status?.Result;

        public int TotalResultReceived
        {
            get;
            private set;
        }

        public InclusionBag(
            IValidatedSqlInclusion inclusion
        )
        {
            if (inclusion == null)
            {
                throw new ArgumentNullException(nameof(inclusion));
            }

            Inclusion = inclusion;
        }

        public void SetValidationResult(IValidationResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            lock (_locker)
            {
                TotalResultReceived++;

                if (TotalResultReceived > Inclusion.Inclusion.FormattedQueriesCount)
                {
                    throw new InvalidOperationException("Too many results came in!");
                }

                if (Inclusion.Status?.Result?.IsFailed ?? false)
                {
                    //inclusion already contains a failed result, so nothing can be done here
                    return;
                }

                //inclusion contains no result or success result, both can be safely replaced
                //but not result state SHOULD be replaced with existing result

                if (result.IsFailed)
                {
                    //incoming result has failed, so it's the end of the result processing for this inclusion
                    Inclusion.SetStatusProcessed(result);
                    return;
                }

                var completed = TotalResultReceived == Inclusion.Inclusion.FormattedQueriesCount;

                if (completed)
                {
                    //all results has came

                    if (Inclusion.Status.Status == ValidationStatusEnum.Processed)
                    {
                        throw new InvalidOperationException("Unknown problem with processed status");
                    }

                    Inclusion.SetStatusProcessed(result);
                    return;
                }

                Inclusion.SetStatusInProgress(TotalResultReceived, Inclusion.Inclusion.FormattedQueriesCount, result);
            }
        }

        public void FixResultIntoInclusion(bool prematurelyStopped)
        {
            if (Inclusion.IsProcessed)
            {
                //inclusion has been processed already, nothing to do
                return;
            }

            //make note: if we are here then TotalResultReceived at least = 1

            var completed = TotalResultReceived == Inclusion.Inclusion.FormattedQueriesCount;

            if (completed)
            {
                //Inclusion should contain valid (success or failure) Result
                //so keep it in the place with new status 'Processed'
                Inclusion.SetStatusProcessedWithNoResultChanged();
            }
            else
            {
                //incomplete validation found
                if (prematurelyStopped)
                {
                    var errorResult = ValidationResult.Error(Inclusion.Inclusion.SqlBody, Inclusion.Inclusion.SqlBody, "Process stopped prematurely");
                    Inclusion.SetStatusProcessed(errorResult);
                }
                else
                {
                    Inclusion.SetStatusProcessedWithNoResultChanged();
                }
            }
        }
    }
}