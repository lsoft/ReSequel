using System;
using System.Collections.Generic;

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

        public bool TryToDetermineParameters(string innerSql, out IReadOnlyDictionary<string, string> result)
        {
            var success = false;
            var rresult = new Dictionary<string, string>();

            foreach (var determiner in _determiners)
            {
                if (determiner.TryToDetermineParameters(innerSql, out var iresult))
                {
                    foreach (var pair in iresult)
                    {
                        if (!rresult.ContainsKey(pair.Key))
                        {
                            rresult[pair.Key] = pair.Value;
                        }
                    }

                    success = true;
                }
            }

            result = rresult;
            return success;
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
