using Extension.Cache;
using Extension.ExtensionStatus;
using Main;
using Main.Inclusion.Scanner;
using Main.Logger;
using Main.SolutionValidator;
using Main.WorkspaceWrapper;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
                this.Dispatcher,
                CompositionRoot.Root.CurrentRoot.Kernel.Get<ISolutionValidatorFactory>(),
                CompositionRoot.Root.CurrentRoot.Kernel.Get<ILastMessageProcessLogger>(),
                CompositionRoot.Root.CurrentRoot.Kernel.Get<IExtensionStatus>()
                );
            this.DataContext = viewmodel;

            InitializeComponent();
        }
    }
}
