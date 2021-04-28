using Extension.CompositionRoot;
using Main.Inclusion.Validated;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Extension.Cache
{
    public class SqlInclusionCache : IVsSolutionEventsExt
    {
        private readonly Dictionary<string, SqlInclusionFileCache> _cache = new Dictionary<string, SqlInclusionFileCache>(StringComparer.InvariantCultureIgnoreCase);
        private readonly object _cacheLocker = new object();

        public event CacheUpdatedDelegate CacheUpdatedEvent;

        public uint Cookie
        {
            get;
            set;
        }

        public SqlInclusionCache(
            )
        {

        }

        public SqlInclusionFileCache GetOrCreateFileCache(
            string filePath
            )
        {
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            lock (_cacheLocker)
            {
                SqlInclusionFileCache result;
                if (!_cache.TryGetValue(filePath, out result))
                {
                    result = new SqlInclusionFileCache(
                        filePath
                        );

                    result.CacheUpdatedEvent += CacheUpdatedEventRaise;

                    _cache[filePath] = result;
                }

                return result;
            }
        }

        public List<IValidatedSqlInclusion> GetAll()
        {
            var result = new List<IValidatedSqlInclusion>();

            List<SqlInclusionFileCache> files;
            lock (_cacheLocker)
            {
                files = _cache.Values.ToList();
            }

            foreach (var file in files)
            {
                file.GetAll(ref result);
            }

            return
                result;
        }

        public List<IValidatedSqlInclusion> GetUnprocessed(
            )
        {
            var result = new List<IValidatedSqlInclusion>();

            List<SqlInclusionFileCache> files;
            lock (_cacheLocker)
            {
                files = _cache.Values.ToList();
            }

            foreach (var file in files)
            {
                file.GetUnprocessed(ref result);
            }

            return
                result;
        }

        public void CleanupProcessedStatus()
        {
            List<SqlInclusionFileCache> files;
            lock (_cacheLocker)
            {
                files = _cache.Values.ToList();
            }

            foreach (var file in files)
            {
                file.CleanupProcessedStatus();
            }
        }

        #region IVsSolutionEvents

        public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
        {
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
        {
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
        {
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
        {
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
        {
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
        {
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        {
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
        {
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        public int OnBeforeCloseSolution(object pUnkReserved)
        {
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        public int OnAfterCloseSolution(object pUnkReserved)
        {
            ClearCache();

            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        #endregion

        private void ClearCache()
        {
            lock (_cacheLocker)
            {
                _cache.Clear();
            }

            //should not call CacheUpdatedEventRaise here!
        }


        private void CacheUpdatedEventRaise()
        {
            var t = CacheUpdatedEvent;
            if (t != null)
            {
                t();
            }
        }

    }
}
