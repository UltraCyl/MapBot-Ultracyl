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
        private bool _scarabControlsInitialized = false;
        
        internal TextBox MapSearchTextBox;
        internal DataGrid MapGrid;
        internal TextBox AffixSearchTextBox;
        internal DataGrid AffixGrid;
        
        // Scarab controls
        internal CheckBox UseScarabsCheckBox;
        internal RadioButton Slots4Radio;
        internal RadioButton Slots5Radio;
        internal RadioButton Slots6Radio;
        internal ComboBox ScarabSlot1ComboBox;
        internal ComboBox ScarabSlot2ComboBox;
        internal ComboBox ScarabSlot3ComboBox;
        internal ComboBox ScarabSlot4ComboBox;
        internal ComboBox ScarabSlot5ComboBox;

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
                            
                            // Setup Scarab controls - use Loaded event to ensure visual tree is ready
                            this.Loaded += (s, e) => SetupScarabControls();
                            
                            // Also try to setup immediately in case already loaded
                            SetupScarabControls();
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
        
        private void SetupScarabControls()
        {
            // Prevent double initialization
            if (_scarabControlsInitialized) return;
            
            var settings = ScarabSettings.Instance;
            
            // Find scarab controls - try multiple methods
            UseScarabsCheckBox = FindDescendant<CheckBox>(this, "UseScarabsCheckBox");
            Slots4Radio = FindDescendant<RadioButton>(this, "Slots4Radio");
            Slots5Radio = FindDescendant<RadioButton>(this, "Slots5Radio");
            Slots6Radio = FindDescendant<RadioButton>(this, "Slots6Radio");
            ScarabSlot1ComboBox = FindDescendant<ComboBox>(this, "ScarabSlot1ComboBox");
            ScarabSlot2ComboBox = FindDescendant<ComboBox>(this, "ScarabSlot2ComboBox");
            ScarabSlot3ComboBox = FindDescendant<ComboBox>(this, "ScarabSlot3ComboBox");
            ScarabSlot4ComboBox = FindDescendant<ComboBox>(this, "ScarabSlot4ComboBox");
            ScarabSlot5ComboBox = FindDescendant<ComboBox>(this, "ScarabSlot5ComboBox");
            
            // If we couldn't find the comboboxes, try again later (visual tree might not be ready)
            if (ScarabSlot1ComboBox == null)
            {
                return;
            }
            
            _scarabControlsInitialized = true;
            
            // Also find the panel that contains the scarab options
            var mapDeviceSlotsPanel = FindDescendant<StackPanel>(this, "MapDeviceSlotsPanel");
            
            // Get the scarab list
            var scarabList = ScarabSettings.AvailableScarabs;
            
            // Setup scarab combo boxes FIRST (before checkbox events)
            SetupScarabComboBox(ScarabSlot1ComboBox, scarabList, settings.ScarabSlot1, v => settings.ScarabSlot1 = v);
            SetupScarabComboBox(ScarabSlot2ComboBox, scarabList, settings.ScarabSlot2, v => settings.ScarabSlot2 = v);
            SetupScarabComboBox(ScarabSlot3ComboBox, scarabList, settings.ScarabSlot3, v => settings.ScarabSlot3 = v);
            SetupScarabComboBox(ScarabSlot4ComboBox, scarabList, settings.ScarabSlot4, v => settings.ScarabSlot4 = v);
            SetupScarabComboBox(ScarabSlot5ComboBox, scarabList, settings.ScarabSlot5, v => settings.ScarabSlot5 = v);
            
            // Setup Map Device Slots radio buttons
            if (Slots4Radio != null)
            {
                Slots4Radio.IsChecked = settings.MapDeviceSlots == 4;
                Slots4Radio.Checked += (s, e) => { settings.MapDeviceSlots = 4; };
            }
            if (Slots5Radio != null)
            {
                Slots5Radio.IsChecked = settings.MapDeviceSlots == 5;
                Slots5Radio.Checked += (s, e) => { settings.MapDeviceSlots = 5; };
            }
            if (Slots6Radio != null)
            {
                Slots6Radio.IsChecked = settings.MapDeviceSlots == 6;
                Slots6Radio.Checked += (s, e) => { settings.MapDeviceSlots = 6; };
            }
            
            // Helper method to update enabled state of all scarab controls
            Action<bool> updateControlsEnabled = (enabled) =>
            {
                if (Slots4Radio != null) Slots4Radio.IsEnabled = enabled;
                if (Slots5Radio != null) Slots5Radio.IsEnabled = enabled;
                if (Slots6Radio != null) Slots6Radio.IsEnabled = enabled;
                if (ScarabSlot1ComboBox != null) ScarabSlot1ComboBox.IsEnabled = enabled;
                if (ScarabSlot2ComboBox != null) ScarabSlot2ComboBox.IsEnabled = enabled;
                if (ScarabSlot3ComboBox != null) ScarabSlot3ComboBox.IsEnabled = enabled;
                if (ScarabSlot4ComboBox != null) ScarabSlot4ComboBox.IsEnabled = enabled;
                if (ScarabSlot5ComboBox != null) ScarabSlot5ComboBox.IsEnabled = enabled;
                if (mapDeviceSlotsPanel != null) mapDeviceSlotsPanel.IsEnabled = enabled;
            };
            
            // Setup UseScarabs checkbox
            if (UseScarabsCheckBox != null)
            {
                UseScarabsCheckBox.IsChecked = settings.UseScarabs;
                
                // Set initial enabled state
                updateControlsEnabled(settings.UseScarabs);
                
                UseScarabsCheckBox.Checked += (s, e) => 
                { 
                    settings.UseScarabs = true;
                    updateControlsEnabled(true);
                };
                UseScarabsCheckBox.Unchecked += (s, e) => 
                { 
                    settings.UseScarabs = false;
                    updateControlsEnabled(false);
                };
            }
        }
        
        private void SetupScarabComboBox(ComboBox comboBox, System.Collections.Generic.List<string> items, string currentValue, Action<string> setter)
        {
            if (comboBox == null) 
            {
                return;
            }
            
            if (items == null || items.Count == 0)
            {
                return;
            }
            
            // Clear and add items manually
            comboBox.Items.Clear();
            foreach (var item in items)
            {
                comboBox.Items.Add(item);
            }
            
            // Set selected value
            if (!string.IsNullOrEmpty(currentValue) && comboBox.Items.Contains(currentValue))
            {
                comboBox.SelectedItem = currentValue;
            }
            else if (comboBox.Items.Count > 0)
            {
                comboBox.SelectedIndex = 0; // Select "None"
            }
            
            // Handle selection change
            comboBox.SelectionChanged += (s, e) =>
            {
                if (comboBox.SelectedItem != null)
                    setter(comboBox.SelectedItem.ToString());
            };
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
            
            try
            {
                // Try Visual Tree
                int childCount = System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);
                for (int i = 0; i < childCount; i++)
                {
                    var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                    
                    if (child is T element && element.Name == name)
                        return element;
                    
                    var result = FindDescendant<T>(child, name);
                    if (result != null) return result;
                }
                
                // Also try Logical Tree for elements not in visual tree yet (with safety checks)
                foreach (var child in LogicalTreeHelper.GetChildren(parent))
                {
                    // Skip non-visual elements like ColumnDefinition, RowDefinition, etc.
                    if (!(child is System.Windows.Media.Visual)) continue;
                    
                    if (child is T element && element.Name == name)
                        return element;
                    
                    if (child is DependencyObject depChild)
                    {
                        var result = FindDescendant<T>(depChild, name);
                        if (result != null) return result;
                    }
                }
            }
            catch
            {
                // Ignore errors from iterating non-visual elements
            }
            
            return null;
        }
        
        // Helper to find element by type when name doesn't work - Visual Tree only
        private static System.Collections.Generic.List<T> FindAllDescendants<T>(DependencyObject parent) where T : FrameworkElement
        {
            var results = new System.Collections.Generic.List<T>();
            if (parent == null) return results;
            
            try
            {
                int childCount = System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);
                for (int i = 0; i < childCount; i++)
                {
                    var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                    
                    if (child is T element)
                        results.Add(element);
                    
                    results.AddRange(FindAllDescendants<T>(child));
                }
            }
            catch
            {
                // Ignore errors
            }
            
            return results;
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
