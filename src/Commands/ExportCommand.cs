using DebugHelper.Utilities;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using System.Windows;
using DebugHelper.Options;
using System.Collections.Generic;
using DebugHelper.Dialogs;

namespace DebugHelper.Commands
{
    internal sealed class ExportCommand : IMenuCommand
    {
        public const int CmdId = 0x0200;
        public CommandID CommandId { get; } = new CommandID(DebugHelperConstants.CommandSet, CmdId);
        private readonly AsyncPackage _package;
        private readonly DTE2 _dte2;
        private readonly Dictionary<ThemeStyle, ResourceDictionary> _styleResources;
        private readonly DebugHelperOptions _options;

        internal ExportCommand(AsyncPackage package, DTE2 dte2, Dictionary<ThemeStyle, ResourceDictionary> styleResources, DebugHelperOptions options)
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
            var exportDialog = new ExportDialog(objectName, _styleResources[_options.Theme], _dte2, _options)
            {
                Width = _options.ExportDefaultWidth,
                Height = _options.ExportDefaultHeight
            };

            exportDialog.Show();
            System.Windows.Threading.Dispatcher.Run();

        }

        public void Dispose()
        {
        }
    }
}
