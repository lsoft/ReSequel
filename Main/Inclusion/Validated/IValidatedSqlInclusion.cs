using Main.Inclusion.Found;
using Main.Inclusion.Validated.Result;
using Main.Other;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace Main.Inclusion.Validated
{
    public interface IValidatedSqlInclusion
    {
        IFoundSqlInclusion Inclusion
        {
            get;
        }

        IValidationResult Result
        {
            get;
        }

        bool HasResult
        {
            get;
        }

        event InclusionStatusChangedDelegate InclusionStatusEvent;

        void ResetResult(
            );

        void SetResult(
            IValidationResult result
            );

        Report GenerateReport(
            );
        
    }
}
