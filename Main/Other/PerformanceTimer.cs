using System;
using System.Runtime.InteropServices;

namespace Main.Other
{
    public class PerformanceTimer 
    {
        private const string LibName = "kernel32.dll";

        [DllImport(LibName)]
        private static extern int QueryPerformanceCounter(ref long count);

        [DllImport(LibName)]
        private static extern int QueryPerformanceFrequency(ref long frequency);

        private static readonly bool _isPerfCounterSupported = false;
        public static readonly double Frequency = 0;

        private long _startValue = 0L;

        static PerformanceTimer()
        {
            // Query the high-resolution timer only if it is supported.
            // A returned frequency of 1000 typically indicates that it is not
            // supported and is emulated by the OS using the same value that is
            // returned by Environment.TickCount.
            // A return value of 0 indicates that the performance counter is
            // not supported.
            long frequency = 0L;
            int returnVal = QueryPerformanceFrequency(ref frequency);

            if (returnVal != 0 && frequency != 1000)
            {
                // The performance counter is supported.
                _isPerfCounterSupported = true;
                Frequency = (double)frequency;
            }
            else
            {
                // The performance counter is not supported. Use
                // Environment.TickCount instead.
                Frequency = 1000.0;
            }
        }

        public PerformanceTimer()
        {
            Restart();
        }

        public void Restart()
        {
            _startValue = Value;
        }

        public long NanoSeconds
        {
            get
            {
                var diff = Value - _startValue;
                diff *= 1000000000L;
                var ddiff = diff / Frequency;

                return
                    (long)ddiff;
            }
        }

        public long MicroSeconds
        {
            get
            {
                var diff = Value - _startValue;
                diff *= 1000000L;
                var ddiff = diff / Frequency;

                return
                    (long)ddiff;
            }
        }

        public double MilliSeconds
        {
            get
            {
                var diff = Value - _startValue;
                diff *= 1000L;
                var ddiff = diff / Frequency;

                return
                    ddiff;
            }
        }

        public double Seconds
        {
            get
            {
                var diff = Value - _startValue;
                var ddiff = diff / Frequency;

                return
                    ddiff;
            }
        }

        public long Value
        {
            get
            {
                long tickCount = 0;

                if (_isPerfCounterSupported)
                {
                    // Get the value here if the counter is supported.
                    QueryPerformanceCounter(ref tickCount);
                    
                    return
                        tickCount;
                }
                else
                {
                    // Otherwise, use Environment.TickCount.
                    return
                        (long)Environment.TickCount;
                }
            }
        }
    }
}
