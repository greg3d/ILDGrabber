namespace DXTesting
{
    // setting enums
    public enum SaveFormat { txt, csv, bin }
    public enum MeasRate : int { rate250Hz = 250, rate500Hz = 500, rate1000Hz = 1000, rate2000Hz = 2000 }


    class Settings
    {

        private static Settings instance;

        // поля
        private bool _demo;
        private MeasRate _fs;

        public int[] ports = new int[8];

        public MeasRate Fs
        {
            get
            {
                return _fs;
            }
            set
            {
                _fs = value;
            }
        }

        // props
        public bool Demo
        {
            get
            {
                return _demo;
            }
            set
            {
                _demo = value;
            }
        }

        private Settings()
        {
            _demo = false;
            _fs = (MeasRate)Properties.Settings.Default.Rate;

            ports[0] = Properties.Settings.Default.Port1;
            ports[1] = Properties.Settings.Default.Port2;
            ports[2] = Properties.Settings.Default.Port3;
            ports[3] = Properties.Settings.Default.Port4;
            ports[4] = Properties.Settings.Default.Port5;
            ports[5] = Properties.Settings.Default.Port6;
            ports[6] = Properties.Settings.Default.Port7;
            ports[7] = Properties.Settings.Default.Port8;

        }

        public static Settings getInstance()
        {
            if (instance == null)
            {
                instance = new Settings();
            }

            return instance;
        }

    }
}
