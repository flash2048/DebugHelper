using System.Windows;
using Microsoft.VisualStudio.PlatformUI;

namespace DebugHelper.Extensions
{
    public static class DialogWindowExtension
    {
        public static void AddResourceDictionary(this DialogWindow dialogWindow, ResourceDictionary resourceDictionary)
        {
            dialogWindow.Resources.MergedDictionaries.Add(resourceDictionary);
        }
    }
}
