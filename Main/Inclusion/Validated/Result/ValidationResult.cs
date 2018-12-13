using System;
using Main.Inclusion.Carved.Result;

namespace Main.Inclusion.Validated.Result
{
    public class ValidationResult : IModifiedValidationResult
    {
        public ValidationResultEnum Result
        {
            get;
        }

        public bool IsSuccess => Result == ValidationResultEnum.Validated;

        public string WarningOrErrorMessage
        {
            get;
        }

        public string FullSqlBody
        {
            get;
        }

        public string CheckedSqlBody
        {
            get;
        }

        public ICarveResult CarveResult
        {
            get;
        }

        private ValidationResult(
            string fullSqlBody,
            string sqlBody,
            ICarveResult carveResult = null
            )
        {
            if (fullSqlBody == null)
            {
                throw new ArgumentNullException(nameof(fullSqlBody));
            }

            if (sqlBody == null)
            {
                throw new ArgumentNullException(nameof(sqlBody));
            }

            Result = ValidationResultEnum.Validated;
            FullSqlBody = fullSqlBody;
            CheckedSqlBody = sqlBody;
            WarningOrErrorMessage = string.Empty;

            CarveResult = carveResult;
        }

        private ValidationResult(
            ValidationResultEnum result,
            string fullSqlBody,
            string sqlBody,
            string warningOrErrorMessage,
            ICarveResult carveResult = null
            )
        {
            if (fullSqlBody == null)
            {
                throw new ArgumentNullException(nameof(fullSqlBody));
            }

            if (sqlBody == null)
            {
                throw new ArgumentNullException(nameof(sqlBody));
            }

            if (warningOrErrorMessage == null)
            {
                throw new ArgumentNullException(nameof(warningOrErrorMessage));
            }
            //carveResult allowed to be null

            Result = result;
            FullSqlBody = fullSqlBody;
            CheckedSqlBody = sqlBody;
            WarningOrErrorMessage = warningOrErrorMessage;
            CarveResult = carveResult;
        }

        public IModifiedValidationResult WithNewFullSqlBody(string fullSqlBody)
        {
            if (fullSqlBody == null)
            {
                throw new ArgumentNullException(nameof(fullSqlBody));
            }

            var result = new ValidationResult(
                this.Result,
                fullSqlBody,
                this.CheckedSqlBody,
                this.WarningOrErrorMessage
                );

            return result;
        }

        public IModifiedValidationResult WithCarveResult(
            ICarveResult carveResult
            )
        {
            //carveResult allowed to be null

            var result = new ValidationResult(
                this.Result,
                this.FullSqlBody,
                this.CheckedSqlBody,
                this.WarningOrErrorMessage,
                carveResult
                );

            return result;
        }


        public static ValidationResult Success(string fullSqlBody, string sqlBody) => new ValidationResult(fullSqlBody, sqlBody);
        public static ValidationResult Error(string fullSqlBody, string sqlBody, string errorMessage) => new ValidationResult(ValidationResultEnum.FoundError, fullSqlBody, sqlBody, errorMessage);
        public static ValidationResult CannotValidate(string fullSqlBody, string sqlBody, string objectName) => new ValidationResult(ValidationResultEnum.CannotValidate, fullSqlBody, sqlBody, string.Format("{0}: This type of queries cannot be validated without an execution.", objectName));

    }
}
