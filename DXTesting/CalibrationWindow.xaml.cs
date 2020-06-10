using System.Windows;

namespace DXTesting
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class CalibrationWindow : Window
    {

        CalibWindowViewModel wm = new CalibWindowViewModel();
        public CalibrationWindow()
        {
            Resources["enumToBoolConverter"] = new EnumBooleanConverter();
            InitializeComponent();
            DataContext = wm;

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Settings settings = Settings.getInstance();
            settings.SaveSettings();

            this.DialogResult = true;
        }
    }
}
