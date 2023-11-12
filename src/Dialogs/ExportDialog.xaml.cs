using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DebugHelper.Extensions;
using DebugHelper.Options;
using DebugHelper.Utilities;
using EnvDTE80;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.Win32;

namespace DebugHelper.Dialogs
{
    /// <summary>
    /// Interaction logic for ExportDialog.xaml
    /// </summary>
    public partial class ExportDialog : DialogWindow
    {
        private readonly DTE2 _dte2;
        private string _objectName;
        private int _maxDepthValue;
        public ExportDialog(string objectName, ResourceDictionary resourceDictionary, DTE2 dte2, DebugHelperOptions debugHelperOptions) : base("Microsoft.VisualStudio.PlatformUI.DialogWindow")
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            this.AddResourceDictionary(resourceDictionary);
            _dte2 = dte2;
            _objectName = objectName;

            InitializeComponent();

            codeObject.Text = objectName;

            _maxDepthValue = debugHelperOptions.ExportDepth;
            maxDepth.Text = _maxDepthValue.ToString();

            LoadAssembly();

            GetDumpResult();
        }

        private void LoadAssembly()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var path = FrameworkVersionUtils.GetObjectDumpingDllPath(_dte2.GetFrameworkVersionString());
            var expressionString = $"System.Reflection.Assembly.LoadFile(@\"{path}\")";
            _dte2.Debugger.GetExpression(expressionString);
        }

        private void GetDumpResult()
        {

            var resultString = _dte2.GetExpressionResultString(GetExpressionString(DumpStyle.CSharp));
            cSharpEditor.Text = resultString;

            resultString = _dte2.GetExpressionResultString(GetExpressionString(DumpStyle.Console));
            consoleEditor.Text = resultString;
        }

        private string GetExpressionString(DumpStyle dumpStyle)
        {
            return $"ObjectDumper.Dump({_objectName}, new DumpOptions(){{MaxLevel = {_maxDepthValue},DumpStyle = DumpStyle.{dumpStyle}}})";
        }

        private void Button_Dec_Click(object sender, RoutedEventArgs e)
        {
            if (_maxDepthValue > 1)
            {
                _maxDepthValue--;
                maxDepth.Text = _maxDepthValue.ToString();
                GetDumpResult();
            }
        }

        private void Button_Inc_Click(object sender, RoutedEventArgs e)
        {
            if (_maxDepthValue < 20)
            {
                _maxDepthValue++;
                maxDepth.Text = _maxDepthValue.ToString();
                GetDumpResult();
            }
        }

        private void CopyToClipboard_Click(object sender, RoutedEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (!(tabs.SelectedItem is TabItem tabItem))
                throw new System.Exception("No tab selected");

            switch (tabItem.Header)

            {
                case "C#":
                    Clipboard.SetText(cSharpEditor.Text);
                    break;
                case "Console":
                    Clipboard.SetText(consoleEditor.Text);
                    break;
            }
        }

        private void Button_SaveToFile_Click(object sender, RoutedEventArgs e)
        {
            if (!(tabs.SelectedItem is TabItem tabItem))
                throw new System.Exception("No tab selected");

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Text file (*.txt)|*.txt|C# file (*.cs)|*.cs"
            };

            string text = null;
            switch (tabItem.Header)
            {
                case "C#":
                    text = cSharpEditor.Text;
                    break;
                case "Console":
                    text = consoleEditor.Text;
                    break;
            }

            if (saveFileDialog.ShowDialog() == true)
                File.WriteAllText(saveFileDialog.FileName, text);
        }

        private void RunDumpResult_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            _objectName = codeObject.Text;

            GetDumpResult();
        }
    }
}
