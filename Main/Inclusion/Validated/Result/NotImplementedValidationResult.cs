
using Main.Inclusion.Carved.Result;
using System;

namespace Main.Inclusion.Validated.Result
{
    public class NotImplementedValidationResult : IModifiedValidationResult
    {
        private readonly string _message;

        public ValidationResultEnum Result => ValidationResultEnum.NotImplemented;

        public bool IsSuccess => false;

        public string WarningOrErrorMessage => _message;

        public string CheckedSqlBody => string.Empty;

        public string FullSqlBody => string.Empty;

        public ICarveResult CarveResult
        {
            get;
        }

        public NotImplementedValidationResult(
            string message,
            ICarveResult carveResult = null
            )
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            //carveResult allowed to be null

            _message = message;

            CarveResult = carveResult;
        }

        public IModifiedValidationResult WithNewFullSqlBody(string fullSqlBody)
        {
            throw new InvalidOperationException();
        }

        public IModifiedValidationResult WithCarveResult(ICarveResult carveResult)
        {
            //carveResult allowed to be null

            var result = new NotImplementedValidationResult(
                _message,
                carveResult
                );

            return result;
        }

        //public IModifiedValidationResult WithNewFullSqlBody(string fullSqlBody)
        //{
        //    if (fullSqlBody == null)
        //    {
        //        throw new ArgumentNullException(nameof(fullSqlBody));
        //    }

        //    var result = new ValidationResult(
        //        this.Result,
        //        fullSqlBody,
        //        this.CheckedSqlBody,
        //        this.WarningOrErrorMessage
        //        );

        //    return result;
        //}

        //public IModifiedValidationResult WithCarveResult(
        //    ICarveResult carveResult
        //    )
        //{
        //    //carveResult allowed to be null

        //    var result = new ValidationResult(
        //        this.Result,
        //        this.FullSqlBody,
        //        this.CheckedSqlBody,
        //        this.WarningOrErrorMessage,
        //        carveResult
        //        );

        //    return result;
        //}
    }


}
