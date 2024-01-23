using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using DebugHelper.Extensions;
using DebugHelper.Options;
using DebugHelper.Utilities;
using EnvDTE80;

namespace DebugHelper.Dialogs
{
    /// <summary>
    /// Interaction logic for ImageDialog.xaml
    /// </summary>
    public partial class ImageDialog : Window
    {
        private readonly DTE2 _dte2;
        private string _objectName;

        public ImageDialog(string objectName, DTE2 dte2, DebugHelperOptions _)
        {
            _dte2 = dte2;
            _objectName = objectName;

            InitializeComponent();

            CodeObject.Text = objectName;

            ShowImageObjects();
            ShowImage();
        }

        private void ShowImage()
        {
            ShowImageObjects();
            Image.Source = null;

            var variableType = _dte2.GetExpressionResultString(ExpressionStrings.GetVariableType(_objectName));

            using (var temporaryFile = new TemporaryFile())
            {
                _dte2.GetExpressionResultString(ExpressionStrings.GetSaveString(temporaryFile.FileName, variableType, _objectName));
                var result = temporaryFile.ReadAllText();

                if (!string.IsNullOrEmpty(result))
                {
                    try
                    {
                        Image.Source = TextUtils.GetBitmapImageFromString(result);
                    }
                    catch (Exception e)
                    {
                        HideImageObjects();
                        ExceptionText.Content = e.Message;
                    }
                }
                else
                {
                    HideImageObjects();
                }
            }
        }

        private void RunDumpResult_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            _objectName = CodeObject.Text;

            ShowImage();
        }

        private void ShowImageObjects()
        {
            ErrorText.Visibility = Visibility.Hidden;
            ExceptionText.Visibility = Visibility.Hidden;
            Image.Visibility = Visibility.Visible;
        }
        private void HideImageObjects()
        {
            ErrorText.Visibility = Visibility.Visible;
            ExceptionText.Visibility = Visibility.Visible;
            Image.Visibility = Visibility.Hidden;
        }

        private void Button_SaveToFile_Click(object sender, RoutedEventArgs e)
        {
            if (Image.Source != null)
            {
                var bitmapImage = (BitmapImage)Image.Source;
                var bitmapEncoder = new PngBitmapEncoder();
                bitmapEncoder.Frames.Add(BitmapFrame.Create(bitmapImage));

                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "PNG Image|*.png",
                    Title = "Save an Image File"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    using (var fileStream =
                           new System.IO.FileStream(saveFileDialog.FileName, System.IO.FileMode.Create))
                    {
                        bitmapEncoder.Save(fileStream);
                    }
                }
            }
            else
            {
                MessageBox.Show("No image to save");
            }
        }
    }
}
