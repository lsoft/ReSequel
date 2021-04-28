using System;
using System.Collections.Generic;
using System.Diagnostics;
using Main.Helper;
using Main.Sql.Identifier;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServerValidator.Visitor;

namespace SqlServerValidator.Identifier
{
    [DebuggerDisplay("{FullFunctionName}")]
    public class SqlServerFunctionName : IFunctionName
    {
        private readonly List<string> _mine;

        public string FullFunctionName
        {
            get;
        }

        public SqlServerFunctionName(
            CallTarget callTarget,
            Microsoft.SqlServer.TransactSql.ScriptDom.Identifier identifier
            )
        {
            if (callTarget != null)
            {
                FullFunctionName = callTarget.ToSqlString() + identifier.ToSqlString();
            }
            else
            {
                FullFunctionName = identifier.ToSqlString();
            }
        }

        public bool IsSame(
            string otherFunctionName
            )
        {
            if (!otherFunctionName.IsCorrectWildcard())
            {
                throw new ArgumentException("Invalid wild card: " + otherFunctionName);
            }

            throw new NotImplementedException("please implement it later when it needs");
        }


    }
}
