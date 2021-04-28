using Main.Helper;
using Main.Sql;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Main.Sql.ConnectionString;

namespace Extension.ConfigurationRelated
{
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
            _watcher.Renamed += (a, e) =>
            {
                if (string.Compare(_path.FilePath, e.FullPath, StringComparison.InvariantCultureIgnoreCase) != 0)
                {
                    //save changes in the file opened in VS is a two step procedure - rename target file to temp filename, and rename other temp file to a target filename
                    //we need to filter the first, and raise after the second
                    return;
                }

                ConfigurationFileChangedEventInvoke();

            };

            _watcher.EnableRaisingEvents = true;
        }

        #region IConnectionStringContainer

        public SqlExecutorTypeEnum ExecutorType
        {
            get
            {
                if (!TryRead(out var configuration))
                {
                    throw new InvalidOperationException("Cannot read configuration");
                }

                var executor = configuration.SqlExecutors.SqlExecutor.First(j => j.IsDefault);

                Enum.TryParse<SqlExecutorTypeEnum>(executor.Type, true, out var result);

                return
                    result;
            }
        }

        public string GetConnectionString()
        {
            if (!TryRead(out var configuration))
            {
                throw new InvalidOperationException("Cannot read configuration");
            }

            var executor = configuration.SqlExecutors.SqlExecutor.First(j => j.IsDefault);

            return
                executor.ConnectionString;
        }

        public bool TryGetParameter(string parameterName, out string parameterValue)
        {
            if (!TryRead(out var configuration))
            {
                throw new InvalidOperationException("Cannot read configuration");
            }

            var executor = configuration.SqlExecutors.SqlExecutor.First(j => j.IsDefault);

            return
                executor.TryGetParameter(parameterName, out parameterValue);
        }

        #endregion

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
