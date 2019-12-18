using System.Windows;

namespace DXTesting
{
    /// <summary>
    /// Логика взаимодействия для ServiceWindow.xaml
    /// </summary>
    public partial class ServiceWindow : Window
    {

        ServiceWindowViewModel wm = new ServiceWindowViewModel();

        public ServiceWindow()
        {
            InitializeComponent();
            DataContext = wm;

        }

        private void pickPort1_Click(object sender, RoutedEventArgs e)
        {
            wm.PortNum = Properties.Settings.Default.Port1;
        }

        private void pickPort2_Click(object sender, RoutedEventArgs e)
        {
            wm.PortNum = Properties.Settings.Default.Port2;
        }

        private void pickPort3_Click(object sender, RoutedEventArgs e)
        {
            wm.PortNum = Properties.Settings.Default.Port3;
        }

        private void pickPort4_Click(object sender, RoutedEventArgs e)
        {
            wm.PortNum = Properties.Settings.Default.Port4;
        }

        private void pickPort5_Click(object sender, RoutedEventArgs e)
        {
            wm.PortNum = Properties.Settings.Default.Port5;
        }

        private void pickPort6_Click(object sender, RoutedEventArgs e)
        {
            wm.PortNum = Properties.Settings.Default.Port6;
        }

        private void pickPort7_Click(object sender, RoutedEventArgs e)
        {
            wm.PortNum = Properties.Settings.Default.Port7;
        }

        private void pickPort8_Click(object sender, RoutedEventArgs e)
        {
            wm.PortNum = Properties.Settings.Default.Port8;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            wm.OutCommand = "GETINFO";
        }

        private void StoreCmdButton_Click(object sender, RoutedEventArgs e)
        {
            wm.OutCommand = "MEASSETTINGS STORE Name1";
        }

        private void StoreCmdButton_Copy_Click(object sender, RoutedEventArgs e)
        {
            wm.OutCommand = "BASICSETTINGS STORE";
        }

        private void StoreCmdButton_Copy1_Click(object sender, RoutedEventArgs e)
        {
            wm.OutCommand = "MEASSETTINGS CURRENT";
        }

        private void StoreCmdButton_Copy2_Click(object sender, RoutedEventArgs e)
        {
            wm.OutCommand = "BASICSETTINGS READ";
        }

        private void button_Copy_Click(object sender, RoutedEventArgs e)
        {
            wm.OutCommand = "EXPORT ALL";
        }
    }
}
