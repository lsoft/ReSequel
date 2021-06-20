using Extension.ConfigurationRelated;
using System.Windows.Media;

namespace Extension.Wpf.ChooseDefaultExecutor
{
    public class ExecutorWrapper
    {
        public ConfigurationSqlExecutorsSqlExecutor Executor
        {
            get;
        }

        public Brush Background
        {
            get
            {
                return
                    Executor.IsDefault ? new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x44, 0x00, 0x00, 0xff)) : Brushes.Transparent;
            }
        }

        public ExecutorWrapper(
            ConfigurationSqlExecutorsSqlExecutor executor
            )
        {
            if (executor == null)
            {
                throw new System.ArgumentNullException(nameof(executor));
            }

            Executor = executor;
        }

    }

}