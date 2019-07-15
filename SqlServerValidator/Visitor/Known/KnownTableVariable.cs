using System;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServerValidator.Visitor.Known
{
    public class KnownTableVariable : IKnownVariable
    {
        public string Name
        {
            get;
        }

        public string FullDeclarationSql
        {
            get;
        }

        public KnownTableVariable(string name, string fullDeclarationSql)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (fullDeclarationSql == null)
            {
                throw new ArgumentNullException(nameof(fullDeclarationSql));
            }

            Name = name;
            FullDeclarationSql = fullDeclarationSql;
        }

        public string ToSqlDeclaration()
        {
            return
                FullDeclarationSql;
        }

        public static KnownTableVariable Parse(
            DeclareTableVariableStatement statement
            )
        {
            if (statement == null)
            {
                throw new ArgumentNullException(nameof(statement));
            }

            var variableName = statement.Body.VariableName.Value;
            //var variableType = string.Format(
            //    "({0})",
            //    statement.Body.Definition.ToSourceSqlString()
            //    );

            var result = new KnownTableVariable(
                variableName,
                statement.ToSourceSqlString()
                );

            return
                result;
        }
    }


}
