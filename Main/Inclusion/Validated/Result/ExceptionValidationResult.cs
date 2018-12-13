using Main.Inclusion.Carved.Result;
using System;
using System.Text;

namespace Main.Inclusion.Validated.Result
{
    public class ExceptionValidationResult : IValidationResult
    {
        private readonly string _sqlBody;
        private readonly Exception _excp;

        public ValidationResultEnum Result => ValidationResultEnum.FoundError;

        public bool IsSuccess => false;

        public string WarningOrErrorMessage => GetFailMessage(_excp);

        public string FullSqlBody => _sqlBody;

        public string CheckedSqlBody => _sqlBody;

        public ICarveResult CarveResult
        {
            get;
        }

        public ExceptionValidationResult(
            string sqlBody,
            Exception excp
            )
        {
            if (sqlBody == null)
            {
                throw new ArgumentNullException(nameof(sqlBody));
            }

            if (excp == null)
            {
                throw new ArgumentNullException(nameof(excp));
            }

            _sqlBody = sqlBody;
            _excp = excp;

            CarveResult = null;
        }

        private string GetFailMessage(
            Exception exception
            )
        {
            if (exception == null)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            ComposeFailMessage(
                sb,
                exception,
                0
                );

            return
                sb.ToString();
        }

        private static void ComposeFailMessage(
            StringBuilder sb,
            Exception excp,
            int prefix
            )
        {
            if (excp == null)
            {
                return;
            }

            sb.AppendLine(new string(' ', prefix) + excp.Message);

            //if(excp.InnerException == null)
            //{
            //    sb.AppendLine(excp.StackTrace);
            //}

            if (excp.InnerException != null)
            {
                ComposeFailMessage(
                    sb,
                    excp.InnerException,
                    prefix + 4
                    );
            }
        }
    }
}
