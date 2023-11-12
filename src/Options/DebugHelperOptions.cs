using Microsoft.VisualStudio.Shell;
using System.ComponentModel;

namespace DebugHelper.Options
{
    public class DebugHelperOptions : DialogPage
    {
        [Category(" Common Settings")]
        [DisplayName("Color Theme:")]
        [Description("Color Theme")]
        [DefaultValue(ThemeStyle.Dark)]
        public ThemeStyle Theme { get; set; } = ThemeStyle.Dark;

        [Category(" Explorer Settings")]
        [DisplayName("Default Search Depth:")]
        [Description("Default Search Depth")]
        [DefaultValue(3)]
        public int SearchDepth { get; set; } = 3;

        [Category(" Explorer Settings")]
        [DisplayName("Default Width:")]
        [Description("Default Width")]
        [DefaultValue(500)]
        public int ExplorerDefaultWidth { get; set; } = 500;

        [Category(" Explorer Settings")]
        [DisplayName("Default Height:")]
        [Description("Default Height")]
        [DefaultValue(300)]
        public int ExplorerDefaultHeight { get; set; } = 300;

        [Category(" Export Settings")]
        [DisplayName("Default Depth:")]
        [Description("Default Depth")]
        [DefaultValue(3)]
        public int ExportDepth { get; set; } = 3;

        [Category(" Export Settings")]
        [DisplayName("Default Width:")]
        [Description("Default Width")]
        [DefaultValue(500)]
        public int ExportDefaultWidth { get; set; } = 500;

        [Category(" Export Settings")]
        [DisplayName("Default Height:")]
        [Description("Default Height")]
        [DefaultValue(300)]
        public int ExportDefaultHeight { get; set; } = 300;
    }
}
