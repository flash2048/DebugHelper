﻿using Microsoft.VisualStudio.Shell;
using System.ComponentModel;

namespace DebugHelper.Options
{
    public class DebugHelperOptions : DialogPage
    {
        [Category(" Export Settings")]
        [DisplayName("Default Depth:")]
        [Description("Default Depth")]
        [DefaultValue(3)]
        public int ExportDepth { get; set; } = 5;

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
