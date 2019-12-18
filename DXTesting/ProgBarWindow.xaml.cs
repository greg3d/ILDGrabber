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
            });

            if (stat == 100)
            {
                this.Close();
            }
        }
    }
}
