﻿using DebugHelper.Utilities;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using DebugHelper.Options;
using DebugHelper.Dialogs;
namespace DebugHelper.Commands
{
    internal sealed class ExportCommand : IMenuCommand
    {
        public const int CmdId = 0x0200;
        public CommandID CommandId { get; } = new CommandID(DebugHelperConstants.CommandSet, CmdId);
        private readonly AsyncPackage _package;
        private readonly DTE2 _dte2;
        private readonly DebugHelperOptions _options;

        internal ExportCommand(AsyncPackage package, DTE2 dte2, DebugHelperOptions options)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));
            _dte2 = dte2;
            _options = options;
        }

        public void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var objectName = TextUtils.GetSelectedText(_package);
            var exportDialog = new ExportDialog(objectName, _dte2, _options)
            {
                Width = _options.ImageDefaultWidth,
                Height = _options.ImageDefaultHeight,
                Icon = DebugHelperConstants.Icon
            };

            exportDialog.ShowDialog();
        }

        public void Dispose()
        {
        }
    }
}
