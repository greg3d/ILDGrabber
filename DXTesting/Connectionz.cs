using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
//using System.Windows;
using System.Windows.Forms;

namespace DXTesting

{
    class RealData
    {
        public float[] X;
        public float[] Y;

        private float[] temp1;
        private float[] temp2;


        public RealData(long n, bool doTemp)
        {
            X = new float[n];
            Y = new float[n];

            if (doTemp)
            {
                temp1 = new float[n];
                temp2 = new float[n];
            }
        }

        public void Push(float[] xx, float[] yy, int len)
        {

            Array.Copy(X, temp1, X.Length);
            Array.Copy(Y, temp2, Y.Length);


            int size = X.Length - len;

            for (int i = 0; i < size; i++)
            {
                X[i] = temp1[i + len];
                Y[i] = temp2[i + len];
            }

            for (int i = 0; i < xx.Length; i++)
            {
                X[size + i] = xx[i];
                Y[size + i] = yy[i];
            }
        }
    }

    class ConnectionEventArgs
    {
        public string Message { get; }

        public int LaserID { get; }

        public ConnectionEventArgs(string mes, int id)
        {
            Message = mes;
            LaserID = id;
        }
    }

    class ProgressArgs
    {
        public int Status { get; }
        public ProgressArgs(int status)
        {
            Status = status;
        }
    }

    class ConzEventArgs
    {
        public string Message { get; }

        public ConzEventArgs(string mes)
        {
            Message = mes;
        }
    }

    class Connectionz
    {

        public delegate void StatusHandler(object sender, ConzEventArgs e);

        public event StatusHandler SendMessage;

        private static Connectionz instance;

        public Connection[] cons { get; private set; }
        public int Count { get; private set; }

        public int ReadyCount { get; private set; }

        public List<int> ReadyList { get; private set; }

        private Connectionz(int num)
        {
            cons = new Connection[num];


            for (int i = 0; i < num; i++)
            {
                cons[i] = new Connection(i);
            }
            Count = num;
        }

        public static Connectionz getInstance(int num)
        {
            if (instance == null)
            {
                instance = new Connectionz(num);
            }

            return instance;
        }

        public static Connectionz getInstance()
        {
            if (instance == null)
            {
                instance = new Connectionz(8);
            }

            return instance;
        }

        public void PrepareReadyList()
        {
            ReadyCount = 0;
            ReadyList = new List<int>();

            foreach (var con in cons)
            {
                if (con.IsReady)
                {
                    ReadyCount++;
                    ReadyList.Add(con.ConnID);
                }
            }
        }

        public List<int> GetGrabbedList()
        {
            //var cnt = 0;

            var list = new List<int>();

            foreach (var con in cons)
            {
                if (con.IsPostProc)
                {
                    //cnt++;
                    list.Add(con.ConnID);
                }
            }

            return list;
        }

        public void Grab()
        {
            PrepareReadyList();

            for (int i = 0; i < Count; i++)
            {
                //Thread.Sleep(500);
                cons[i].StartGrab();
            }
        }

        public void Calibrate()
        {
            MessageBox.Show("Выставите датчики в нужное положение и нажмите ОК...");
            Settings sets = Settings.getInstance();

            for (int i = 0; i < Count; i++)
            {
                cons[i].StartCalibrate();

            }

        }

