using System;
using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServerValidator.Visitor;
using SqlServerValidator.Visitor.Known;

namespace SqlServerValidator
{
    public static class SqlServerHelper
    {
        public static bool IsItTableVariable(
            this string objectName
            )
        {
            if (objectName == null)
            {
                throw new ArgumentNullException(nameof(objectName));
            }

            if (objectName.StartsWith(StatementVisitor.SqlServerVariablePrefix))
            {
                return true;
            }
            //if (objectName.StartsWith(StatementVisitor.SqlServerSpecificVariablePrefix))
            //{
            //    return true;
            //}

            return
                false;
        }

        public static bool IsItTempTable(
            this string objectName
            )
        {
            if (objectName == null)
            {
                throw new ArgumentNullException(nameof(objectName));
            }

            if (objectName.StartsWith(StatementVisitor.SqlServerTempTablePrefix1))
            {
                return true;
            }
            if (objectName.StartsWith(StatementVisitor.SqlServerTempTablePrefix2))
            {
                return true;
            }

            return
                false;
        }

        public static string FixDuplicates(
            this TSqlFragment statement,
            List<IKnownVariable> knownTokens
            )
        {
            if (statement == null)
            {
                throw new ArgumentNullException(nameof(statement));
            }

            if (knownTokens == null)
            {
                throw new ArgumentNullException(nameof(knownTokens));
            }

            //fix duplicate variables
            var knownVariables = new HashSet<string>(SqlVariableStringComparer.Instance);
            foreach (var knownToken in knownTokens)
            {
                knownVariables.Add(knownToken.Name);
            }

            var processedVariables = new HashSet<string>(SqlVariableStringComparer.Instance);
            for(var tokenIndex = statement.FirstTokenIndex; tokenIndex <= statement.LastTokenIndex; tokenIndex++)
            {
                var token = statement.ScriptTokenStream[tokenIndex];

                if (token.TokenType != TSqlTokenType.Variable)
                {
                    continue;
                }

                var variableName = token.Text;

                if (variableName.StartsWith(StatementVisitor.SqlServerSpecificVariablePrefix))
                {
                    continue;
                }
                if (!variableName.StartsWith(StatementVisitor.SqlServerVariablePrefix))
                {
                    continue;
                }

                if (knownVariables.Contains(variableName))
                {
                    //Это известная переменная, которая будет объявлена в скрипте
                    //ее не надо переименовывать
                    continue;
                }

                if (processedVariables.Contains(variableName))
                {
                    //it's a duplicate
                    var newVariableName = variableName + Guid.NewGuid().ToString().Replace("-", "");

                    token.Text = newVariableName;

                    processedVariables.Add(newVariableName);
                }
                else
                {
                    //not a duplicate, skip it
                    processedVariables.Add(variableName);
                }
            }

            var result = statement.ToSourceSqlString();

            return
                result;
        }
    }


}
