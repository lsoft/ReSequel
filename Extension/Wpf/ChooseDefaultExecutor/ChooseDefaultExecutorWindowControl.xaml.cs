using Extension.Cache;
using Extension.ConfigurationRelated;
using Main;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Controls;

namespace Extension.Wpf.ChooseDefaultExecutor
{
    /// <summary>
    /// Interaction logic for ChooseDefaultExecutorWindowControl.
    /// </summary>
    public partial class ChooseDefaultExecutorWindowControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChooseDefaultExecutorWindowControl"/> class.
        /// </summary>
        public ChooseDefaultExecutorWindowControl(
            IConfigurationProvider configurationProvider,
            SqlInclusionCache cache
            )
        {
            if (configurationProvider == null)
            {
                throw new System.ArgumentNullException(nameof(configurationProvider));
            }

            if (cache == null)
            {
                throw new System.ArgumentNullException(nameof(cache));
            }

            var viewmodel = new ChooseDefaultExecutorViewModel(
                this.Dispatcher,
                configurationProvider,
                cache
                );
            this.DataContext = viewmodel;

            this.InitializeComponent();
        }

        ///// <summary>
        ///// Handles click on the button by displaying a message box.
        ///// </summary>
        ///// <param name="sender">The event sender.</param>
        ///// <param name="e">The event args.</param>
        //[SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions", Justification = "Sample code")]
        //[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Default event handler naming pattern")]
        //private void button1_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show(
        //        string.Format(System.Globalization.CultureInfo.CurrentUICulture, "Invoked '{0}'", this.ToString()),
        //        "ChooseDefaultExecutorWindow");
        //}
    }

}