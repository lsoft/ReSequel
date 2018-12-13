using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Extension.CompositionRoot;
using Extension.ConfigurationRelated;
using Extension.Other;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Ninject;
using Ninject.Parameters;
using Ninject.Syntax;
using Task = System.Threading.Tasks.Task;

namespace Extension.Command
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class EditXmlCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("8680faa3-9c53-40fa-b803-a847699df6c2");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;
        private readonly EnvDTE.DTE _dte;
        private readonly ConfigurationFilePath _path;
        private readonly IConfigurationProvider _configurationProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="EditXmlCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private EditXmlCommand(
            AsyncPackage package,
            OleMenuCommandService commandService,
            EnvDTE.DTE dte,
            ConfigurationFilePath path,
            IConfigurationProvider configurationProvider
            )
        {
            if (dte == null)
            {
                throw new ArgumentNullException(nameof(dte));
            }

            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (configurationProvider == null)
            {
                throw new ArgumentNullException(nameof(configurationProvider));
            }

            this.package = package ?? throw new ArgumentNullException(nameof(package));
            this._dte = dte;
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            _path = path;
            _configurationProvider = configurationProvider;

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static EditXmlCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(
            AsyncPackage package,
            IKernel kernel
            )
        {
            if (kernel == null)
            {
                throw new ArgumentNullException(nameof(kernel));
            }

            // Switch to the main thread - the call to AddCommand in EditXmlCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            
            Instance = new EditXmlCommand(
                package,
                commandService,
                kernel.Get<EnvDTE.DTE>(),
                kernel.Get<ConfigurationFilePath>(),
                kernel.Get<IConfigurationProvider>()
                );
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread(nameof(EditXmlCommand.Execute));

            if (_path.IsFileExists)
            {
                VisualStudioHelper.OpenFile(
                    _dte,
                    _path.FilePath
                    );

                Configuration configuration;
                if (_configurationProvider.TryRead(out configuration))
                {
                    if(configuration.IsScanSchemeExists)
                    {
                        VisualStudioHelper.OpenFile(
                            _dte,
                            configuration.FullPathToScanScheme
                            );
                        //VisualStudioHelper.OpenAndNavigate(
                        //    configuration.FullPathToScanScheme,
                        //    0,
                        //    0,
                        //    0,
                        //    0
                        //    );
                    }
                }
            }
        }
    }
}
