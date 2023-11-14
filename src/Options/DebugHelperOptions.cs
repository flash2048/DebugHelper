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
        [DefaultValue(600)]
        public int ExplorerDefaultWidth { get; set; } = 600;

        [Category(" Explorer Settings")]
        [DisplayName("Default Height:")]
        [Description("Default Height")]
        [DefaultValue(400)]
        public int ExplorerDefaultHeight { get; set; } = 400;

        [Category(" Export Settings")]
        [DisplayName("Default Depth:")]
        [Description("Default Depth")]
        [DefaultValue(3)]
        public int ExportDepth { get; set; } = 3;

        [Category(" Export Settings")]
        [DisplayName("Default Width:")]
        [Description("Default Width")]
        [DefaultValue(600)]
        public int ExportDefaultWidth { get; set; } = 600;

        [Category(" Export Settings")]
        [DisplayName("Default Height:")]
        [Description("Default Height")]
        [DefaultValue(400)]
        public int ExportDefaultHeight { get; set; } = 400;
    }
}
