using Main;
using Main.Inclusion;
using Main.Inclusion.Validated;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Inclusion.Validated.Status;

namespace Extension.Cache
{
    public class SqlInclusionFileCache
    {
        private readonly HashSet<IValidatedSqlInclusion> _cache = new HashSet<IValidatedSqlInclusion>(ValidatedSqlInclusionEqualityComparer.Instance);
        private readonly object _locker = new object();

        public string FilePath
        {
            get;
        }

        public SqlInclusionFileCache(
            string filePath
            )
        {
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            FilePath = filePath;
        }

        public void Clear(
            )
        {
            var changesExists = false;
            lock (_locker)
            {
                changesExists = _cache.Count > 0;
                _cache.Clear();
            }

            if (changesExists)
            {
                CacheUpdatedEventRaise();
            }
        }


        public List<IValidatedSqlInclusion> GetAll(
            )
        {
            lock (_locker)
            {
                var result = new IValidatedSqlInclusion[_cache.Count];
                _cache.CopyTo(result);

                return
                    result.ToList();
            }
        }

        public void GetAll(
            ref List<IValidatedSqlInclusion> result
            )
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            lock (_locker)
            {
                result.AddRange(_cache);
            }
        }

        public void Update(
            List<IValidatedSqlInclusion> inclusionsList
            )
        {
            if (inclusionsList == null)
            {
                throw new ArgumentNullException(nameof(inclusionsList));
            }

            var changesExists = false;
            lock (_locker)
            {
                changesExists |= _cache.Count > 0;
                _cache.Clear();

                changesExists |= inclusionsList.Count > 0;
                foreach (var inclusion in inclusionsList)
                {
                    _cache.Add(inclusion);
                }
            }

            if (changesExists)
            {
                CacheUpdatedEventRaise();
            }
        }

        public void GetUnprocessed(
            ref List<IValidatedSqlInclusion> result
            )
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            IValidatedSqlInclusion[] array;
            lock (_locker)
            {
                array = new IValidatedSqlInclusion[_cache.Count];
                _cache.CopyTo(array);
            }

            result.AddRange(array.Where(j => !j.IsProcessed));
        }

        public void CleanupProcessedStatus()
        {
            var changesExists = false;
            lock (_locker)
            {
                foreach (var item in _cache)
                {
                    changesExists |= (item.Status.Status == ValidationStatusEnum.Processed);
                    item.ResetToNotStarted();
                }
            }

            if (changesExists)
            {
                CacheUpdatedEventRaise();
            }
        }


        public event CacheUpdatedDelegate CacheUpdatedEvent;

        private void CacheUpdatedEventRaise()
        {
            var t = CacheUpdatedEvent;
            if(t != null)
            {
                t();
            }
        }
    }
}
