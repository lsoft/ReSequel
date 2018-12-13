using Main.Logger;
using System;
using System.Threading;

namespace Main.Progress
{
    public class ValidationProgress
    {
        private int _inclusionFound;
        private int _processedInclusionCount;
        private readonly IProcessLogger _logger;

        public int InclusionFound
        {
            get
            {
                return
                    _inclusionFound;
            }
        }

        public int ProcessedInclusionCount
        {
            get
            {
                return
                    Volatile.Read(ref _processedInclusionCount);
            }
        }

        public DateTime? StartTime
        {
            get;
            private set;
        }

        public DateTime? InclusionFoundFinishTime
        {
            get;
            private set;
        }

        public DateTime? FinishTime
        {
            get;
            private set;
        }

        public ValidationProgress(
            IProcessLogger logger
            )
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            _logger = logger;

            _inclusionFound = -1;
        }

        public void SetInclusionCount(
            int inclusionCount
            )
        {
            var old = Interlocked.Exchange(ref _inclusionFound, inclusionCount);
            if (old != -1)
            {
                throw new InvalidOperationException("Double time");
            }

            InclusionFoundFinishTime = DateTime.Now;

            UpdateMessage();
        }

        public void Start()
        {
            StartTime = DateTime.Now;
        }
        
        public void Finish()
        {
            FinishTime = DateTime.Now;
        }

        public void AddProcessedInclusionCount(
            int value
            )
        {
            Interlocked.Add(ref _processedInclusionCount, value);

            UpdateMessage();
        }

        public void UpdateMessage()
        {
            var startTime = StartTime ?? DateTime.Now;
            var inclusionFoundFinishTime = InclusionFoundFinishTime ?? DateTime.Now;
            var finishTime = FinishTime ?? DateTime.Now;

            var taken0 = inclusionFoundFinishTime - startTime;
            var taken1 = finishTime - inclusionFoundFinishTime;
            var total = finishTime - startTime;

            _logger.ShowProcessMessage(
                "Total found: {0:D5}  |  Scanning: {1}  |  Validated: {2:D5}  |  Validation: {3}  |  Total: {4}",
                _inclusionFound,
                taken0,
                _processedInclusionCount,
                taken1,
                total
                );
        }

    }


}
