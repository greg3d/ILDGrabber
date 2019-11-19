using DXTesting.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Shapes;

namespace DXTesting
{




    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {


        //public static MainWindow Instance { get; private set; } // тут будет форма


        /*

        private void Window_Closing(object sender, ExitEventArgs e)
        {
            Connectionz cons = Connectionz.getInstance();
            cons.Close();

        }*/

        private Ellipse[] indicators;

        public MainWindow()
        {

            InitializeComponent();
            this.DataContext = new MainWindowViewModel();

            //Connectionz cons = Connectionz.getInstance(1);

            //Instance = this;

            GrabButton.IsEnabled = false;
            StopButton.IsEnabled = false;
            ConnectButton.IsEnabled = true;

            Connectionz cons = Connectionz.getInstance();

            for (int i = 0; i < cons.cons.Length; i++)
            {
                cons.cons[i].Notify += SetConnIndicator;
            }

            indicators = new Ellipse[8];

            indicators[0] = EStatus1;
            indicators[1] = EStatus2;
            indicators[2] = EStatus3;
            indicators[3] = EStatus4;
            indicators[4] = EStatus5;
            indicators[5] = EStatus6;
            indicators[6] = EStatus7;
            indicators[7] = EStatus8;

            //cons.Notify=
        }


        private void SetConnIndicator(object sender, ConnectionEventArgs e)
        {
            
            this.Dispatcher.Invoke(() =>
            {
                ConnectButton.IsEnabled = false;

                switch (e.Message)
                {
                    case "ConnectionSuccess":
                        indicators[e.LaserID].Fill = System.Windows.Media.Brushes.LightGreen;

                        GrabButton.IsEnabled = true;
                        StopButton.IsEnabled = false;
                        ConnectButton.IsEnabled = false;
                        break;

                    case "ConnectionError":
                        indicators[e.LaserID].Fill = System.Windows.Media.Brushes.Red;
                        indicators[e.LaserID].Stroke = System.Windows.Media.Brushes.Yellow;
                        break;

                    default:
                        break;
                }
            });

            
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            Connectionz cons = Connectionz.getInstance();
            cons.ConnectAll();
        }

        private void GrabButton_Click(object sender, RoutedEventArgs e)
        {
            Connectionz cons = Connectionz.getInstance();
            cons.Grab();
            GrabButton.IsEnabled = false;
            StopButton.IsEnabled = true;
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            Connectionz cons = Connectionz.getInstance();
            cons.Stop();
            GrabButton.IsEnabled = true;
        }

        private void PropertyChanged(object sender, ProgressChangedEventArgs e)
        {
            MessageBox.Show(e.ToString());
        }

    }

}
