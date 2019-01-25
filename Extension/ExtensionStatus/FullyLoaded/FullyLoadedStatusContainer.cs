using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Extension.ExtensionStatus.FullyLoaded
{
    public sealed class FullyLoadedStatusContainer : IFullyLoadedStatusContainer, IDisposable
    {
        private readonly IVsSolution _solution;

        private readonly ManualResetEvent _stopSignal = new ManualResetEvent(false);

        private volatile bool _isSolutionFullyLoaded;

        private JoinableTask _task;
        private long _disposed = 0L;

        public bool IsSolutionFullyLoaded => _isSolutionFullyLoaded;

        public FullyLoadedStatusContainer(
            IVsSolution solution
            )
        {
            if (solution == null)
            {
                throw new ArgumentNullException(nameof(solution));
            }

            _solution = solution;
        }

        public void AsyncStart()
        {
            if (Interlocked.Read(ref _disposed) != 0L)
            {
                return;
            }

            _task = ThreadHelper.JoinableTaskFactory.RunAsync(async delegate
            {
                await ReadFullyLoadStatusAsync();
            });
        }

        public void SyncStop()
        {
            if (Interlocked.Exchange(ref _disposed, 1L) != 0L)
            {
                return;
            }

            _stopSignal.Set();

            var task = Interlocked.Exchange(ref _task, null);
            if (task != null)
            {
                task.Join();
            }
        }

        public void Dispose()
        {
            SyncStop();
        }


        private async System.Threading.Tasks.Task ReadFullyLoadStatusAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            while (!_stopSignal.WaitOne(0))
            {
                object asm;
                _solution.GetProperty((int)__VSPROPID4.VSPROPID_IsSolutionFullyLoaded, out asm);

                if (asm is bool)
                {
                    if ((bool)asm)
                    {
                        _isSolutionFullyLoaded = true;
                        return;
                    }
                }

                await System.Threading.Tasks.Task.Delay(100);
            }
        }
    }
}
