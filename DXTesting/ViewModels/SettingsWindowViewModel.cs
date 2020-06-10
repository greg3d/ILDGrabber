using System.Windows.Input;

namespace DXTesting
{

    class SettingsWindowViewModel : BaseViewModel
    {

        private Settings settings = Settings.getInstance();

        public int Port1
        {
            get { return settings.getPort(1); }
            set
            {
                settings.setPort(1, value);
                OnPropertyChanged("Port1");
            }
        }
        public int Port2
        {
            get { return settings.getPort(2); }
            set
            {
                settings.setPort(2, value);
                OnPropertyChanged("Port2");
            }
        }
        public int Port3
        {
            get { return settings.getPort(3); }
            set
            {
                settings.setPort(3, value);
                OnPropertyChanged("Port3");
            }
        }
        public int Port4
        {
            get { return settings.getPort(4); }
            set
            {
                settings.setPort(4, value);
                OnPropertyChanged("Port4");
            }
        }
        public int Port5
        {
            get { return settings.getPort(5); }
            set
            {
                settings.setPort(5, value);
                OnPropertyChanged("Port5");
            }
        }
        public int Port6
        {
            get { return settings.getPort(6); }
            set
            {
                settings.setPort(6, value);
                OnPropertyChanged("Port6");
            }
        }
        public int Port7
        {
            get { return settings.getPort(7); }
            set
            {
                settings.setPort(7, value);
                OnPropertyChanged("Port7");
            }
        }
        public int Port8
        {
            get { return settings.getPort(8); }
            set
            {
                settings.setPort(8, value);
                OnPropertyChanged("Port8");
            }
        }


        public double Offset1
        {
            get
            {
                return settings.getOffset(1);
            }
            set
            {
                if (value != settings.getOffset(1))
                {
                    settings.setOffset(1, value);
                    OnPropertyChanged("Offset1");
                }
            }
        }
        public double Offset2
        {
            get
            {
                return settings.getOffset(2);
            }
            set
            {
                if (value != settings.getOffset(2))
                {
                    settings.setOffset(2, value);
                    OnPropertyChanged("Offset2");
                }
            }
        }
        public double Offset3
        {
            get
            {
                return settings.getOffset(3);
            }
            set
            {
                if (value != settings.getOffset(3))
                {
                    settings.setOffset(3, value);
                    OnPropertyChanged("Offset3");
                }
            }
        }
        public double Offset4
        {
            get
            {
                return settings.getOffset(4);
            }
            set
            {
                if (value != settings.getOffset(4))
                {
                    settings.setOffset(4, value);
                    OnPropertyChanged("Offset4");
                }
            }
        }
        public double Offset5
        {
            get
            {
                return settings.getOffset(5);
            }
            set
            {
                if (value != settings.getOffset(5))
                {
                    settings.setOffset(5, value);
                    OnPropertyChanged("Offset5");
                }
            }
        }
        public double Offset6
        {
            get
            {
                return settings.getOffset(6);
            }
            set
            {
                if (value != settings.getOffset(6))
                {
                    settings.setOffset(6, value);
                    OnPropertyChanged("Offset6");
                }
            }
        }
        public double Offset7
        {
            get
            {
                return settings.getOffset(7);
            }
            set
            {
                if (value != settings.getOffset(7))
                {
                    settings.setOffset(7, value);
                    OnPropertyChanged("Offset7");
                }
            }
        }
        public double Offset8
        {
            get
            {
                return settings.getOffset(8);
            }
            set
            {
                if (value != settings.getOffset(8))
                {
                    settings.setOffset(8, value);
                    OnPropertyChanged("Offset8");
                }
            }
        }

        public string IpAddress
        {
            get { return settings.IpAddress; }
            set
            {
                settings.IpAddress = value;
                OnPropertyChanged("IpAddress");
            }
        }

        public MeasRate CurrentRate
        {
            get { return settings.Fs; }
            set
            {
                settings.Fs = value;
                OnPropertyChanged("CurrentRate");
            }
        }

        public OffsetModeList OffsetMode
        {
            get { return settings.OffsetMode; }
            set
            {
                settings.OffsetMode = value;
                OnPropertyChanged("OffsetMode");
            }
        }

        public string DefaultFolder
        {
            get { return settings.SaveDir; }
            set
            {
                settings.SaveDir = value;
                OnPropertyChanged("DefaultFolder");
            }
        }

        public ICommand CalibrateCommand { get; set; }
        private bool CanExecuteCalibrate(object parameter)
        {
            return true;
        }
        private void CalibrateMethod(object parameter)
        {
            var cons = Connectionz.getInstance();
            cons.Calibrate();
        }

        public SettingsWindowViewModel()
        {
            CalibrateCommand = new RelayCommand(CalibrateMethod, CanExecuteCalibrate);
        }

        //


    }
}
