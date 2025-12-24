using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using Default.EXtensions;

namespace Default.MapBot
{
    public class Gui : UserControl
    {
        private readonly ToolTip _toolTip = new ToolTip();
        private ICollectionView _mapCollectionView;
        private ICollectionView _affixCollectionView;
        
        internal TextBox MapSearchTextBox;
        internal DataGrid MapGrid;
        internal TextBox AffixSearchTextBox;
        internal DataGrid AffixGrid;

        public Gui()
        {
            InitializeComponent();
        }
        
        private void InitializeComponent()
        {
            // Get the path to the XAML file
            string xamlPath = GetXamlPath("MapBot", "Gui.xaml");
            
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
                            MapSearchTextBox = FindDescendant<TextBox>(this, "MapSearchTextBox");
                            MapGrid = FindDescendant<DataGrid>(this, "MapGrid");
                            AffixSearchTextBox = FindDescendant<TextBox>(this, "AffixSearchTextBox");
                            AffixGrid = FindDescendant<DataGrid>(this, "AffixGrid");
                            
                            // Wire up event handlers
                            if (MapSearchTextBox != null)
                                MapSearchTextBox.TextChanged += MapSearchTextBox_TextChanged;
                            
                            if (MapGrid != null)
                            {
                                MapGrid.BeginningEdit += MapGrid_BeginningEdit;
                                MapGrid.SelectionChanged += DataGridUnselectAll;
                            }
                            
                            if (AffixSearchTextBox != null)
                                AffixSearchTextBox.TextChanged += AffixSearchTextBox_TextChanged;
                            
                            if (AffixGrid != null)
                                AffixGrid.SelectionChanged += DataGridUnselectAll;
                            
                            // Setup collection views
                            _mapCollectionView = CollectionViewSource.GetDefaultView(MapSettings.Instance.MapList);
                            _affixCollectionView = CollectionViewSource.GetDefaultView(AffixSettings.Instance.AffixList);
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
            var basePaths = new[]
            {
                AppDomain.CurrentDomain.BaseDirectory,
                Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                Environment.CurrentDirectory
            };
            
            foreach (var basePath in basePaths)
            {
                if (string.IsNullOrEmpty(basePath)) continue;
                
                var path1 = Path.Combine(basePath, "3rdParty", "MapBot", folder, filename);
                if (File.Exists(path1)) return path1;
                
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

        private bool MapFilter(object obj)
        {
            if (MapSearchTextBox == null) return true;
            var text = MapSearchTextBox.Text;
            if (string.IsNullOrEmpty(text)) return true;
            
            var map = (MapData) obj;
            var words = map.Name.Split(' ');
            foreach (var word in words)
            {
                if (word.StartsWith(text, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private bool AffixFilter(object obj)
        {
            if (AffixSearchTextBox == null) return true;
            var text = AffixSearchTextBox.Text;
            if (string.IsNullOrEmpty(text)) return true;
            
            var affix = (AffixData) obj;
            return affix.Name.StartsWith(text, StringComparison.OrdinalIgnoreCase) ||
                   affix.Description.ContainsIgnorecase(text);
        }

        private void MapSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_mapCollectionView == null) return;
            
            if (string.IsNullOrEmpty(MapSearchTextBox?.Text))
            {
                _mapCollectionView.Filter = null;
            }
            else
            {
                _mapCollectionView.Filter = MapFilter;
            }
        }

        private void AffixSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_affixCollectionView == null) return;
            
            if (string.IsNullOrEmpty(AffixSearchTextBox?.Text))
            {
                _affixCollectionView.Filter = null;
            }
            else
            {
                _affixCollectionView.Filter = AffixFilter;
            }
        }

        private async void MapGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            var header = e.Column.Header as string;
            if (header == null || header != "E%")
                return;

            var type = ((MapData) e.Row.DataContext).Type;
            if (type == MapType.Regular || type == MapType.Bossroom)
                return;

            e.Cancel = true;
            await ShowTooltip("Only for Regular and Bossroom maps");
        }

        private async void OnIbCheckBoxClicked(object sender, RoutedEventArgs e)
        {
            var mapData = (MapData) ((CheckBox) sender).DataContext;

            if (mapData.Type == MapType.Bossroom)
                return;

            await ShowTooltip("Only for Bossroom maps");
        }

        private async void OnFtCheckBoxClicked(object sender, RoutedEventArgs e)
        {
            var mapData = (MapData) ((CheckBox) sender).DataContext;
            var type = mapData.Type;

            if (type == MapType.Multilevel || type == MapType.Complex)
                return;

            await ShowTooltip("Only for Multilevel and Complex maps");
        }

        private async Task ShowTooltip(string msg)
        {
            if (_toolTip.IsOpen)
                return;

            _toolTip.Content = msg;
            _toolTip.IsOpen = true;
            await Task.Delay(2000);
            _toolTip.IsOpen = false;
        }

        private void DataGridUnselectAll(object sender, SelectionChangedEventArgs e)
        {
            ((DataGrid) sender).UnselectAll();
        }

        public class EnumToBoolConverter : IValueConverter
        {
            public static readonly EnumToBoolConverter Instance = new EnumToBoolConverter();

            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return value.Equals(parameter);
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return value.Equals(true) ? parameter : Binding.DoNothing;
            }
        }

        public class IntToBoolConverter : IValueConverter
        {
            public static readonly IntToBoolConverter Instance = new IntToBoolConverter();

            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value == null || parameter == null)
                    return false;
                
                int intValue = (int)value;
                int paramValue = int.Parse(parameter.ToString());
                return intValue == paramValue;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value == null || parameter == null)
                    return Binding.DoNothing;
                
                bool isChecked = (bool)value;
                if (isChecked)
                    return int.Parse(parameter.ToString());
                
                return Binding.DoNothing;
            }
        }
    }
}
