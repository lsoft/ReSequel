using System;
using System.Threading;
using Main;
using Main.Other;
using Main.Inclusion.Validated.Result;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Main.Inclusion.Found;

namespace Main.Inclusion.Validated
{
    public class ValidatedSqlInclusion : IValidatedSqlInclusion
    {
        private IValidationResult _result;

        public IFoundSqlInclusion Inclusion
        {
            get;
            private set;
        }

        public IValidationResult Result => _result;

        public bool HasResult => _result != null;

        public event InclusionStatusChangedDelegate InclusionStatusEvent;

        public ValidatedSqlInclusion(
           IFoundSqlInclusion inclusion
            )
        {
            if (inclusion == null)
            {
                throw new ArgumentNullException(nameof(inclusion));
            }

            _result = null;
            Inclusion = inclusion;
        }

        public void ResetResult()
        {
            _result = null;

            RaiseInclusionStatusEvent();
        }

        public void SetResult(IValidationResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if(Interlocked.CompareExchange(ref _result, result, null) != null) 
            {
                throw new InvalidOperationException("double time");
            }

            RaiseInclusionStatusEvent();
        }

        public Report GenerateReport(
            )
        {
            if(!HasResult)
            {
                throw new InvalidOperationException("Not processed already");
            }

            var report = new Report(
                Inclusion.Location.Path,
                Inclusion.Location.StartLinePosition.Line,
                Inclusion.SqlBody,
                _result.Result,
                _result.WarningOrErrorMessage
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
