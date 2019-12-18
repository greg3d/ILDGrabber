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

            Connectionz cons = Connectionz.getInstance();
            cons.ProgressChanged += ProgressChangedHandler;
        }

        private void ProgressChangedHandler(ProgressArgs e)
        {
            int stat = e.Status;
            this.Dispatcher.Invoke(() =>
            {
                progbar1.Value = stat;

                if (stat == 100)
                {
                    Close();
                }
            });

            
        }
    }
}
