using System.Windows.Input;

namespace DXTesting
{

    class CalibWindowViewModel : BaseViewModel
    {

        private Settings settings = Settings.getInstance();

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


        public OffsetModeList OffsetMode
        {
            get { return settings.OffsetMode; }
            set
            {
                settings.OffsetMode = value;
                OnPropertyChanged("OffsetMode");
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

        public CalibWindowViewModel()
        {
            CalibrateCommand = new RelayCommand(CalibrateMethod, CanExecuteCalibrate);
        }

        //


    }
}
