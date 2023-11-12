using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;

namespace DebugHelper.Commands.Internal
{
    internal sealed class DisposableMenuCommand : OleMenuCommand, IDisposable
    {
        private readonly EventHandler _beforeQueryStatusHandler;
        public DisposableMenuCommand(EventHandler executeCommandHandler, EventHandler beforeQueryStatusHandler, CommandID command)
            : base(executeCommandHandler, command)
        {
            _beforeQueryStatusHandler = beforeQueryStatusHandler ?? throw new ArgumentNullException(nameof(beforeQueryStatusHandler));
            BeforeQueryStatus += beforeQueryStatusHandler;
        }

        public void Dispose()
        {
            BeforeQueryStatus -= _beforeQueryStatusHandler;
        }
    }
}
