
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Main.Sql.SqlServer.Visitor.Known
{
    public class KnownVariable : IKnownVariable
    {
        public string Name
        {
            get;
        }

        public string Type
        {
            get;
        }

        public KnownVariable(string name, string type)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            Name = name;
            Type = type;
        }

        public string ToSqlDeclaration()
        {
            return
                string.Format("declare {0} {1}", Name, Type);
        }

        public static List<KnownVariable> Parse(
            DeclareVariableStatement statement
            )
        {
            if (statement == null)
            {
                throw new ArgumentNullException(nameof(statement));
            }

            var result = new List<KnownVariable>();
            foreach (var declaration in statement.Declarations)
            {
                var variableName = declaration.VariableName.Value;
                var variableType = declaration.DataType.ToSourceSqlString();

                var kv = new KnownVariable(
                    variableName,
                    variableType
                    );

                result.Add(kv);
            }

            return
                result;
        }

    }


}
