using System;
using System.Collections.Generic;
using System.Linq;
using Main.Sql.ConnectionString;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServerValidator.UndeclaredDeterminer;
using SqlServerValidator.Visitor;
using SqlServerValidator.Visitor.Known;

namespace SqlServerValidator
{

    public interface IDuplicateProcessor
    {
        string DetermineDuplicates(
            TSqlFragment statement,
            List<IKnownVariable> knownTokens
            );

        //string SuggestDuplicates(
        //    TSqlFragment statement,
        //    List<IKnownVariable> knownTokens
        //    );
    }

    public class DuplicateProcessor : IDuplicateProcessor
    {
        private readonly IConnectionStringContainer _connectionStringContainer;

        public DuplicateProcessor(
            IConnectionStringContainer connectionStringContainer
            )
        {
            if (connectionStringContainer == null)
            {
                throw new ArgumentNullException(nameof(connectionStringContainer));
            }

            _connectionStringContainer = connectionStringContainer;
        }

        public string DetermineDuplicates(
            TSqlFragment statement,
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


            var determinedVariables = new Dictionary<string, string>(SqlVariableStringComparer.Instance);

            var knownDeclarations = string.Join(Environment.NewLine, knownTokens.ConvertAll(j => j.ToSqlDeclaration()));
            var renamedSql = knownDeclarations + Environment.NewLine + RenameDuplicates(statement, knownTokens);
            using (var determiner = new UndeclaredParameterDeterminer(_connectionStringContainer.GetConnectionString()))
            {
                if (determiner.TryToDetermineTypes(renamedSql, out var dict))
                {
                    foreach (var pair in dict)
                    {
                        determinedVariables.Add(pair.Key, pair.Value);
                    }
                }
            }

            //determine unknown variable references
            var butcher = new ButcherVisitor();
            statement.Accept(butcher);

            //prepare known variables
            var knownVariables = new HashSet<string>(SqlVariableStringComparer.Instance);
            foreach (var knownToken in knownTokens)
            {
                knownVariables.Add(knownToken.Name);
            }

            //process only variables with multiple references
            var processedVariables = new Dictionary<string, IKnownVariable>(SqlVariableStringComparer.Instance);
            foreach (var variable in butcher.VariableReferenceList.Where(j => j.ReferenceCount > 1))
            {
                var variableName = variable.Name;

                if (knownVariables.Contains(variableName))
                {
                    //it is KNOWN variable, that has been declared in this script
                    //we should skip it
                    continue;
                }

                //it is UNKNOWN variable

                if (!processedVariables.ContainsKey(variableName))
                {
                    var variableType = "int";
                    if (determinedVariables.ContainsKey(variableName))
                    {
                        //we were able to determine its type, use it!
                        variableType = determinedVariables[variableName];
                    }

                    var v = new KnownVariable(variableName, variableType);

                    processedVariables.Add(variableName, v);
                }
            }

            var result = string.Join(Environment.NewLine, processedVariables.Values.Select(j => j.ToSqlDeclaration())) + Environment.NewLine + statement.ToSourceSqlString();

            return
                result;
        }


        //public string SuggestDuplicates(
        //    TSqlFragment statement,
        //    List<IKnownVariable> knownTokens
        //    )
        //{
        //    if (statement == null)
        //    {
        //        throw new ArgumentNullException(nameof(statement));
        //    }

        //    if (knownTokens == null)
        //    {
        //        throw new ArgumentNullException(nameof(knownTokens));
        //    }

        //    //determine unknown variable references
        //    var butcher = new ButcherVisitor();
        //    statement.Accept(butcher);

        //    //prepare known variables
        //    var knownVariables = new HashSet<string>(SqlVariableStringComparer.Instance);
        //    foreach (var knownToken in knownTokens)
        //    {
        //        knownVariables.Add(knownToken.Name);
        //    }

