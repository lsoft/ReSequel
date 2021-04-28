using System;
using System.Threading;
using Ninject;
using Extension.TaskRelated;

namespace Extension.CompositionRoot
{
    internal sealed class Root : IDisposable
    {
        private long _disposed = 0L;

        private StandardKernel _kernel;
        private readonly WorkingTask _task;

        public Root(
            WorkingTask task
            )
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            _task = task;

            _kernel = new StandardKernel();
        }

        public void BindAll(
            )
        {
            var ccm = new CommonComponentsModule(
                _task
                );
            _kernel.Load(ccm);

            var ssem = new SqlServerExecutorModule(
                );
            _kernel.Load(ssem);
        }

        public T GetInstance<T>()
        {
            return
                _kernel.Get<T>();
        }

        public T GetInstance<T>(
            string bindingName
            )
        {
            if (bindingName == null)
            {
                throw new ArgumentNullException(nameof(bindingName));
            }

            return
                _kernel.Get<T>(
                    bindingName
                    );
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1L) != 0L)
            {
                return;
            }

            _kernel.Dispose();
        }

    }

}
