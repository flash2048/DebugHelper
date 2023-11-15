using DebugHelper.Utilities;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using System.Windows;
using DebugHelper.Options;
using System.Collections.Generic;

namespace DebugHelper.Commands
{
    internal sealed class ObjectExplorerCommand : IMenuCommand
    {
        public const int CmdId = 0x0100;

        public CommandID CommandId { get; } = new CommandID(DebugHelperConstants.CommandSet, CmdId);
        private readonly AsyncPackage _package;
        private readonly DTE2 _dte2;
        private readonly Dictionary<ThemeStyle, ResourceDictionary> _styleResources;
        private readonly DebugHelperOptions _options;

        internal ObjectExplorerCommand(AsyncPackage package, DTE2 dte2, Dictionary<ThemeStyle, ResourceDictionary> styleResources, DebugHelperOptions options)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));
            _dte2 = dte2;
            _styleResources = styleResources;
            _options = options;
        }

        public void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var objectName = TextUtils.GetSelectedText(_package);
            var customExpression = _dte2.Debugger.GetExpression(objectName);

            var objectExplorer = new Dialogs.ObjectExplorer(objectName, _styleResources[_options.Theme], customExpression, _dte2, _options)
            {
                Width = _options.ExplorerDefaultWidth,
                Height = _options.ExplorerDefaultHeight
            };

            objectExplorer.ShowModal();
        }

        public void Dispose()
        {
        }
    }
}
