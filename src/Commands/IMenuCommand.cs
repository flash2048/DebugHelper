using System;
using System.ComponentModel.Design;

namespace DebugHelper.Commands
{
    public interface IMenuCommand: IDisposable
    {
        CommandID CommandId { get; }
        void Execute(object sender, EventArgs e);
    }
}