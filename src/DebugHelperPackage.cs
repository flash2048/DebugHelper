using Microsoft;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Threading;
using DebugHelper.Commands;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell.Interop;
using DebugHelper.Commands.Internal;
using EnvDTE;
using EnvDTE80;
using System.Windows;
using DebugHelper.Options;

namespace DebugHelper
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideOptionPage(typeof(DebugHelperOptions), "Debug Helper", "General", 0, 0, true)]
    [Guid(PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class DebugHelperPackage : AsyncPackage
    {
        private List<IDisposable> _menuItems;
        private OleMenuCommandService _menuCommandService;
        private CommandHelper _commandHelper;
        private DebugHelperOptions _options;

        public const string PackageGuidString = "5c3697ac-169b-447d-b376-1af0cc303f29";

        #region Package Members

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            var dte2 = (DTE2)GetGlobalService(typeof(DTE));

            _options = (DebugHelperOptions)GetDialogPage(typeof(DebugHelperOptions));

            _menuCommandService = (OleMenuCommandService)await GetServiceAsync(typeof(IMenuCommandService));
            Assumes.Present(_menuCommandService);

            var styleResources = new Dictionary<ThemeStyle, ResourceDictionary>
            {
                {
                    ThemeStyle.Dark, new ResourceDictionary
                    {
                        Source = new Uri("pack://application:,,,/DebugHelper;component/Styles/DarkTheme.xaml")
                    }
                },
                {
                    ThemeStyle.Light, new ResourceDictionary
                    {
                        Source = new Uri("pack://application:,,,/DebugHelper;component/Styles/LightTheme.xaml")
                    }
                }
            };

            _commandHelper = new CommandHelper(dte2, this);

            _menuItems = new IMenuCommand[]
            {
                (new ObjectExplorerCommand(this, dte2, styleResources, _options)),
                (new ExportCommand(this, dte2, styleResources, _options)),
            }.Select(AddMenuCommand).ToList();
        }

        private void BeforeQueryStatus(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (!(sender is OleMenuCommand menuCommand)) return;

            var isCommandMenuAvailable = _commandHelper.IsCommandMenuAvailable();
            menuCommand.Visible = isCommandMenuAvailable;
            menuCommand.Enabled = isCommandMenuAvailable;
        }

        public IDisposable AddMenuCommand(IMenuCommand menuCommand)
        {
            var menuItem = new DisposableMenuCommand(menuCommand.Execute, BeforeQueryStatus, menuCommand.CommandId);
            _menuCommandService.AddCommand(menuItem);
            return menuItem;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _menuCommandService.Dispose();

                foreach (var menuItem in _menuItems)
                {
                    menuItem.Dispose();
                }
                _menuItems.Clear();
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}
