using System.Windows;

namespace DXTesting
{
    /// <summary>
    /// Логика взаимодействия для SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            Resources["enumToBoolConverter"] = new EnumBooleanConverter();
            InitializeComponent();
            DataContext = new SettingsWindowViewModel();
        }

        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            Properties.Settings.Default.Save();
        }


    }
}
