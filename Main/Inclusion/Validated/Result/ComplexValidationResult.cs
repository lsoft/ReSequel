using System;
using System.Collections.Generic;
using System.Linq;
using Main.Inclusion.Carved.Result;

namespace Main.Inclusion.Validated.Result
{
    public class ComplexValidationResult : IComplexValidationResult
    {
        private readonly List<IValidationResult> _internalResults;
        private readonly CarveResult _carveResult;

        public IReadOnlyCollection<IValidationResult> InternalResults => _internalResults;

        public ValidationResultEnum Result
        {
            get
            {
                //if (InternalResults.Count == 0)
                //{
                //    Console.WriteLine(FullSqlBody);
                //    Console.WriteLine("aaaa");
                //}

                return InternalResults.Min(j => j.Result);
            }
        }

        public bool IsSuccess => InternalResults.All(j => j.IsSuccess);

        public bool IsFailed => !IsSuccess;

        public string WarningOrErrorMessage => IsSuccess ? string.Empty : string.Join(Environment.NewLine, InternalResults.Select(j => j.WarningOrErrorMessage).Where(j => !string.IsNullOrWhiteSpace(j)));

        public string CheckedSqlBody => string.Join(Environment.NewLine, InternalResults.Select(j => j.CheckedSqlBody));

        public string FullSqlBody => string.Join(Environment.NewLine, InternalResults.Select(j => j.CheckedSqlBody));

        public ICarveResult CarveResult => _carveResult;

        public ComplexValidationResult(
            )
        {
            _internalResults = new List<IValidationResult>();
            _carveResult = new CarveResult();
        }

        public void Append(
            IValidationResult result
            )
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            _internalResults.Add(result);

            if (result.CarveResult != null)
            {
                _carveResult.Append(result.CarveResult);
            }
        }
    }
}
