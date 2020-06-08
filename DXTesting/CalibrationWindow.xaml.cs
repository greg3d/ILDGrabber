using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
