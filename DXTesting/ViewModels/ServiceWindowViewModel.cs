using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace DXTesting
{
    class ServiceWindowViewModel : BaseViewModel
    {
        private int _portNum = 0;
        private int _baudrate = 230400;
        private string _outcommand = "";
        private string _rcvd = "";

        public string Rcvd
        {
            get
            {
                return _rcvd;
            }
            set
            {
                _rcvd = value;
                OnPropertyChanged("Rcvd");
            }
        }

        public int PortNum
        {
            get
            {
                return _portNum;
            }
            set
            {
                _portNum = value;
                OnPropertyChanged("PortNum");
            }
        }
        public int BaudRate
        {
            get
            {
                return _baudrate;
            }
            set
            {
                _baudrate = value;
                OnPropertyChanged("BaudRate");
            }
        }

        public string OutCommand
        {
            get
            {
                return _outcommand;
            }
            set
            {
                _outcommand = value;
                OnPropertyChanged("OutCommand");
            }
        }


        // дейтсвия
        public ICommand SendCommand { get; set; }
        public ICommand PrepareBaudCmd { get; set; }

        private bool CanExecuteSend(object parameter)
        {
            if (String.IsNullOrEmpty(OutCommand))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private bool CanExecuteBaud(object parameter)
        {
            return true;
        }

        private void SendMethod(object parameter)
        {
            Rcvd = "Ожидаем ответ...";
            var ocmd = OutCommand + "\r\n";
            byte[] outcmd = Encoding.ASCII.GetBytes(ocmd);


            using (var client = new TcpClient())
            {
                try
                {
                    client.Connect(Properties.Settings.Default.IpAddress, PortNum);
                }
                catch (SocketException e)
                {
                    MessageBox.Show(e.Message);
                    Rcvd = "Ошибка";
                }

                if (client.Connected)
                {
                    var stream = client.GetStream();

                    stream.Write(outcmd, 0, outcmd.Length);

                    byte[] rcvdata = new byte[128];

                    int size = 0;
                    string infoOut = "";

                    Thread.Sleep(20);

                    while (stream.DataAvailable) // пока данные есть в потоке
                    {
                        size = stream.Read(rcvdata, 0, rcvdata.Length);
                        infoOut = infoOut + Encoding.ASCII.GetString(rcvdata, 0, size);
                        Thread.Sleep(20);
                    }

                    Rcvd = infoOut;

                    stream.Close();

                }
                else
                {
                    MessageBox.Show("Not Connected");
                    Rcvd = "Ошибка";
                }

            }



        }

        private void PrepareBaudMethod(object parameter)
        {
            OutCommand = "BAUDRATE " + BaudRate.ToString();
        }


        public ServiceWindowViewModel()
        {

            SendCommand = new RelayCommand(SendMethod, CanExecuteSend);
            PrepareBaudCmd = new RelayCommand(PrepareBaudMethod, CanExecuteBaud);
        }




    }
}
