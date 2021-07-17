using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SqlServerValidator.UndeclaredDeterminer
{
    public interface IUndeclaredParameterDeterminer : IDisposable
    {
        Task<(bool, IReadOnlyDictionary<string, string>)> TryToDetermineParametersAsync(
            string innerSql
            );
    }
}
