namespace DXTesting
{
    class Settings
    {

        private static Settings instance;

        // поля
        private bool _demo = false;

        // props
        public bool Demo
        {
            get
            {
                return _demo;
            }
        }

        private Settings()
        {

            _demo = true;

        }

        public static Settings getInstance()
        {
            if (instance == null)
                instance = new Settings();
            return instance;
        }

    }
}
