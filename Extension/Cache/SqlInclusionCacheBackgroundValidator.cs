using Main.Progress;
using Main.Validator;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Extension.Cache
{
    public sealed class SqlInclusionCacheBackgroundValidator : IDisposable
    {
        private const long NotStarted = 0L;
        private const long Started = 1L;
        private const long Disposed = 2L;

        private readonly IValidatorFactory _validatorFactory;
        private readonly ValidationProgressFactory _statusFactory;
        private readonly SqlInclusionCache _cache;

        private readonly AutoResetEvent _stopSignal = new AutoResetEvent(false);

        private long _status = NotStarted;

        private Thread _t;

        public SqlInclusionCacheBackgroundValidator(
            IValidatorFactory validatorFactory,
            ValidationProgressFactory statusFactory,
            SqlInclusionCache cache
            )
        {
            if (validatorFactory == null)
            {
                throw new ArgumentNullException(nameof(validatorFactory));
            }

            if (statusFactory == null)
            {
                throw new ArgumentNullException(nameof(statusFactory));
            }

            if (cache == null)
            {
                throw new ArgumentNullException(nameof(cache));
            }

            _validatorFactory = validatorFactory;
            _statusFactory = statusFactory;
            _cache = cache;
        }

        public void AsyncStart()
        {
            if (Interlocked.CompareExchange(ref _status, Started, NotStarted) != NotStarted)
            {
                return;
            }

            _cache.CacheUpdatedEvent += StartValidation;
        }

        public void SyncStop(
            )
        {
            Dispose();
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _status, Disposed) == Disposed)
            {
                return;
            }

            _cache.CacheUpdatedEvent -= StartValidation;

            StopOldThread(null);

            _stopSignal.Dispose();
        }

        private void StartValidation()
        {
            var newThread = new Thread(DoWork);
            newThread.IsBackground = true;

            StopOldThread(newThread);

            newThread.Start();
        }

        private void StopOldThread(
            Thread newThread
            )
        {
            //newThread allowed to be null

            var oldThread = Interlocked.Exchange(ref _t, newThread);

            if (oldThread == null)
            {
                return;
            }

            if (!oldThread.IsAlive)
            {
                return;
            }

            _stopSignal.Set();

            try
            {
                oldThread.Join();
            }
            catch (Exception excp)
            {
                Debug.WriteLine(excp.Message);
                Debug.WriteLine(excp.StackTrace);
            }
        }

        private void DoWork()
        {
            try
            {
                var unprocesseds = _cache.GetUnprocessed();
                if (unprocesseds.Count > 0)
                {
                    //Debug.WriteLine(
                    //    "$$$$$$$$$$$$$$$$$$ PROCESS ITEMS: {0} $$$$$$$$$$$$$$$$$$",
                    //    unprocesseds.Count
                    //    );

                    var status = _statusFactory.Create(
                        );

                    var validator = _validatorFactory.Create(
                        status
                        );

                    validator.Validate(
                        unprocesseds
                        );
                }
            }
            catch(Exception excp)
            {
                Debug.WriteLine(excp.Message);
                Debug.WriteLine(excp.StackTrace);
            }
        }

    }
}
