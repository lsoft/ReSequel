using Ninject;
using Ninject.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tests.CompositionRoot
{
    public sealed class Root : IDisposable
    {
        private long _disposed = 0L;

        private StandardKernel _kernel;

        public Root(
            )
        {
            _kernel = new StandardKernel();
        }

        public void BindCommon(
            )
        {
            var ccm = new CommonComponentsModule(
                );
            _kernel.Load(ccm);
        }

        public void BindSqlServer()
        {
            var ssm = new SqlServerModule();
            _kernel.Load(ssm);
        }

        public void BindSqlite()
        {
            var slm = new SqliteModule();
            _kernel.Load(slm);
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
