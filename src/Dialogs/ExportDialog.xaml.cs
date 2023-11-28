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

            var frameworkVersionString = _dte2.GetFrameworkVersionString();

            var path = FrameworkVersionUtils.GetObjectDumpingDllPath(frameworkVersionString);
            var expressionString = $"System.Reflection.Assembly.LoadFile(@\"{path}\")";
            _dte2.Debugger.GetExpression(expressionString);

            path = FrameworkVersionUtils.GetSystemTextJsonDllPath(frameworkVersionString);
            expressionString = $"System.Reflection.Assembly.LoadFile(@\"{path}\")";
            _dte2.Debugger.GetExpression(expressionString);
        }

        private void GetDumpResult()
        {
            if (!(Tabs.SelectedItem is TabItem tabItem))
                throw new System.Exception("No tab selected");

            switch (tabItem.Header)
            {
                case DebugHelperConstants.CsharpName:
                    var resultString = _dte2.GetExpressionResultString(GetExpressionString("CSharp"));
                    CSharpEditor.Text = resultString;
                    CSharpEditor.IsReadOnly = false;
                    break;
                case DebugHelperConstants.ConsoleName:
                    resultString = _dte2.GetExpressionResultString(GetExpressionString("Console"));
                    ConsoleEditor.Text = resultString;
                    ConsoleEditor.IsReadOnly = false;
                    break;
                case DebugHelperConstants.JsonName:
                    resultString = _dte2.GetExpressionResultString(GetExpressionJsonString());
                    JsonEditor.Text = resultString;
                    JsonEditor.IsReadOnly = false;
                    break;
            }
        }

        private string GetExpressionString(string dumpStyle)
        {
            return $"ObjectDumper.Dump({_objectName}, new DumpOptions(){{MaxLevel = {_maxDepthValue},DumpStyle = DumpStyle.{dumpStyle}, UseTypeFullName = {UseTypeFullName.IsChecked.ToString().ToLower()}, IgnoreIndexers = {IgnoreIndexers.IsChecked.ToString().ToLower()}, IgnoreDefaultValues = {IgnoreDefaultValues.IsChecked.ToString().ToLower()}, SetPropertiesOnly = {SetPropertiesOnly.IsChecked.ToString().ToLower()}, TrimInitialVariableName = {TrimInitialVariableName.IsChecked.ToString().ToLower()}, TrimTrailingColonName = {TrimTrailingColonName.IsChecked.ToString().ToLower()}}})";
        }

        private string GetExpressionJsonString()
        {
            return $"System.Text.Json.JsonSerializer.Serialize({_objectName}, new System.Text.Json.JsonSerializerOptions() {{MaxDepth = {(_maxDepthValue < 1 ? _maxDepthValue : 1)}, WriteIndented = true}})";
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
                case DebugHelperConstants.CsharpName:
                    Clipboard.SetText(CSharpEditor.Text);
                    break;
                case DebugHelperConstants.ConsoleName:
                    Clipboard.SetText(ConsoleEditor.Text);
                    break;
                case DebugHelperConstants.JsonName:
                    Clipboard.SetText(JsonEditor.Text);
                    break;
            }
        }

        private void Button_SaveToFile_Click(object sender, RoutedEventArgs e)
        {
            if (!(Tabs.SelectedItem is TabItem tabItem))
                throw new System.Exception("No tab selected");

            var saveFileDialog = new SaveFileDialog();

            string text = null;
            switch (tabItem.Header)
            {
                case DebugHelperConstants.CsharpName:
                    text = CSharpEditor.Text;
                    saveFileDialog.Filter = "C# file (*.cs)|*.cs|Text file (*.txt)|*.txt";
                    break;
                case DebugHelperConstants.ConsoleName:
                    text = ConsoleEditor.Text;
                    saveFileDialog.Filter = "Text file (*.txt)|*.txt";
                    break;
                case DebugHelperConstants.JsonName:
                    text = JsonEditor.Text;
                    saveFileDialog.Filter = "Json file (*.json)|*.json|Text file (*.txt)|*.txt";
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

        private void Checkbox_Checked(object sender, RoutedEventArgs e)
        {
            GetDumpResult();
        }

        private void Tabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetDumpResult();
        }
    }
}