        //    //process only variables with multiple references
        //    var processedVariables = new Dictionary<string, IKnownVariable>(SqlVariableStringComparer.Instance);
        //    foreach(var variable in butcher.VariableReferenceList.Where(j => j.ReferenceCount > 1))
        //    {
        //        var variableName = variable.Name;

        //        if (knownVariables.Contains(variableName))
        //        {
        //            //Это известная переменная, которая будет объявлена в скрипте
        //            //ее не надо переименовывать
        //            continue;
        //        }

        //        if (!processedVariables.ContainsKey(variableName))
        //        {
        //            //it's a duplicate
        //            var v = new KnownVariable(variableName, "varchar(10)");

        //            processedVariables.Add(variableName, v);
        //        }
        //    }

        //    var result = string.Join(Environment.NewLine, processedVariables.Values.Select(j => j.ToSqlDeclaration())) + Environment.NewLine + statement.ToSourceSqlString();

        //    return
        //        result;
        //}

        //public static string ProcessDuplicates(
        //    this TSqlFragment statement,
        //    List<IKnownVariable> knownTokens
        //    )
        //{
        //    if (statement == null)
        //    {
        //        throw new ArgumentNullException(nameof(statement));
        //    }

        //    if (knownTokens == null)
        //    {
        //        throw new ArgumentNullException(nameof(knownTokens));
        //    }

        //    //fix duplicate variables
        //    var knownVariables = new HashSet<string>(SqlVariableStringComparer.Instance);
        //    foreach (var knownToken in knownTokens)
        //    {
        //        knownVariables.Add(knownToken.Name);
        //    }

        //    var processedVariables = new Dictionary<string, IKnownVariable>(SqlVariableStringComparer.Instance);
        //    for (var tokenIndex = statement.FirstTokenIndex; tokenIndex <= statement.LastTokenIndex; tokenIndex++)
        //    {
        //        var token = statement.ScriptTokenStream[tokenIndex];

        //        if (token.TokenType != TSqlTokenType.Variable)
        //        {
        //            continue;
        //        }

        //        var variableName = token.Text;

        //        if (variableName.StartsWith(StatementVisitor.SqlServerSpecificVariablePrefix))
        //        {
        //            continue;
        //        }
        //        if (!variableName.StartsWith(StatementVisitor.SqlServerVariablePrefix))
        //        {
        //            continue;
        //        }

        //        if (knownVariables.Contains(variableName))
        //        {
        //            //Это известная переменная, которая будет объявлена в скрипте
        //            //ее не надо переименовывать
        //            continue;
        //        }

        //        if (!processedVariables.ContainsKey(variableName))
        //        {
        //            //it's a duplicate
        //            var v = new KnownVariable(variableName, "varchar(10)");

        //            processedVariables.Add(variableName, v);
        //        }
        //    }

        //    var result = string.Join(Environment.NewLine, processedVariables.Values.Select(j => j.ToSqlDeclaration())) + Environment.NewLine + statement.ToSourceSqlString();

        //    return
        //        result;
        //}

        private string RenameDuplicates(
            TSqlFragment statement,
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

            ////clone the token stream because we want to modify it
            //var tokenStream = statement.ScriptTokenStream.Skip(statement.FirstTokenIndex).Take(statement.LastTokenIndex - statement.FirstTokenIndex + 1).ToList();

            //revert info container
            var revertList = new List<(int, string)>();

            //iterate through stream
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

                    //store additional info for further revert
                    revertList.Add((tokenIndex, token.Text));

                    //modify SQL body
                    statement.ScriptTokenStream[tokenIndex].Text = newVariableName;

                    processedVariables.Add(newVariableName);
                }
                else
                {
                    //not a duplicate, skip it
                    processedVariables.Add(variableName);
                }
            }

            var result = statement.ToSourceSqlString();

            //do revert
            foreach (var tuple in revertList)
            {
                statement.ScriptTokenStream[tuple.Item1].Text = tuple.Item2;
            }

            return
                result;
        }
    }

    
}
