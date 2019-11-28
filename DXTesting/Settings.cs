namespace DXTesting
{
    class Settings
    {

        private static Settings instance;

        // поля
        private bool _demo;
        private string _saveDir;

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

        public string SaveDir
        {
            get
            {
                return _saveDir;
            }
            set
            {
                _saveDir = value;
            }
        }

        private Settings()
        {
            _demo = true;
            _saveDir =  @"C:\Results";
        }

        public static Settings getInstance()
        {
            if (instance == null)
                instance = new Settings();
            return instance;
        }

    }
}
