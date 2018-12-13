namespace Extension.Wpf.ChooseDefaultExecutor
{
    using System;
    using System.Runtime.InteropServices;
    using Extension.Cache;
    using Extension.ConfigurationRelated;
    using Microsoft.VisualStudio.Shell;
    using Ninject;

    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("19dd755f-6f7a-4e1d-84ac-3468b7736388")]
    public class ChooseDefaultExecutorWindow : ToolWindowPane
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChooseDefaultExecutorWindow"/> class.
        /// </summary>
        public ChooseDefaultExecutorWindow(
            ) : base(null)
        {
            this.Caption = "List of SQL executors";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            this.Content = new ChooseDefaultExecutorWindowControl(
                CompositionRoot.Root.CurrentRoot.Kernel.Get<IConfigurationProvider>(),
                CompositionRoot.Root.CurrentRoot.Kernel.Get<SqlInclusionCache>()
                );
        }
    }
}
