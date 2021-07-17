using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SqlServerValidator.UndeclaredDeterminer
{
    public class AdapterUndeclaredParameterDeterminer : IUndeclaredParameterDeterminer
    {
        private readonly IUndeclaredParameterDeterminer[] _determiners;

        public AdapterUndeclaredParameterDeterminer(
            params IUndeclaredParameterDeterminer[] determiners
            )
        {
            if (determiners is null)
            {
                throw new ArgumentNullException(nameof(determiners));
            }
            _determiners = determiners;
        }

        public async Task<(bool, IReadOnlyDictionary<string, string>)> TryToDetermineParametersAsync(
            string innerSql
            )
        {
            var success = false;
            var rresult = new Dictionary<string, string>();

            foreach (var determiner in _determiners)
            {
                var ir = await determiner.TryToDetermineParametersAsync(innerSql);
                if (ir.Item1)
                {
                    foreach (var pair in ir.Item2)
                    {
                        if (!rresult.ContainsKey(pair.Key))
                        {
                            rresult[pair.Key] = pair.Value;
                        }
                    }

                    success = true;
                }
            }

            return (success, rresult);
        }


        public void Dispose()
        {
            foreach (var determiner in _determiners)
            {
                determiner.Dispose();
            }
        }
    }
}
