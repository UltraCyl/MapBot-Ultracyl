using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Default.EXtensions
{
    public class Gui : UserControl
    {
        internal DataGrid InventoryCurrencyGrid;
        
        public Gui()
        {
            InitializeComponent();
        }
        
        private void InitializeComponent()
        {
            // Get the path to the XAML file
            string xamlPath = GetXamlPath("EXtensions", "Gui.xaml");
            
            if (!string.IsNullOrEmpty(xamlPath) && File.Exists(xamlPath))
            {
                try
                {
                    using (var stream = new FileStream(xamlPath, FileMode.Open, FileAccess.Read))
                    {
                        var content = XamlReader.Load(stream) as UserControl;
                        if (content != null)
                        {
                            this.Content = content.Content;
                            
                            // Find named controls
                            InventoryCurrencyGrid = FindDescendant<DataGrid>(this, "InventoryCurrencyGrid");
                            
                            // Wire up event handlers
                            var addButton = FindDescendant<Button>(this, "InventoryCurrencyAddButton");
                            if (addButton != null) addButton.Click += InventoryCurrencyAdd;
                            
                            var deleteButton = FindDescendant<Button>(this, "InventoryCurrencyDeleteButton");
                            if (deleteButton != null) deleteButton.Click += InventoryCurrencyDelete;
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.Content = new TextBlock 
                    { 
                        Text = $"Error loading GUI: {ex.Message}\nPath: {xamlPath}",
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(10)
                    };
                }
            }
            else
            {
                this.Content = new TextBlock 
                { 
                    Text = $"XAML file not found.\nSearched: {xamlPath ?? "null"}",
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(10)
                };
            }
        }
        
        private static string GetXamlPath(string folder, string filename)
        {
            // Try various paths to find the XAML file
            var basePaths = new[]
            {
                AppDomain.CurrentDomain.BaseDirectory,
                Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                Environment.CurrentDirectory
            };
            
            foreach (var basePath in basePaths)
            {
                if (string.IsNullOrEmpty(basePath)) continue;
                
                // Try: 3rdParty/MapBot/EXtensions/Gui.xaml
                var path1 = Path.Combine(basePath, "3rdParty", "MapBot", folder, filename);
                if (File.Exists(path1)) return path1;
                
                // Try: EXtensions/Gui.xaml (relative)
                var path2 = Path.Combine(basePath, folder, filename);
                if (File.Exists(path2)) return path2;
            }
            
            return null;
        }
        
        private static T FindDescendant<T>(DependencyObject parent, string name) where T : FrameworkElement
        {
            if (parent == null) return null;
            
            int childCount = System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                
                if (child is T element && element.Name == name)
                    return element;
                
                var result = FindDescendant<T>(child, name);
                if (result != null) return result;
            }
            
            return null;
        }

        private void InventoryCurrencyAdd(object sender, RoutedEventArgs e)
        {
            Settings.Instance.InventoryCurrencies.Add(new Settings.InventoryCurrency());
        }

        private void InventoryCurrencyDelete(object sender, RoutedEventArgs e)
        {
            var selected = InventoryCurrencyGrid?.SelectedItem as Settings.InventoryCurrency;
            if (selected != null) Settings.Instance.InventoryCurrencies.Remove(selected);
        }
    }
}
