using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Other
{
    internal static class VisualStudioHelper
    {
        public static void OpenFile(
            EnvDTE.DTE dte,
            string documentFullPath
            )
        {
            ThreadHelper.ThrowIfNotOnUIThread(nameof(VisualStudioHelper.OpenAndNavigate));

#if VS2019
            Process.Start(
                "notepad.exe",
                documentFullPath
                );
#else
            dte.ItemOperations.OpenFile(documentFullPath, "{" + VSConstants.LOGVIEWID_Code + "}");
#endif
        }

        public static void OpenAndNavigate(
            string documentFullPath,
            int startLine,
            int startColumn,
            int endLine,
            int endColumn
            )
        {
            if (documentFullPath == null)
            {
                throw new ArgumentNullException(nameof(documentFullPath));
            }

            //VsShellUtilities.OpenDocument(
            //    _serviceProvider,
            //    inclusionWrapper.Inclusion.Inclusion.FilePath
            //    );

            ThreadHelper.ThrowIfNotOnUIThread(nameof(VisualStudioHelper.OpenAndNavigate));

            var openDoc = AsyncPackage.GetGlobalService(typeof(IVsUIShellOpenDocument)) as IVsUIShellOpenDocument;

            IVsWindowFrame frame;
            Microsoft.VisualStudio.OLE.Interop.IServiceProvider sp;
            IVsUIHierarchy hier;
            uint itemid;
            Guid logicalView = VSConstants.LOGVIEWID_Code;

            if (ErrorHandler.Failed(
                openDoc.OpenDocumentViaProject(
                    documentFullPath,
                    ref logicalView,
                    out sp,
                    out hier,
                    out itemid,
                    out frame)
                )
                || frame == null)
            {
                return;
            }

            object docData;
            frame.GetProperty((int)__VSFPROPID.VSFPROPID_DocData, out docData);

            // Get the VsTextBuffer  
            var buffer = docData as VsTextBuffer;
            if (buffer == null)
            {
                IVsTextBufferProvider bufferProvider = docData as IVsTextBufferProvider;
                if (bufferProvider != null)
                {
                    IVsTextLines lines;
                    ErrorHandler.ThrowOnFailure(
                        bufferProvider.GetTextBuffer(out lines)
                        );

                    buffer = lines as VsTextBuffer;

                    Debug.Assert(buffer != null, "IVsTextLines does not implement IVsTextBuffer");

                    if (buffer == null)
                    {
                        return;
                    }
                }
            }

            IVsTextManager textManager = Package.GetGlobalService(typeof(VsTextManagerClass)) as IVsTextManager;

            var docViewType = default(Guid);

            //textManager.NavigateToPosition(
            //    buffer,
            //    ref docViewType,
            //    inclusionWrapper.Inclusion.Inclusion.Start.Line,
            //    inclusionWrapper.Inclusion.Inclusion.Start.Character
            //    );

            textManager.NavigateToLineAndColumn(
                buffer,
                ref docViewType,
                startLine,
                startColumn,
                endLine,
                endColumn
                );
        }

    }
}
