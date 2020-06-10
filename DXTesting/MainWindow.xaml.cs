//using DXTesting.ViewModels;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Shapes;

namespace DXTesting
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    #region RelayCommand Defenition

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
    #endregion


    public class EnumBooleanConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string parameterString = parameter as string;
            if (parameterString == null)
            {
                return DependencyProperty.UnsetValue;
            }

            if (Enum.IsDefined(value.GetType(), value) == false)
            {
                return DependencyProperty.UnsetValue;
            }

            object parameterValue = Enum.Parse(value.GetType(), parameterString);

            return parameterValue.Equals(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string parameterString = parameter as string;
            if (parameterString == null)
            {
                return DependencyProperty.UnsetValue;
            }

            return Enum.Parse(targetType, parameterString);
        }
        #endregion
    }


    public partial class MainWindow : Window
    {
        Canvas2DD gdi;
        private System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();

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


            Loaded += MainWindow_Loaded;
            timer.Tick += DoRedrawHandler;
            timer.Interval = 30;

            GrabButton.IsEnabled = false;
            StopButton.IsEnabled = false;
            ConnectButton.IsEnabled = true;

            Connectionz cons = Connectionz.getInstance();
            cons.SendMessage += ButtonsDisabling;
            //cons.ProgressChanged += ProgressChangedHandler;

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

            Binding bind = new Binding();
            bind.Source = cons.cons[0];
            bind.Path = new PropertyPath("IsVisible");
            bind.Mode = BindingMode.TwoWay;
            checkBox1.SetBinding(CheckBox.IsCheckedProperty, bind);

            bind = new Binding();
            bind.Source = cons.cons[1];
            bind.Path = new PropertyPath("IsVisible");
            bind.Mode = BindingMode.TwoWay;
            checkBox2.SetBinding(CheckBox.IsCheckedProperty, bind);

            bind = new Binding();
            bind.Source = cons.cons[2];
            bind.Path = new PropertyPath("IsVisible");
            bind.Mode = BindingMode.TwoWay;
            checkBox3.SetBinding(CheckBox.IsCheckedProperty, bind);

            bind = new Binding();
            bind.Source = cons.cons[3];
            bind.Path = new PropertyPath("IsVisible");
            bind.Mode = BindingMode.TwoWay;
            checkBox4.SetBinding(CheckBox.IsCheckedProperty, bind);

            bind = new Binding();
            bind.Source = cons.cons[4];
            bind.Path = new PropertyPath("IsVisible");
            bind.Mode = BindingMode.TwoWay;
            checkBox5.SetBinding(CheckBox.IsCheckedProperty, bind);

            bind = new Binding();
            bind.Source = cons.cons[5];
            bind.Path = new PropertyPath("IsVisible");
            bind.Mode = BindingMode.TwoWay;
            checkBox6.SetBinding(CheckBox.IsCheckedProperty, bind);

            bind = new Binding();
            bind.Source = cons.cons[6];
            bind.Path = new PropertyPath("IsVisible");
            bind.Mode = BindingMode.TwoWay;
            checkBox7.SetBinding(CheckBox.IsCheckedProperty, bind);

            bind = new Binding();
            bind.Source = cons.cons[7];
            bind.Path = new PropertyPath("IsVisible");
            bind.Mode = BindingMode.TwoWay;
            checkBox8.SetBinding(CheckBox.IsCheckedProperty, bind);
        }

        private void EnableScaleButtons(bool status)
        {
            buttonAutoScale.IsEnabled = status;
            buttonXMinus.IsEnabled = status;
            buttonYMinus.IsEnabled = status;
            buttonXPlus.IsEnabled = status;
            buttonYPlus.IsEnabled = status;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //gField.SizeChanged += GField_SizeChanged;
            //WindowState = WindowState.Maximized;
            gdi = new Canvas2DD(gContainer, gField);
            progressBar2.Visibility = Visibility.Hidden;
            EnableScaleButtons(false);
        }

        private void GField_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //gdi.createBitmaps();
        }

        private void DoRedrawHandler(object sender, EventArgs e)
        {
            gdi.Redraw();
        }

        private void ButtonsDisabling(object sender, ConzEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                ConnectButton.IsEnabled = true;

                switch (e.Message)
                {
                    case "AllConnectedSuccess":
                        ConnectButton.IsEnabled = false;
                        GrabButton.IsEnabled = true;
                        StopButton.IsEnabled = false;
                        buttonOpenCalibration.IsEnabled = true;
                        break;
                    case "AllConnectedError":
                        ConnectButton.IsEnabled = true;
                        GrabButton.IsEnabled = false;
                        StopButton.IsEnabled = false;
                        buttonOpenCalibration.IsEnabled = false;
                        break;
                    case "Disconnected":
                        ConnectButton.IsEnabled = true;
                        GrabButton.IsEnabled = false;
                        StopButton.IsEnabled = false;
                        buttonOpenCalibration.IsEnabled = false;
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
                    case "Disconnected":
                        ConnectButton.IsEnabled = true;
                        GrabButton.IsEnabled = false;
                        StopButton.IsEnabled = false;
                        buttonOpenCalibration.IsEnabled = false;
                        break;


                    default:
                        break;
                }
            });


        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            ConnectButton.IsEnabled = false;

            Connectionz cons = Connectionz.getInstance();
            cons.ConnectAll();

            checkBoxDemo.IsEnabled = false;
        }

        private void GrabButton_Click(object sender, RoutedEventArgs e)
        {
            EnableScaleButtons(false);
            GrabButton.IsEnabled = false;
            StopButton.IsEnabled = true;

            timer.Start();
            Connectionz cons = Connectionz.getInstance();
            cons.Grab();
        }

        private async void StopButton_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();

            Connectionz cons = Connectionz.getInstance();

            GrabButton.IsEnabled = false;
            StopButton.IsEnabled = false;

            ((MainWindowViewModel)DataContext).ProgBarVisibility = Visibility.Visible;

            var progress = new Progress<int>(s => progressBar2.Value = s);

            int result = await Task.Factory.StartNew<int>(() => cons.Stop(progress), TaskCreationOptions.LongRunning);

            GrabButton.IsEnabled = true;
            StopButton.IsEnabled = false;

            EnableScaleButtons(true);

            ((MainWindowViewModel)DataContext).ProgBarVisibility = Visibility.Hidden;

            gdi.Redraw();

        }

        private void chartControl1_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (gdi.IsPostProc)
            {
                mouseMode = 1;
                gdi.AutoYzoom = false;
                startPoint = e.GetPosition(gField);
                gField.CaptureMouse();
                gdi.Redraw();
            }
        }

        private void chartControl1_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {

            if (gField.IsMouseCaptured)
            {
                if (mouseMode == 1)
                {
                    endPoint = e.GetPosition(gField);

                    var w = gField.ActualWidth;
                    var k = (gdi.xMax - gdi.xMin) / w;
                    gdi.xMin = (float)(gdi.xMin - k * (endPoint.X - startPoint.X));
                    gdi.xMax = (float)(gdi.xMax - k * (endPoint.X - startPoint.X));
                    gdi.Redraw();

                    startPoint = e.GetPosition(gField);
                }

                if (mouseMode == 2)
                {
                    endPoint = e.GetPosition(gField);


                    foreach (var plot in gdi.plotList)
                    {
                        if (superStartPoint.Y < plot.y2 && superStartPoint.Y > plot.y1)
                        {
                            var w = plot.y2 - plot.y1;
                            var k = (plot.yMax - plot.yMin) / w;

                            plot.yMin = (float)(plot.yMin + k * (endPoint.Y - startPoint.Y));
                            plot.yMax = (float)(plot.yMax + k * (endPoint.Y - startPoint.Y));

                            gdi.Redraw();
                        }
                    }
                    startPoint = e.GetPosition(gField);
                }
            }
            else
            {
                if (gdi.IsPostProc)
                {
                    endPoint = e.GetPosition(gField);
                    gdi.DrawCursor((int)endPoint.X);
                    gdi.Redraw();
                }
            }
        }

        private void chartControl1_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (gField.IsMouseCaptured)
            {
                mouseMode = 0;
                gField.ReleaseMouseCapture();
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void chartControl1_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            gdi.DrawCursor(-1);
            gdi.Redraw();
        }

        private void buttonYPlus_Click(object sender, RoutedEventArgs e)
        {
            if (gdi.IsPostProc)
            {
                gdi.ScaleY(1);
                gdi.Redraw();
            }
        }

        private void buttonYMinus_Click(object sender, RoutedEventArgs e)
        {
            if (gdi.IsPostProc)
            {
                gdi.ScaleY(-1);
                gdi.Redraw();
            }
        }

        private void buttonXPlus_Click(object sender, RoutedEventArgs e)
        {
            if (gdi.IsPostProc)
            {
                gdi.ScaleX(1);
                gdi.Redraw();
            }
        }

        private void buttonXMinus_Click(object sender, RoutedEventArgs e)
        {
            if (gdi.IsPostProc)
            {
                gdi.ScaleX(-1);
                gdi.Redraw();
            }
        }

        private void chartControl1_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (gdi.IsPostProc)
            {
                mouseMode = 2;
                gdi.AutoYzoom = false;

                startPoint = e.GetPosition(gField);
                superStartPoint = e.GetPosition(gField);
                gField.CaptureMouse();
            }
        }

        private void chartControl1_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (gField.IsMouseCaptured)
            {
                mouseMode = 0;
                gField.ReleaseMouseCapture();
            }
        }

        private void buttonAutoScale_Click(object sender, RoutedEventArgs e)
        {
            gdi.AutoYzoom = true;
            gdi.Redraw();
        }

        private void buttonOpenSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow sWindow = new SettingsWindow();
            sWindow.ShowDialog();
            sWindow.Activate();
        }

        private void buttonOpenService_Click(object sender, RoutedEventArgs e)
        {
            ServiceWindow sWindow = new ServiceWindow();
            sWindow.ShowDialog();
            sWindow.Activate();
        }

        private void buttonOpenCalibration_Click(object sender, RoutedEventArgs e)
        {
            CalibrationWindow sWindow = new CalibrationWindow();
            sWindow.ShowDialog();
            sWindow.Activate();
        }
    }

}
