using DXTesting.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Shapes;

namespace DXTesting
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private Ellipse[] indicators;
        private Point startPoint = new Point();
        private Point endPoint = new Point();

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
            cons.SendMessage += ButtonsDisabling;

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

        }

        private void ButtonsDisabling(object sender, ConzEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                ConnectButton.IsEnabled = false;

                switch (e.Message)
                {
                    case "AllConnectedSuccess":
                        ConnectButton.IsEnabled = false;
                        GrabButton.IsEnabled = true;
                        StopButton.IsEnabled = false;
                        break;
                }

            });
        }

        private void SetConnIndicator(object sender, ConnectionEventArgs e)
        {

            this.Dispatcher.Invoke(() =>
            {
                switch (e.Message)
                {
                    case "ConnectionSuccess":
                        indicators[e.LaserID].Fill = System.Windows.Media.Brushes.LightGreen;

                        break;

                    case "ConnectionError":
                        indicators[e.LaserID].Fill = System.Windows.Media.Brushes.Red;
                        indicators[e.LaserID].Stroke = System.Windows.Media.Brushes.Yellow;
                        break;

                    case "PrepareSuccess":
                        indicators[e.LaserID].Fill = System.Windows.Media.Brushes.LightGreen;
                        indicators[e.LaserID].Stroke = System.Windows.Media.Brushes.Green;
                        ConnectButton.IsEnabled = false;
                        break;
                    case "StartGrabSuccess":
                        indicators[e.LaserID].Fill = System.Windows.Media.Brushes.LightGreen;
                        indicators[e.LaserID].Stroke = System.Windows.Media.Brushes.White;
                        break;

                    case "PrepareError":
                        indicators[e.LaserID].Fill = System.Windows.Media.Brushes.Red;
                        indicators[e.LaserID].Stroke = System.Windows.Media.Brushes.White;
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
            StopButton.IsEnabled = false;
        }

        private void PropertyChanged(object sender, ProgressChangedEventArgs e)
        {
            MessageBox.Show(e.ToString());
        }

        private void chartControl1_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (chartControl1.IsPostProc)
            {
                chartControl1.AutoYzoom = false;
                startPoint = e.GetPosition(chartControl1);
                chartControl1.CaptureMouse();
            }

            


            //chartControl1.
        }

        private void chartControl1_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (chartControl1.IsMouseCaptured)
            {
                endPoint = e.GetPosition(chartControl1);
                   
            }
        }

        private void chartControl1_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

            if (chartControl1.IsMouseCaptured)
            {
                endPoint = e.GetPosition(chartControl1);

                var w = chartControl1.ActualWidth;
                var k = (chartControl1.xMax - chartControl1.xMin) / w;
                chartControl1.xMin = (float)(chartControl1.xMin - k * (endPoint.X - startPoint.X));
                chartControl1.xMax = (float)(chartControl1.xMax - k * (endPoint.X - startPoint.X));


                chartControl1.ReleaseMouseCapture();
            }
        }
                
    }

}