        public int Stop(IProgress<int> progress)
        {
            PrepareReadyList();
            progress.Report(0);

            Task[] tasks = new Task[ReadyCount];
            foreach (var ch in ReadyList.Select((x, i) => new { Value = x, Index = i }))
            {
                tasks[ch.Index] = cons[ch.Value].grabbing;
            }


            for (int i = 0; i < Count; i++)
            {
                cons[i].StopGrab();
            }

            Task.WaitAll(tasks);

            progress.Report(5);

            float[] startTimes = new float[ReadyCount];
            int[] startCounts = new int[ReadyCount];
            int[] endCounts = new int[ReadyCount];
            int[] lens = new int[ReadyCount];

            foreach (var ch in ReadyList.Select((x, i) => new { Value = x, Index = i }))
            {
                var fname = cons[ch.Value].filename;
                var fs = new FileStream(fname, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                var fReader = new BinaryReader(fs);
                lens[ch.Index] = (int)fs.Length / 12;

                startTimes[ch.Index] = fReader.ReadSingle();
                var s1 = fReader.ReadSingle();
                var s2 = fReader.ReadSingle();

                fReader.Close();
                fs.Close();
            }

            progress.Report(10);

            float max = startTimes.Max();

            for (int i = 0; i < startTimes.Length; i++)
            {
                startTimes[i] = Math.Abs(max - startTimes[i]);

                int fs = (int)Settings.getInstance().Fs;
                double k = (1000d / fs);
                startCounts[i] = (int)Math.Floor(startTimes[i] / k);
            }

            progress.Report(20);

            var min = lens[0] - startCounts[0];

            for (int i = 1; i < startTimes.Length; i++)
            {
                if (lens[i] - startCounts[i] < min)
                {
                    min = lens[i] - startCounts[i];
                }
            }

            progress.Report(30);

            foreach (var ch in ReadyList.Select((x, i) => new { Value = x, Index = i }))
            {
                var fname = cons[ch.Value].filename;
                var fs = new FileStream(fname, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                var fReader = new BinaryReader(fs);
                var n = (int)fs.Length / 12;

                var someData = new RealData(n, false);

                var j = 0;

                while (fs.Position != fs.Length)
                {
                    var jjj = fReader.ReadSingle();
                    someData.X[j] = fReader.ReadSingle();
                    someData.Y[j] = fReader.ReadSingle();

                    j++;
                }

                fReader.Close();
                fs.Close();

                fs = new FileStream(fname, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                var bw = new BinaryWriter(fs);
                float newEx = 0;

                for (int k = startCounts[ch.Index]; k < startCounts[ch.Index] + min; k++)
                {
                    var package = new byte[4 * 2];
                    newEx++;
                    Buffer.BlockCopy(new float[] { newEx / (float)cons[0].Rate, someData.Y[k] }, 0, package, 0, 8);
                    bw.Write(package);
                }

                bw.Close();
                fs.Close();
            }

            progress.Report(40);

            int curProgress = 40;

            foreach (var i in ReadyList)
            {

                cons[i].PrepareForView();
                curProgress = curProgress + i * (60 / ReadyCount);
                progress.Report(curProgress);
            }

            progress.Report(100);

            return (int)100;

        }

        public void ConnectAllTask()
        {

            Settings sets = Settings.getInstance();

            for (int i = 0; i < Count; i++)
            {

                cons[i].Connect(sets.getPort(i+1), sets);

            }

            PrepareReadyList();
            if (this.ReadyCount > 0)
            {
                SendMessage?.Invoke(this, new ConzEventArgs("AllConnectedSuccess"));
            }
            else
            {
                SendMessage?.Invoke(this, new ConzEventArgs("AllConnectedError"));
            }

        }

        public void SaveAll(string format)
        {

            Settings sets = Settings.getInstance();
            SaveFileDialog saveDialog = new SaveFileDialog();

            var sdir = Properties.Settings.Default.SaveDir + "\\";

            saveDialog.AddExtension = true;
            saveDialog.Filter = "Файл в формате *." + format + "|" + "*." + format;
            saveDialog.InitialDirectory = sdir;


            var dlm = ";";
            var newline = "\r\n";
            var bracket = "\"";

            switch (format)
            {
                case "txt":
                    dlm = "\t";
                    newline = "\r\n";
                    bracket = "";
                    break;
                default:
                    break;
            }

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {

                using (var csv = new FileStream(saveDialog.FileName, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
                using (var writer = new StreamWriter(csv, Encoding.UTF8))
                {
                    var header = bracket + "x сек" + bracket + dlm;

                    var l = GetGrabbedList();

                    int n = 0;

                    foreach (var item in l)
                    {
                        header = header + bracket + "port_" + sets.getPort(item+1) + bracket + dlm;
                        n = cons[item].rdata.X.Length;
                    }

                    var exes = cons[l[0]].rdata.X;

                    header = header + newline;

                    writer.Write(header);


                    for (int i = 0; i < n; i++)
                    {
                        var line = bracket + exes[i].ToString("F3") + bracket + dlm;

                        foreach (var item in l)
                        {
                            line = line + bracket + cons[item].rdata.Y[i].ToString("F2") + bracket + dlm;
                        }

                        line = line + newline;

                        writer.Write(line);
                    }

                    //writer.Close();
                    //csv.Close();
                }
                MessageBox.Show("Сохранено в " + saveDialog.FileName);
            }



        }

        public void ConnectAll()
        {

            /*Task.Factory.StartNew(() =>
            {
                ConnectAllTask();
            });*/


            Thread conThread = new Thread(new ThreadStart(ConnectAllTask));
            conThread.Name = "Connections thread";
            conThread.IsBackground = true;
            conThread.SetApartmentState(ApartmentState.STA);

            conThread.Start();


        }

    }
}
