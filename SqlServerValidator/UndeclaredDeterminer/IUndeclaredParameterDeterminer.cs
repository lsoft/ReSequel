using System;
using System.Collections.Generic;

namespace SqlServerValidator.UndeclaredDeterminer
{
    public interface IUndeclaredParameterDeterminer : IDisposable
    {
        bool TryToDetermineParameters(
                string innerSql,
                out IReadOnlyDictionary<string, string> result
                );
    }
}
