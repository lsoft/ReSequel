using System;
using Main.Inclusion.Validated;
using Main.Inclusion.Validated.Result;

namespace Main.Validator.UnitProvider.Bag
{
    public class InclusionBag : IInclusionBag
    {
        private readonly object _locker = new object();

        public IValidatedSqlInclusion Inclusion
        {
            get;
        }

        public IComplexValidationResult Result
        {
            get;
            private set;
        }

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

        public void SetValidationResult(IComplexValidationResult result)
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
                    throw new InvalidOperationException("Too many results come in!");
                }

                if (Result != null)
                {
                    if (Result.IsSuccess)
                    {
                        //if previous Result is a success, replace it
                        Result = result;
                    }
                    else
                    {
                        //previous Result is a failure, so keep it in the place
                    }
                }
                else
                {
                    //previous Result does not exists yet
                    Result = result;
                }

                if (TotalResultReceived == Inclusion.Inclusion.FormattedQueriesCount)
                {
                    //all results has came
                    //it's time to store result into inclusion

                    FixResultIntoInclusion(false);
                }
            }
        }

        public void FixResultIntoInclusion(bool prematurelyStopped)
        {
            if (Inclusion.HasResult)
            {
                return;
            }

            if (Result != null && TotalResultReceived == Inclusion.Inclusion.FormattedQueriesCount)
            {
                //bag is exists and full, let's process it

                Inclusion.SetResult(Result);
            }
            else
            {
                //result for this inclusion is not found
                if (!prematurelyStopped)
                {
                    //validation process completed at 100%
                    //but results does not exists
                    //unknown problem
                    throw new InvalidOperationException("Unknown problem with validation #2");
                }

                var errorResult = ValidationResult.Error(Inclusion.Inclusion.SqlBody, Inclusion.Inclusion.SqlBody, "Process stopped prematurely");
                Inclusion.SetResult(errorResult);

                //var cvr = new ComplexValidationResult();
                //cvr.Append(ValidationResult.Error(Inclusion.Inclusion.SqlBody, Inclusion.Inclusion.SqlBody, "Process stopped prematurely"));
                //Inclusion.SetResult(cvr);
            }
        }
    }
}