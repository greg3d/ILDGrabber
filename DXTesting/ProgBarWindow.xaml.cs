using System.Windows;

namespace DXTesting
{
    /// <summary>
    /// Логика взаимодействия для ProgBarWindow.xaml
    /// </summary>
    public partial class ProgBarWindow : Window
    {
        public ProgBarWindow()
        {
            InitializeComponent();
            progbar1.ValueChanged += Progbar1_ValueChanged;
        }

        private void Progbar1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.NewValue == 100)
            {
                Close();
            }
        }
    }
}
