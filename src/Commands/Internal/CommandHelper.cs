using System;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;

namespace DebugHelper.Commands.Internal
{
    internal class CommandHelper
    {
        private readonly DTE2 _dte;

        public CommandHelper(DTE2 dte, IServiceProvider serviceProvider)
        {
            _dte = dte ?? throw new ArgumentNullException(nameof(dte));
        }

        public bool IsCommandMenuAvailable()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            return _dte.Debugger != null
                   && _dte.Debugger.CurrentMode == dbgDebugMode.dbgBreakMode
                   && _dte.Debugger.CurrentStackFrame != null
                   && (_dte.ActiveWindow.ObjectKind == Constants.vsDocumentKindText);

        }
    }
}
