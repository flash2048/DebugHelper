using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using DebugHelper.AvalonEdit;
using DebugHelper.Extensions;
using DebugHelper.Options;
using DebugHelper.Utilities;
using EnvDTE80;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Indentation;
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
        private FoldingManager _foldingManagerCsharp;
        private FoldingManager _foldingManagerJson;
        private AbstractFoldingStrategy _foldingStrategyCsharp;
        private bool _jsonChanged;
        private bool _csharpChanged;

        public ExportDialog(string objectName, DTE2 dte2, DebugHelperOptions debugHelperOptions)
        {
            _dte2 = dte2;
            _objectName = objectName;

            InitializeComponent();

            CodeObject.Text = objectName;

            _maxDepthValue = debugHelperOptions.ExportDepth;
            MaxDepth.Text = _maxDepthValue.ToString();

            LoadAssembly();
            FoldingInit();
            GetDumpResult();
        }

        private void FoldingInit()
        {
            var foldingUpdateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            foldingUpdateTimer.Tick += FoldingUpdateTimer_Tick;
            foldingUpdateTimer.Start();

            CSharpEditor.TextChanged += (sender, args) => _csharpChanged = true;
            JsonEditor.TextChanged += (sender, args) => _jsonChanged = true;

            CSharpEditor.TextArea.IndentationStrategy =
                new ICSharpCode.AvalonEdit.Indentation.CSharp.CSharpIndentationStrategy(CSharpEditor.Options);
            JsonEditor.TextArea.IndentationStrategy = new DefaultIndentationStrategy();
            _foldingStrategyCsharp = new BraceFoldingStrategy();

            _foldingManagerCsharp = FoldingManager.Install(CSharpEditor.TextArea);
            _foldingManagerJson = FoldingManager.Install(JsonEditor.TextArea);

            _foldingStrategyCsharp.UpdateFoldings(_foldingManagerCsharp, CSharpEditor.Document);
            _foldingStrategyCsharp.UpdateFoldings(_foldingManagerJson, JsonEditor.Document);
        }

        private void LoadAssembly()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var frameworkVersionString = _dte2.GetFrameworkVersionString();

            var path = FrameworkVersionUtils.GetObjectDumpingDllPath(frameworkVersionString);
            var expressionString = $"System.Reflection.Assembly.LoadFile(@\"{path}\")";
            _dte2.Debugger.GetExpression(expressionString, Timeout: DebugHelperConstants.DebuggerExpressionTimeoutMilliseconds);

            path = FrameworkVersionUtils.GetNewtonsoftJsonDllPath(frameworkVersionString);
            expressionString = $"System.Reflection.Assembly.LoadFile(@\"{path}\")";
            _dte2.Debugger.GetExpression(expressionString, Timeout: DebugHelperConstants.DebuggerExpressionTimeoutMilliseconds);
        }

        private void GetDumpResult()
        {
            if (!(Tabs.SelectedItem is TabItem tabItem))
                throw new Exception("No tab selected");

            switch (tabItem.Header)
            {
                case DebugHelperConstants.CsharpName:
                    ShowObjectDumpOptions();
                    SaveImage.Source = new BitmapImage(new Uri("pack://application:,,,/DebugHelper;component/imgs/csharp.png"));

                    using (var temporaryFile = new TemporaryFile())
                    {
                        var expressionResultTrim = _dte2.GetExpressionResultString(GetExpressionString("CSharp", temporaryFile.FileName));
                        var result = temporaryFile.ReadAllText();

                        CSharpEditor.Text = !string.IsNullOrEmpty(result) ? result : expressionResultTrim;
                        CSharpEditor.IsReadOnly = false;
                    }
                    break;
                case DebugHelperConstants.ConsoleName:
                    ShowObjectDumpOptions();
                    SaveImage.Source = new BitmapImage(new Uri("pack://application:,,,/DebugHelper;component/imgs/file.png"));

                    using (var temporaryFile = new TemporaryFile())
                    {
                        var expressionResultTrim = _dte2.GetExpressionResultString(GetExpressionString("Console", temporaryFile.FileName));
                        var result = temporaryFile.ReadAllText();

                        ConsoleEditor.Text = !string.IsNullOrEmpty(result) ? result : expressionResultTrim;
                        ConsoleEditor.IsReadOnly = false;
                    }
                    break;
                case DebugHelperConstants.JsonName:
                    ShowJsonOptions();
                    SaveImage.Source = new BitmapImage(new Uri("pack://application:,,,/DebugHelper;component/imgs/json.png"));

                    using (var temporaryFile = new TemporaryFile())
                    {
                        var expressionResultTrim = _dte2.GetExpressionResultString(GetExpressionJsonString(temporaryFile.FileName));
                        var result = temporaryFile.ReadAllText();

                        JsonEditor.Text = !string.IsNullOrEmpty(result) ? result : expressionResultTrim;
                        JsonEditor.IsReadOnly = false;
                    }
                    break;
            }
        }

        private void ShowJsonOptions()
        {
            UseTypeFullName.Visibility = Visibility.Visible;
            IgnoreIndexers.Visibility = Visibility.Hidden;
            IgnoreDefaultValues.Visibility = Visibility.Hidden;
            SetPropertiesOnly.Visibility = Visibility.Hidden;
            TrimInitialVariableName.Visibility = Visibility.Hidden;
            TrimTrailingColonName.Visibility = Visibility.Hidden;
        }

        private void ShowObjectDumpOptions()
        {
            UseTypeFullName.Visibility = Visibility.Visible;
            IgnoreIndexers.Visibility = Visibility.Visible;
            IgnoreDefaultValues.Visibility = Visibility.Visible;
            SetPropertiesOnly.Visibility = Visibility.Visible;
            TrimInitialVariableName.Visibility = Visibility.Visible;
            TrimTrailingColonName.Visibility = Visibility.Visible;
        }

        private string GetExpressionString(string dumpStyle, string fileName)
        {
            return $"System.IO.File.WriteAllText(@\"{fileName}\", ObjectDumper.Dump({_objectName}, new DumpOptions(){{MaxLevel = {_maxDepthValue},DumpStyle = DumpStyle.{dumpStyle}, UseTypeFullName = {UseTypeFullName.IsChecked.ToString().ToLower()}, IgnoreIndexers = {IgnoreIndexers.IsChecked.ToString().ToLower()}, IgnoreDefaultValues = {IgnoreDefaultValues.IsChecked.ToString().ToLower()}, SetPropertiesOnly = {SetPropertiesOnly.IsChecked.ToString().ToLower()}, TrimInitialVariableName = {TrimInitialVariableName.IsChecked.ToString().ToLower()}, TrimTrailingColonName = {TrimTrailingColonName.IsChecked.ToString().ToLower()}}}))";
        }

        private string GetExpressionJsonString(string fileName)
        {
            return $"System.IO.File.WriteAllText(@\"{fileName}\", Newtonsoft.Json.JsonConvert.SerializeObject({_objectName}, new Newtonsoft.Json.JsonSerializerSettings() {{ MaxDepth = {(_maxDepthValue > 1 ? _maxDepthValue : 1)},{(UseTypeFullName.IsChecked == true ? "TypeNameHandling = Newtonsoft.Json.TypeNameHandling.All," : string.Empty)} ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore, Formatting = Newtonsoft.Json.Formatting.Indented }}))";
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
                throw new Exception("No tab selected");

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
                throw new Exception("No tab selected");

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
                    saveFileDialog.Filter = "Json (*.json)|*.json|Text file (*.txt)|*.txt|All files (*.*)|*.*";
                    break;
            }

            if (saveFileDialog.ShowDialog() == true)
                File.WriteAllText(saveFileDialog.FileName, text);
        }

        private void FoldingUpdateTimer_Tick(object sender, EventArgs e)
        {
            if (_foldingStrategyCsharp == null)
                return;

            if (!(Tabs.SelectedItem is TabItem tabItem))
                throw new Exception("No tab selected");

            switch (tabItem.Header)
            {
                case DebugHelperConstants.CsharpName:
                    if (_csharpChanged)
                        _foldingStrategyCsharp.UpdateFoldings(_foldingManagerCsharp, CSharpEditor.Document);
                    break;
                case DebugHelperConstants.JsonName:
                    if (_jsonChanged)
                        _foldingStrategyCsharp.UpdateFoldings(_foldingManagerJson, JsonEditor.Document);
                    break;
            }
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
