using Main.Sql.VariableRef;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServerValidator.Visitor.Known;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerValidator.Visitor
{
    public class FunctionArgumentVisitor : TSqlFragmentVisitor
    {
        public readonly Dictionary<string, KnownVariable> _variables = new Dictionary<string, KnownVariable>(SqlVariableStringComparer.Instance);

        public IReadOnlyCollection<KnownVariable> Variables => _variables.Values;

        public FunctionArgumentVisitor()
        {
        }

        public override void ExplicitVisit(FunctionCall node)
        {
            if (StringComparer.InvariantCultureIgnoreCase.Compare("substring", node.FunctionName.Value) == 0)
            {
                ProcessSubstringFunction(node);
                return;
            }

            node.AcceptChildren(this);
        }

        private void ProcessSubstringFunction(FunctionCall node)
        {
            var index = 0;
            foreach (var parameter in node.Parameters)
            {
                if (parameter is VariableReference vrn)
                {
                    AppendNewVariable(
                        vrn.Name,
                        index == 0 ? "varchar(10)" : "int"
                        );
                }

                index++;
            }
        }

        private void AppendNewVariable(string variableName, string variableType)
        {
            if (!_variables.ContainsKey(variableName))
            {
                _variables[variableName] = new KnownVariable(
                    variableName,
                    variableType
                    );
            }
        }

    }
}
