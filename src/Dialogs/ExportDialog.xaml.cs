using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DebugHelper.Extensions;
using DebugHelper.Options;
using DebugHelper.Utilities;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.Win32;

namespace DebugHelper.Dialogs
{
    /// <summary>
    /// Interaction logic for ExportDialog.xaml
    /// </summary>
    // ReSharper disable once RedundantExtendsListEntry 
    public partial class ExportDialog : Window
    {
        private readonly DTE2 _dte2;
        private string _objectName;
        private int _maxDepthValue;
        public ExportDialog(string objectName, DTE2 dte2, DebugHelperOptions debugHelperOptions)
        {
            _dte2 = dte2;
            _objectName = objectName;

            InitializeComponent();

            CodeObject.Text = objectName;

            _maxDepthValue = debugHelperOptions.ExportDepth;
            MaxDepth.Text = _maxDepthValue.ToString();

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
            if (!(Tabs.SelectedItem is TabItem tabItem))
                throw new System.Exception("No tab selected");

            switch (tabItem.Header)
            {
                case "C#":
                    var resultString = _dte2.GetExpressionResultString(GetExpressionString(DumpStyle.CSharp));
                    CSharpEditor.Text = resultString;
                    CSharpEditor.IsReadOnly = false;
                    break;
                case "Console":
                    resultString = _dte2.GetExpressionResultString(GetExpressionString(DumpStyle.Console));
                    ConsoleEditor.Text = resultString;
                    ConsoleEditor.IsReadOnly = false;
                    break;
            }
        }

        private string GetExpressionString(DumpStyle dumpStyle)
        {
            return $"ObjectDumper.Dump({_objectName}, new DumpOptions(){{MaxLevel = {_maxDepthValue},DumpStyle = DumpStyle.{dumpStyle}, UseTypeFullName = {UseTypeFullName.IsChecked.ToString().ToLower()}, IgnoreIndexers = {IgnoreIndexers.IsChecked.ToString().ToLower()}, IgnoreDefaultValues = {IgnoreDefaultValues.IsChecked.ToString().ToLower()}, SetPropertiesOnly = {SetPropertiesOnly.IsChecked.ToString().ToLower()}, TrimInitialVariableName = {TrimInitialVariableName.IsChecked.ToString().ToLower()}, TrimTrailingColonName = {TrimTrailingColonName.IsChecked.ToString().ToLower()}}})";
        }

        private void Button_Dec_Click(object sender, RoutedEventArgs e)
        {
            if (_maxDepthValue <= 1)
                return;

            _maxDepthValue--;
            MaxDepth.Text = _maxDepthValue.ToString();
            GetDumpResult();
        }

        private void Button_Inc_Click(object sender, RoutedEventArgs e)
        {
            if (_maxDepthValue >= DebugHelperConstants.MaxDepthValue)
                return;

            _maxDepthValue++;
            MaxDepth.Text = _maxDepthValue.ToString();
            GetDumpResult();
        }

        private void CopyToClipboard_Click(object sender, RoutedEventArgs e)
        {
            if (!(Tabs.SelectedItem is TabItem tabItem))
                throw new System.Exception("No tab selected");

            switch (tabItem.Header)
            {
                case "C#":
                    Clipboard.SetText(CSharpEditor.Text);
                    break;
                case "Console":
                    Clipboard.SetText(ConsoleEditor.Text);
                    break;
            }
        }

        private void Button_SaveToFile_Click(object sender, RoutedEventArgs e)
        {
            if (!(Tabs.SelectedItem is TabItem tabItem))
                throw new System.Exception("No tab selected");

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Text file (*.txt)|*.txt|C# file (*.cs)|*.cs"
            };

            string text = null;
            switch (tabItem.Header)
            {
                case "C#":
                    text = CSharpEditor.Text;
                    break;
                case "Console":
                    text = ConsoleEditor.Text;
                    break;
            }

            if (saveFileDialog.ShowDialog() == true)
                File.WriteAllText(saveFileDialog.FileName, text);
        }

        private void RunDumpResult_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            _objectName = CodeObject.Text;

            GetDumpResult();
        }

        private void checkbox_Checked(object sender, RoutedEventArgs e)
        {
            GetDumpResult();
        }
    }
}
