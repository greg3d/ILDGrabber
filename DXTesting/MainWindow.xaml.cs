//using DXTesting.ViewModels;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Shapes;

namespace DXTesting
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>

    public enum SaveFormat { txt, csv, bin }

    public class RelayCommand : ICommand
    {

        Action<object> _execteMethod;
        Func<object, bool> _canexecuteMethod;

        public RelayCommand(Action<object> execteMethod, Func<object, bool> canexecuteMethod)
        {
            _execteMethod = execteMethod;
            _canexecuteMethod = canexecuteMethod;
        }

        public bool CanExecute(object parameter)
        {
            if (_canexecuteMethod != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }

        public void Execute(object parameter)
        {
            _execteMethod(parameter);
        }
    }

    public partial class MainWindow : Window
    {
        public class EnumBooleanConverter : IValueConverter
        {
            #region IValueConverter Members
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                string parameterString = parameter as string;
                if (parameterString == null)
                    return DependencyProperty.UnsetValue;

                if (Enum.IsDefined(value.GetType(), value) == false)
                    return DependencyProperty.UnsetValue;

                object parameterValue = Enum.Parse(value.GetType(), parameterString);

                return parameterValue.Equals(value);
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                string parameterString = parameter as string;
                if (parameterString == null)
                    return DependencyProperty.UnsetValue;

                return Enum.Parse(targetType, parameterString);
            }
            #endregion
        }

        private Ellipse[] indicators;

        private Point startPoint = new Point();
        private Point superStartPoint = new Point();
        private Point endPoint = new Point();

        int mouseMode = 0;


        public MainWindow()
        {
            this.Resources["enumToBoolConverter"] = new EnumBooleanConverter();

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

            for (int i = 0; i < cons.cons.Length; i++)
            {
                cons.cons[i].NeedRedraw += DoRedrawHandler;
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

        private void DoRedrawHandler(object sender)
        {
            chartControl1.DoRedraw = true;
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
            checkBoxDemo.IsEnabled = false;
            textBoxSaveDir.IsEnabled = false;

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

                mouseMode = 1;

                chartControl1.AutoYzoom = false;
                startPoint = e.GetPosition(chartControl1);
                chartControl1.CaptureMouse();
                chartControl1.DoRedraw = true;
            }

        }

        private void chartControl1_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (chartControl1.IsMouseCaptured)
            {
                if (mouseMode == 1)
                {
                    endPoint = e.GetPosition(chartControl1);

                    var w = chartControl1.ActualWidth;
                    var k = (chartControl1.xMax - chartControl1.xMin) / w;
                    chartControl1.xMin = (float)(chartControl1.xMin - k * (endPoint.X - startPoint.X));
                    chartControl1.xMax = (float)(chartControl1.xMax - k * (endPoint.X - startPoint.X));
                    chartControl1.DoRedraw = true;

                    startPoint = e.GetPosition(chartControl1);
                }

                if (mouseMode == 2)
                {
                    endPoint = e.GetPosition(chartControl1);


                    foreach (var plot in chartControl1.plotList)
                    {
                        if (superStartPoint.Y < plot.y2 && superStartPoint.Y > plot.y1)
                        {
                            var w = plot.y2- plot.y1;
                            var k = (plot.yMax - plot.yMin) / w;

                            plot.yMin = (float)(plot.yMin + k * (endPoint.Y - startPoint.Y));
                            plot.yMax = (float)(plot.yMax + k * (endPoint.Y - startPoint.Y));


                            chartControl1.DoRedraw = true;
                        }
                    }

                    

                    startPoint = e.GetPosition(chartControl1);
                }


            }
            
            else
            {
                if (chartControl1.IsPostProc)
                {
                    endPoint = e.GetPosition(chartControl1);
                    chartControl1.DrawCursor((int)endPoint.X);
                    chartControl1.DoRedraw = true;
                }

            }
        }

        private void chartControl1_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

            if (chartControl1.IsMouseCaptured)
            {

                mouseMode = 0;
                chartControl1.ReleaseMouseCapture();

            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void chartControl1_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            chartControl1.DrawCursor(-1);
            chartControl1.DoRedraw = true;
        }

        private void buttonYPlus_Click(object sender, RoutedEventArgs e)
        {
            if (chartControl1.IsPostProc)
            {
                chartControl1.ScaleY(1);
                chartControl1.DoRedraw = true;
            }
        }

        private void buttonYMinus_Click(object sender, RoutedEventArgs e)
        {
            if (chartControl1.IsPostProc)
            {

                chartControl1.ScaleY(-1);
                chartControl1.DoRedraw = true;
            }
        }

        private void buttonXPlus_Click(object sender, RoutedEventArgs e)
        {
            if (chartControl1.IsPostProc)
            {

                chartControl1.ScaleX(1);
                chartControl1.DoRedraw = true;
            }
        }

        private void buttonXMinus_Click(object sender, RoutedEventArgs e)
        {
            if (chartControl1.IsPostProc)
            {

                chartControl1.ScaleX(-1);
                chartControl1.DoRedraw = true;
            }
        }

        private void chartControl1_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (chartControl1.IsPostProc)
            {

                mouseMode = 2;

                chartControl1.AutoYzoom = false;
                
                startPoint = e.GetPosition(chartControl1);
                superStartPoint = e.GetPosition(chartControl1);
                chartControl1.CaptureMouse();
            }
        }

        private void chartControl1_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chartControl1.IsMouseCaptured)
            {

                mouseMode = 0;
                chartControl1.ReleaseMouseCapture();

            }
        }
    }

}
