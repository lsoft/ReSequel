using SqlServerValidator.Visitor;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SqlServerValidator.UndeclaredDeterminer
{
    public class SqlXmlUndeclaredParameterDeterminer : IUndeclaredParameterDeterminer
    {
        public SqlXmlUndeclaredParameterDeterminer(
            )
        {
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<(bool, IReadOnlyDictionary<string, string>)> TryToDetermineParametersAsync(
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            string innerSql
            )
        {
            if (innerSql == null)
            {
                throw new ArgumentNullException(nameof(innerSql));
            }

            var dict = new Dictionary<string, string>();

            try
            {
                var sqlXmlVariableExtractor = new SqlXmlVariableExtractor();
                foreach (var variableName in sqlXmlVariableExtractor.ExtractNames(innerSql))
                {
                    dict.Add(variableName, "int");
                }

                return (true, dict);
            }
            catch (Exception excp)
            {
                Debug.WriteLine(excp.Message);
                Debug.WriteLine(excp.StackTrace);
            }

            return (false, null);
        }


        public void Dispose()
        {
        }
    }
}
