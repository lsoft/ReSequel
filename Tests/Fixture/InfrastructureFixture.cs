using System.Collections.Generic;
using Main.Inclusion.Carved.Result;
using Main.Inclusion.Found;
using Main.Inclusion.Scanner.Generator;
using Main.Inclusion.Validated;
using Main.Progress;
using Main.Sql;
using Main.Validator;
using Microsoft.CodeAnalysis.CSharp;
using Tests.CompositionRoot;

namespace Tests.Fixture
{
    public class InfrastructureFixture
    {
        protected static Root Root;



        public static void ClassCleanup()
        {
            Root.Dispose();
        }


        protected ICarveResult Carve(
            string sqlBody
            )
        {

            var sqlButcherFactory = Root.GetInstance<ISqlButcherFactory>();

            var sqlButcher = sqlButcherFactory.Create(
                );

            var carveResult = sqlButcher.Carve(
                sqlBody
                );

            return
                carveResult;
        }

        protected IValidatedSqlInclusion ValidateAgainstSchema(
            string sqlBody
            )
        {
            var syntax = SyntaxFactory.Argument(
                SyntaxFactory.LiteralExpression(
                    SyntaxKind.StringLiteralExpression,
                    SyntaxFactory.Token(
                        SyntaxKind.StringLiteralToken
                        )
                    )
                );

            var foundInclusionList = new List<IFoundSqlInclusion>
            {
                new FoundSqlInclusion(
                    syntax,
                    sqlBody,
                    false
                    )
            };

            var validationInclusionList = foundInclusionList
                .ConvertAll(j => (IValidatedSqlInclusion)new ValidatedSqlInclusion(j))
                ;

            var status = Root.GetInstance<ValidationProgress>();

            var validatorFactory = Root.GetInstance<IValidatorFactory>();

            var validator = validatorFactory.Create(
                status
                );

            validator.ValidateAsync(
                validationInclusionList,
                () => false
                ).GetAwaiter().GetResult();

            return
                validationInclusionList[0];
        }

        protected IValidatedSqlInclusion ValidateAgainstSchema(
            Generator generator
            )
        {
            var syntax = SyntaxFactory.Argument(
                SyntaxFactory.LiteralExpression(
                    SyntaxKind.StringLiteralExpression,
                    SyntaxFactory.Token(
                        SyntaxKind.StringLiteralToken
                    )
                )
            );

            var foundInclusionList = new List<IFoundSqlInclusion>
            {
                new FoundSqlInclusion(
                    syntax,
                    generator,
                    false
                )
            };

            var validationInclusionList = foundInclusionList
                   .ConvertAll(j => (IValidatedSqlInclusion)new ValidatedSqlInclusion(j))
                ;

            var status = Root.GetInstance<ValidationProgress>();

            var validatorFactory = Root.GetInstance<IValidatorFactory>();

            var validator = validatorFactory.Create(
                status
            );

            validator.ValidateAsync(
                validationInclusionList,
                () => false
                ).GetAwaiter().GetResult();

            return
                validationInclusionList[0];
        }
    }
}
