using System;
using Main.Inclusion.Validated;
using Main.Inclusion.Validated.Result;
using Main.Validator.UnitProvider.Bag;

namespace Main.Validator.UnitProvider
{
    public class ValidationUnit : IValidationUnit
    {
        private readonly IInclusionBag _bag;

        public IValidatedSqlInclusion Inclusion => _bag.Inclusion;

        public string SqlBody
        {
            get;
        }

        public IValidationResult Result => _bag.Result;

        public ValidationUnit(
            IInclusionBag bag,
            string sqlBody
        )
        {
            if (bag == null)
            {
                throw new ArgumentNullException(nameof(bag));
            }

            if (sqlBody == null)
            {
                throw new ArgumentNullException(nameof(sqlBody));
            }

            //if (sqlBody == "")
            //{
            //    Console.WriteLine("ccc");
            //    Console.WriteLine(bag.Inclusion.Inclusion.FilePath);
            //    Console.WriteLine(bag.Inclusion.Inclusion.Location.StartLinePosition.Line);
            //    Console.WriteLine(bag.Inclusion.Inclusion.Location.StartLinePosition.Character);
            //}

            SqlBody = sqlBody;

            _bag = bag;
        }

        public void SetValidationResult(IValidationResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            _bag.SetValidationResult(result);
        }
    }
}