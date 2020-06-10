using System;

namespace DXTesting
{
    // setting enums
    public enum SaveFormat { txt, csv, bin }
    public enum MeasRate : int { rate250Hz = 250, rate500Hz = 500, rate1000Hz = 1000, rate2000Hz = 2000 }
    public enum OffsetModeList : int { Standart, Symmetric, Assymetric }

    class Settings
    {

        private static Settings instance;

        // поля
        private bool _demo;


        private int[] ports = new int[8];
        private double[] offsets = new double[8];

        public int getPort(int num)
        {
            return ports[num - 1];
        }

        public void setPort(int num, int val)
        {
            ports[num - 1] = val;
        }

        public double getOffset(int num)
        {
            return offsets[num - 1];
        }

        public void setOffset(int num, double val)
        {
            offsets[num - 1] = val;
        }

        public OffsetModeList OffsetMode
        {
            get
            {
                return (OffsetModeList)Properties.Settings.Default.OffsetMode;
            }
            set
            {
                Properties.Settings.Default.OffsetMode = (int)value;
            }
        }
        public MeasRate Fs
        {
            get
            {
                return (MeasRate)Properties.Settings.Default.Rate;
            }
            set
            {
                Properties.Settings.Default.Rate = (int)value;
            }
        }

        public string IpAddress
        {
            get
            {
                return Properties.Settings.Default.IpAddress;
            }
            set
            {
                if (value != Properties.Settings.Default.IpAddress)
                {
                    Properties.Settings.Default.IpAddress = value;
                }
            }
        }

        public string SaveDir
        {
            get
            {
                return Properties.Settings.Default.SaveDir;
            }
            set
            {
                if (value != Properties.Settings.Default.SaveDir)
                {
                    Properties.Settings.Default.SaveDir = value;
                }
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

            ports[0] = Properties.Settings.Default.Port1;
            ports[1] = Properties.Settings.Default.Port2;
            ports[2] = Properties.Settings.Default.Port3;
            ports[3] = Properties.Settings.Default.Port4;
            ports[4] = Properties.Settings.Default.Port5;
            ports[5] = Properties.Settings.Default.Port6;
            ports[6] = Properties.Settings.Default.Port7;
            ports[7] = Properties.Settings.Default.Port8;

            double[] arr = new double[8];
            var arrs = Properties.Settings.Default.Offsets.Split(';');
            for (int i = 0; i < arr.Length; i++)
            {
                offsets[i] = Double.Parse(arrs[i]);
            }

        }

        public void SaveSettings()
        {

            Properties.Settings.Default.Port1 = ports[0];
            Properties.Settings.Default.Port2 = ports[1];
            Properties.Settings.Default.Port3 = ports[2];
            Properties.Settings.Default.Port4 = ports[3];
            Properties.Settings.Default.Port5 = ports[4];
            Properties.Settings.Default.Port6 = ports[5];
            Properties.Settings.Default.Port7 = ports[6];
            Properties.Settings.Default.Port8 = ports[7];

            string[] arr = new string[8];

            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = offsets[i].ToString();
            }

            var str = String.Join(";", arr);

            Properties.Settings.Default.Offsets = str;

            //сейвим
            Properties.Settings.Default.Save();
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
