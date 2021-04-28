using Extension.Cache;
using Extension.ExtensionStatus;
using Main.Logger;
using Main.SolutionValidator;
using Ninject;
using System.Windows.Controls;

namespace Extension.Wpf.InclusionList
{
    /// <summary>
    /// Interaction logic for InclusionListWindowControl.xaml
    /// </summary>
    public partial class InclusionListWindowControl : UserControl
    {
        public InclusionListWindowControl(
            SqlInclusionCache cache
            )
        {
            if (cache == null)
            {
                throw new System.ArgumentNullException(nameof(cache));
            }


            var viewmodel = new InclusionListViewModel(
                CompositionRoot.Root.CurrentRoot.Kernel.Get<ISolutionValidatorFactory>(),
                CompositionRoot.Root.CurrentRoot.Kernel.Get<ILastMessageProcessLogger>(),
                CompositionRoot.Root.CurrentRoot.Kernel.Get<IExtensionStatus>()
                );
            this.DataContext = viewmodel;

            InitializeComponent();
        }
    }
}
