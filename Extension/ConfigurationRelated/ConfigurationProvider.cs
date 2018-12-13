using Main;
using Main.Helper;
using Main.Sql;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Extension.ConfigurationRelated
{
    public interface IConfigurationProvider
    {
        bool TryRead(
            out Configuration configuration
            );

        void Save(
            Configuration configuration
            );

        event ConfigurationFileChangedDelegate ConfigurationFileChangedEvent;
    }

    public delegate void ConfigurationFileChangedDelegate(
        );

    internal sealed class ConfigurationProvider : IConnectionStringContainer, IConfigurationProvider, IDisposable
    {
        private readonly ConfigurationFilePath _path;
        private readonly FileSystemWatcher _watcher;

        public event ConfigurationFileChangedDelegate ConfigurationFileChangedEvent;

        private long _disposed = 0L;

        public ConfigurationProvider(
            ConfigurationFilePath path
            )
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (!path.IsFileExists)
            {
                throw new ArgumentException(path.FilePath);
            }


            _path = path;

            _watcher = new FileSystemWatcher(
                );
            _watcher.Path = _path.FolderPath;
            _watcher.Filter = _path.FileName;
            _watcher.Changed += (a, e) => ConfigurationFileChangedEventInvoke();

            _watcher.EnableRaisingEvents = true;
        }

        public string GetConnectionString()
        {
            Configuration configuration;
            if (!TryRead(out configuration))
            {
                throw new InvalidOperationException("Cannot read configuration");
            }

            var executor = configuration.SqlExecutors.SqlExecutor.First(j => j.IsDefault);

            return
                executor.ConnectionString;
        }

        public bool TryRead(
            out Configuration configuration
            )
        {
            configuration = null;

            try
            {
                var rc = _path.FilePath.ReadXml<Configuration>();

                if (rc != null && !rc.IsEmpty)
                {
                    configuration = rc;

                    return
                        true;
                }
            }
            catch (Exception excp)
            {
                Debug.WriteLine(excp.Message);
                Debug.WriteLine(excp.StackTrace);
            }

            return
                false;
        }

        public void Save(
            Configuration configuration
            )
        {
            _watcher.EnableRaisingEvents = false;

            try
            {
                configuration.SaveXml(
                    _path.FilePath
                    );
            }
            finally
            {
                _watcher.EnableRaisingEvents = true;
            }
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1L) != 0L)
            {
                return;
            }

            _watcher.EnableRaisingEvents = false;
            _watcher.Dispose();
        }

        private void ConfigurationFileChangedEventInvoke()
        {
            var e = this.ConfigurationFileChangedEvent;
            if (e != null)
            {
                e();
            }
        }



    }
}
