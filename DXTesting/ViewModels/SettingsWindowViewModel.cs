namespace DXTesting
{

    class SettingsWindowViewModel : BaseViewModel
    {

        private Settings settings = Settings.getInstance();

        public int Port1
        {
            get { return Properties.Settings.Default.Port1; }
            set
            {
                Properties.Settings.Default.Port1 = value;
                settings.ports[0] = value;
            }
        }

        public int Port2
        {
            get { return Properties.Settings.Default.Port2; }
            set
            {
                Properties.Settings.Default.Port2 = value;
                settings.ports[1] = value;
            }
        }

        public int Port3
        {
            get { return Properties.Settings.Default.Port3; }
            set
            {
                Properties.Settings.Default.Port3 = value;
                settings.ports[2] = value;
            }
        }

        public int Port4
        {
            get { return Properties.Settings.Default.Port4; }
            set
            {
                Properties.Settings.Default.Port4 = value;
                settings.ports[3] = value;
            }
        }

        public int Port5
        {
            get { return Properties.Settings.Default.Port5; }
            set
            {
                Properties.Settings.Default.Port5 = value;
                settings.ports[4] = value;
            }
        }

        public int Port6
        {
            get { return Properties.Settings.Default.Port6; }
            set
            {
                Properties.Settings.Default.Port6 = value;
                settings.ports[5] = value;
            }
        }

        public int Port7
        {
            get { return Properties.Settings.Default.Port7; }
            set
            {
                Properties.Settings.Default.Port7 = value;
                settings.ports[6] = value;
            }
        }

        public int Port8
        {
            get { return Properties.Settings.Default.Port8; }
            set
            {
                Properties.Settings.Default.Port8 = value;
                settings.ports[7] = value;
            }
        }

        public string IpAddress
        {
            get { return Properties.Settings.Default.IpAddress; }
            set
            {
                Properties.Settings.Default.IpAddress = value;
                OnPropertyChanged("IpAddress");
            }
        }

        public MeasRate CurrentRate
        {
            get { return settings.Fs; }
            set
            {
                settings.Fs = value;
                Properties.Settings.Default.Rate = (int)value;
                OnPropertyChanged("CurrentRate");
            }
        }

        public string DefaultFolder
        {
            get { return Properties.Settings.Default.SaveDir; }
            set
            {
                Properties.Settings.Default.SaveDir = value;
                OnPropertyChanged("DefaultFolder");
            }
        }

        public SettingsWindowViewModel()
        {
            CurrentRate = settings.Fs;
        }


    }
}
