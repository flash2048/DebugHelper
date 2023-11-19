using System.Collections.ObjectModel;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Controls;
using System;
using System.Windows.Media;
using System.Windows.Input;
using DebugHelper.Options;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Expression = EnvDTE.Expression;
using Window = System.Windows.Window;

namespace DebugHelper.Dialogs
{
    /// <summary>
    /// Interaction logic for ObjectExplorer.xaml
    /// </summary>
    // ReSharper disable once RedundantExtendsListEntry
    public partial class ObjectExplorer : Window
    {
        private readonly Expression _expression;
        private readonly DTE2 _dte2;
        private readonly SolidColorBrush _defaultSearchColorBrush;
        private readonly DebugHelperOptions _debugHelperOptions;
        private int _maxDepthValue;
        private string _objectName;

        public ObjectExplorer(string objectName, Expression expression, DTE2 dte2, DebugHelperOptions debugHelperOptions)
        {
            _objectName = objectName;
            _expression = expression;
            _dte2 = dte2;
            InitializeComponent();
            Variables.Text = _objectName;
            _defaultSearchColorBrush = Search.Foreground as SolidColorBrush;
            Search.Foreground = Brushes.Gray;
            _debugHelperOptions = debugHelperOptions;
            _maxDepthValue = _debugHelperOptions.SearchDepth;
            MaxDepth.Text = _maxDepthValue.ToString();
            InitTree();
        }

        private void SearchAndFilterDataItemsWithoutRecursion(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                ObjectTree.ItemsSource = new ObservableCollection<Expression> { _expression };
                return;
            }

            var itemsQueue = new Queue<(TreeViewItem, int)>();
            var itemsToRecheck = new List<TreeViewItem>();

            // Start with the top-level items
            foreach (var item in ObjectTree.Items)
            {
                if (ObjectTree.ItemContainerGenerator.ContainerFromItem(item) is TreeViewItem treeViewItem)
                {
                    itemsQueue.Enqueue((treeViewItem, 1));
                }
            }

            while (itemsQueue.Count > 0)
            {
                var (currentTreeViewItem, deep) = itemsQueue.Dequeue();
                var dataItem = currentTreeViewItem.DataContext as Expression;

                currentTreeViewItem.IsExpanded = true;
                currentTreeViewItem.UpdateLayout();

                if (dataItem?.Name?.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    dataItem?.Value?.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    currentTreeViewItem.Visibility = Visibility.Visible;
                    currentTreeViewItem.IsSelected = true;
                    currentTreeViewItem.BringIntoView();
                }
                else
                {
                    currentTreeViewItem.Visibility = Visibility.Collapsed;
                    currentTreeViewItem.IsSelected = false;
                }

                // Add child items to the queue
                foreach (var childItem in currentTreeViewItem.Items)
                {
                    if (deep <= _maxDepthValue && currentTreeViewItem.ItemContainerGenerator.ContainerFromItem(childItem) is TreeViewItem childTreeViewItem)
                    {
                        itemsQueue.Enqueue((childTreeViewItem, deep + 1));
                        itemsToRecheck.Add(childTreeViewItem);
                    }
                }
            }

            foreach (var treeViewItem in itemsToRecheck)
            {
                if (treeViewItem.Visibility != Visibility.Visible)
                    continue;

                var parentTreeViewItem = FindParentTreeViewItem(treeViewItem);
                while (parentTreeViewItem != null)
                {
                    parentTreeViewItem.Visibility = Visibility.Visible;
                    parentTreeViewItem = FindParentTreeViewItem(parentTreeViewItem);
                }
            }
        }

        private TreeViewItem FindParentTreeViewItem(TreeViewItem childItem)
        {
            var parent = VisualTreeHelper.GetParent(childItem);
            while (!(parent is TreeViewItem || parent is TreeView) && parent != null)
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            return parent as TreeViewItem;
        }


        private void Search_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            CustomGotKeyboardFocus(sender);
        }

        private void CustomGotKeyboardFocus(object sender)
        {
            if (!(sender is TextBox textBox))
                return;
            if (textBox.Foreground != Brushes.Gray)
                return;

            textBox.Text = "";
            textBox.Foreground = _defaultSearchColorBrush;
        }

        private void CustomLostKeyboardFocus(object sender)
        {
            if (!(sender is TextBox textBox))
                return;
            if (!textBox.Text.Trim().Equals(""))
                return;

            textBox.Foreground = Brushes.Gray;
            textBox.Text = "Search";
        }

        private void Search_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            CustomLostKeyboardFocus(sender);
        }

        private void Search_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            SearchAndFilterDataItemsWithoutRecursion(Search.Text);
        }

        private void Init_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            _objectName = Variables.Text;
            InitTree();
        }

        private void InitTree()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var customExpression = _dte2.Debugger.GetExpression(_objectName);
            if (customExpression != null)
            {
                ObjectTree.ItemsSource = new ObservableCollection<Expression> { customExpression };
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var exportDialog = new ExportDialog(_objectName, _dte2, _debugHelperOptions)
            {
                Width = _debugHelperOptions.ExportDefaultWidth,
                Height = _debugHelperOptions.ExportDefaultHeight,
            };
            exportDialog.ShowDialog();
        }

        private void Button_Dec_Click(object sender, RoutedEventArgs e)
        {
            if (_maxDepthValue <= 1)
                return;

            _maxDepthValue--;
            MaxDepth.Text = _maxDepthValue.ToString();
            if (Search.Foreground != Brushes.Gray && !string.IsNullOrEmpty(Search.Text))
                SearchAndFilterDataItemsWithoutRecursion(Search.Text);
        }

        private void Button_Inc_Click(object sender, RoutedEventArgs e)
        {
            if (_maxDepthValue >= DebugHelperConstants.MaxDepthValue)
                return;

            _maxDepthValue++;
            MaxDepth.Text = _maxDepthValue.ToString();
            if (Search.Foreground != Brushes.Gray && !string.IsNullOrEmpty(Search.Text))
                SearchAndFilterDataItemsWithoutRecursion(Search.Text);
        }
    }
}
