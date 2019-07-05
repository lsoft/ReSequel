using Main;
using Main.Inclusion;
using Main.Inclusion.Carved;
using Main.Inclusion.Carved.Result;
using Main.Inclusion.Found;
using Main.Inclusion.Validated;
using Main.Progress;
using Main.Sql;
using Main.Validator;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Main.Inclusion.Scanner.Generator;
using Tests.CompositionRoot;

namespace Tests.Fixture
{
    [TestClass]
    public class BaseFixture : AbstractBaseFixture
    {
        protected static Root Root
        {
            get;
            private set;
        }

        public static void ClassInit()
        {
            //MSBuildLocator.RegisterDefaults();

            using (var connection = OpenConnection("master"))
            {
                connection.ExecuteBatch(
                    string.Format(
                        TestSettings.Default.CreateDatabaseScript,
                        TestSettings.Default.DatabaseName
                        )
                    );
            }

            Root = new Root();
            Root.BindAll();
        }


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

            validator.Validate(
                validationInclusionList,
                () => false
                );

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

            validator.Validate(
                validationInclusionList,
                () => false
            );

            return
                validationInclusionList[0];
        }
    }
}
