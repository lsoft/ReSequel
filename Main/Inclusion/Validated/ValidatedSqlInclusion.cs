using System;
using System.Threading;
using Main;
using Main.Other;
using Main.Inclusion.Validated.Result;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Main.Inclusion.Found;
using Main.Inclusion.Validated.Status;

namespace Main.Inclusion.Validated
{
    public class ValidatedSqlInclusion : IValidatedSqlInclusion
    {
        public IFoundSqlInclusion Inclusion
        {
            get;
            private set;
        }

        public IValidationStatus Status
        {
            get;
            private set;
        }

        public bool IsProcessed => Status.Status == ValidationStatusEnum.Processed;

        public event InclusionStatusChangedDelegate InclusionStatusEvent;

        public ValidatedSqlInclusion(
            IFoundSqlInclusion inclusion
            )
        {
            if (inclusion == null)
            {
                throw new ArgumentNullException(nameof(inclusion));
            }

            Inclusion = inclusion;
            Status = ValidationStatus.NotStarted();
        }

        public void SetStatusNotStarted()
        {
            Status = ValidationStatus.NotStarted();

            RaiseInclusionStatusEvent();
        }

        public void SetStatusInProgress(int processedCount, int totalCount)
        {
            Status = ValidationStatus.InProgress(processedCount, totalCount, null);

            RaiseInclusionStatusEvent();
        }

        public void SetStatusInProgress(int processedCount, int totalCount, IValidationResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            Status = ValidationStatus.InProgress(processedCount, totalCount, result);

            RaiseInclusionStatusEvent();
        }

        public void SetStatusProcessed(IValidationResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            Status = ValidationStatus.Processed(result);

            RaiseInclusionStatusEvent();
        }

        public void SetStatusProcessedWithNoResultChanged()
        {
            if (Status.Status == ValidationStatusEnum.Processed)
            {
                throw new InvalidOperationException("Status is processed, but it shouldn't allowed!");
            }

            Status = ValidationStatus.Processed(Status.Result);

            RaiseInclusionStatusEvent();
        }


        public void ResetToNotStarted()
        {
            Status.ResetToNotStarted();

            RaiseInclusionStatusEvent();
        }

        public Report GenerateReport(
            )
        {
            if(!IsProcessed)
            {
                throw new InvalidOperationException("Not processed already");
            }

            var report = new Report(
                Inclusion.Location.Path,
                Inclusion.Location.StartLinePosition.Line,
                Inclusion.SqlBody,
                Status.Result.Result,
                Status.Result.WarningOrErrorMessage
                );

            return
                report;
        }


        private void RaiseInclusionStatusEvent()
        {
            var t = InclusionStatusEvent;
            if(t!=null)
            {
                t();
            }
        }

    }
}
