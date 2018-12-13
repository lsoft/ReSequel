using Main.Inclusion.Validated.Result;
using System;

namespace Main.Other
{
    public sealed class Report
    {
        public string FilePath
        {
            get;
        }
        public int LineNumber
        {
            get;
        }
        public string SqlQuery
        {
            get;
        }

        public ValidationResultEnum Result
        {
            get;
        }

        public string FailMessage
        {
            get;
        }

        public bool IsSuccess
        {
            get
            {
                return
                    string.IsNullOrEmpty(FailMessage);
            }
        }

        public Report(
            string filePath, 
            int lineNumber,
            string sqlQuery,
            ValidationResultEnum result,
            string failMessage
            )
        {
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (sqlQuery == null)
            {
                throw new ArgumentNullException(nameof(sqlQuery));
            }

            if (failMessage == null)
            {
                throw new ArgumentNullException(nameof(failMessage));
            }

            FilePath = filePath;
            LineNumber = lineNumber;
            SqlQuery = sqlQuery;
            Result = result;
            FailMessage = failMessage;
        }
    }



}
