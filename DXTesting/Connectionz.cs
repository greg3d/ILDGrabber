using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DXTesting

{

    class ViewData
    {
        public float[] X;
        public float[] Y;

        private float[] temp1;
        private float[] temp2;

        public ViewData()
        {

            X = new float[5000];
            Y = new float[5000];
            temp1 = new float[5000];
            temp2 = new float[5000];
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

    class RealData
    {
        public float[] X;
        public float[] Y;

        public RealData(long n)
        {
            X = new float[n];
            Y = new float[n];
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

    class ConzEventArgs
    {
        public string Message { get; }

        public ConzEventArgs(string mes)
        {
            Message = mes;
        }
    }


    class Connection : IDisposable
    {

        public delegate void StatusHandler(object sender, ConnectionEventArgs e);
        public delegate void RedrawHandler(object sender);

        public event StatusHandler Notify;
        public event RedrawHandler NeedRedraw;

        public TcpClient client;
        public NetworkStream stream;
        private FileStream fs;
        private BinaryWriter FileWriter;
        private BinaryReader fReader;
        //private Ellipse indicator;

        public string filename { get; private set; }

        public int PortNum { get; private set; }

        public bool IsGrabbing { get; private set; } = false;
        public bool IsPostProc { get; private set; } = false;
        public bool IsConnected { get; private set; } = false;
        public bool IsReady { get; private set; } = false;

        public bool GrabTrigger = false;
        public int ConnID { get; private set; }

        private long ticks = 0;
        //private Thread thread;

        private byte[] measrate;
        private byte[] outputrs;
        private byte[] outputnone;
        private byte[] newLine;

        // p
        public ViewData vdata;
        public RealData rdata;
        public float Range { get; private set; } = 50f; // mm
        public string Name { get; private set; }
        public string Serial { get; private set; }

        private bool demoMode = false;

        public Task grabbing;
        public TaskFactory tf;

        public void Dispose()
        {

            fs.Close();
            FileWriter.Close();
            stream.Close();
            client.Close();

            fs?.Dispose();
            FileWriter?.Dispose();
            stream?.Dispose();

        }

        public Connection(int id)
        {
            client = new TcpClient();
            //indicator = indi;

            ConnID = id;

            string command = "MEASRATE 0.25";
            measrate = Encoding.ASCII.GetBytes(command);

            command = "OUTPUT RS422";
            outputrs = Encoding.ASCII.GetBytes(command);

            command = "OUTPUT NONE";
            outputnone = Encoding.ASCII.GetBytes(command);

            newLine = Encoding.ASCII.GetBytes(Environment.NewLine);


        }


        public void Connect(int port)
        {

            Settings sets = Settings.getInstance();
            demoMode = sets.Demo;

            PortNum = port;

            var sdir = Settings.getInstance().SaveDir + "\\";

            if (!Directory.Exists(sdir))
                Directory.CreateDirectory(sdir);

            filename = sdir + "flow" + PortNum + ".dat";

            if (demoMode)
            {
                Thread.Sleep(50);
                IsConnected = true;
                IsReady = true;
                Notify?.Invoke(this, new ConnectionEventArgs("PrepareSuccess", ConnID));

            }
            else
            {
                int localMode = 0;
                while (localMode < 10)
                {

                    switch (localMode)
                    {
                        case 0:
                            try
                            {
                                Task connTa = client.ConnectAsync("192.168.0.252", port);
                                bool result = connTa.Wait(500);

                                if (result)
                                {

                                    IsConnected = true;
                                    localMode = 1;

                                    Notify?.Invoke(this, new ConnectionEventArgs("ConnectionSuccess", ConnID));

                                }
                                else
                                {
                                    Notify?.Invoke(this, new ConnectionEventArgs("ConnectionError", ConnID));

                                    localMode = 10;
                                    IsConnected = false;
                                }
                            }

                            catch (SocketException e)
                            {
                                Notify?.Invoke(this, new ConnectionEventArgs("ConnectionError", ConnID));
                                MessageBox.Show(e.Message);
                                localMode = 10;
                            }
                            catch (Exception e)
                            {
                                Notify?.Invoke(this, new ConnectionEventArgs("ConnectionError", ConnID));
                                MessageBox.Show(e.Message);
                                localMode = 10;
                            }

                            break;

                        case 1: // пытаемся чекнуть лазер 

                            stream = client.GetStream();

                            // отправка сообщения\\\
                            string command = "GETINFO";
                            byte[] getinfo = Encoding.ASCII.GetBytes(command);

                            stream.Write(getinfo, 0, getinfo.Length);
                            stream.Write(newLine, 0, newLine.Length);

                            byte[] data = new byte[128];

                            int size = 0;

                            string infoOut = "";

                            Thread.Sleep(100);

                            while (stream.DataAvailable) // пока данные есть в потоке
                            {
                                size = stream.Read(data, 0, data.Length);
                                infoOut = infoOut + Encoding.ASCII.GetString(data, 0, size);
                                Thread.Sleep(100);
                            }

                            if (infoOut.Length < 1)
                            {
                                stream.Close();
                                client.Close();

                                Notify?.Invoke(this, new ConnectionEventArgs("GetInfoError", ConnID));
                                localMode = 10;
                            }
                            else
                            {
                                stream.Flush();

                                var sArr = infoOut.Split("\n".ToCharArray());
                                /*
                                 MessageBox.Show(sArr[1]); // name
                                 MessageBox.Show(sArr[2]); // serial
                                 MessageBox.Show(sArr[3]);
                                 MessageBox.Show(sArr[4]);
                                 MessageBox.Show(sArr[5]);
                                 MessageBox.Show(sArr[6]); // measure range
                                 */

                                Notify?.Invoke(this, new ConnectionEventArgs("GetInfoSuccess", ConnID));
                                localMode = 2;

                            }


                            break;

                        case 2:

                            stream.Write(measrate, 0, measrate.Length);
                            stream.Write(newLine, 0, newLine.Length);
                            data = new byte[16];
                            size = 0;

                            Thread.Sleep(500);

                            while (stream.DataAvailable) // пока данные есть в потоке
                            {
                                size = stream.Read(data, 0, data.Length);
                                Thread.Sleep(100);
                            }

                            if (size == 0)
                            {
                                localMode = 10;
                                IsConnected = false;
                                IsReady = false;

                                Notify?.Invoke(this, new ConnectionEventArgs("PrepareError", ConnID));

                                /*
                                indicator.Fill = System.Windows.Media.Brushes.Yellow;
                                indicator.Stroke = System.Windows.Media.Brushes.Red;*/

                            }
                            else
                            {
                                localMode = 10;
                                IsConnected = true;
                                IsReady = true;

                                Notify?.Invoke(this, new ConnectionEventArgs("PrepareSuccess", ConnID));
                                /*
                                indicator.Fill = System.Windows.Media.Brushes.LightGreen;
                                indicator.Stroke = System.Windows.Media.Brushes.Green;*/
                            }

                            break;


                        default:
                            break;
                    }

                    //MessageBox.Show(localMode.ToString());
                } // endwhile
            }

        }


        public void StartGrab()
        {
            IsPostProc = false;
            vdata = new ViewData();

            tf = new TaskFactory(
                TaskCreationOptions.LongRunning,
                TaskContinuationOptions.LongRunning
            );
            ticks = 0;
            GrabTrigger = true;

            grabbing = tf.StartNew(DemoGrabbingTask);
            //Notify?.Invoke(this, new ConnectionEventArgs("StartGrabSuccess", ConnID));

        }


        public void StopGrab()
        {
            GrabTrigger = false;
            IsGrabbing = false;

            //Thread.Sleep(100);
            //thread.Abort();

            if (demoMode)
            {

            }
            else
            {
                stream.Write(outputnone, 0, outputnone.Length);
                stream.Write(newLine, 0, newLine.Length);
                stream.Close();
                client.Close();
            }

            FileWriter.Close();
            fs.Close();


            fs.Dispose();
            FileWriter.Dispose();
            Notify?.Invoke(this, new ConnectionEventArgs("GrabbedSuccess", ConnID));
            NeedRedraw?.Invoke(this);


        }

        public void PrepareForView()
        {

            IsGrabbing = false;

            fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            fReader = new BinaryReader(fs);

            long n = fs.Length / 8;

            rdata = new RealData(n);

            var i = 0;

            while (fs.Position != fs.Length)
            {
                //var jjj = fReader.ReadSingle();
                rdata.X[i] = fReader.ReadSingle();
                rdata.Y[i] = fReader.ReadSingle();

                i++;

            }

            fReader.Close();
            fs.Close();

            //Thread.Sleep(50);

            IsPostProc = true;
            NeedRedraw?.Invoke(this);

        }

        private void DemoGrabbingTask()
        {
            if (IsReady && IsConnected)
            {
                fs = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
                FileWriter = new BinaryWriter(fs);

                ticks = 0;

                Thread.Sleep(50);

                do
                {

                    int realSize = 48;
                    DateTime currentDate = DateTime.Now;
                    float curTick = currentDate.Second * 1000 + currentDate.Millisecond;

                    float[] internalCount = new float[realSize];
                    float[] realValues = new float[realSize];
                    float[] timeValues = new float[realSize];

                    for (int j = 0; j < realValues.Length; j++)
                    {

                        ticks++;

                        float val = (float)Math.Sin(2f * 3.14f * ((float)ticks / 1024f)) * 25 + PortNum - 4000;

                        float tt = ticks / 250f;
                        internalCount[j] = tt;

                        realValues[j] = val;
                        timeValues[j] = curTick;

                        var package = new byte[4 * 3];

                        Buffer.BlockCopy(new float[] { curTick, tt, val }, 0, package, 0, 12);
                        FileWriter.Write(package);

                    }

                    Task.Factory.StartNew(() =>
                    {
                        vdata.Push(internalCount, realValues, realSize);
                        NeedRedraw?.Invoke(this);
                    });

                    //pushing.


                    Thread.Sleep(100);

                    IsGrabbing = true;

                }
                while (GrabTrigger); // пока данные есть в потоке и не отменена операция

            }
        }

        private void GrabbingTask()
        {
            if (client.Connected && IsReady)
            {

                fs = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
                FileWriter = new BinaryWriter(fs);

                byte[] data = new byte[96];

                int size = 0;

                stream.Write(outputrs, 0, outputrs.Length);
                stream.Write(newLine, 0, newLine.Length);

                stream.Read(data, 0, 4);

                do
                {
                    size = stream.Read(data, 0, 96);

                    int realSize = size / 3;

                    float[] timeValues = new float[realSize];
                    float[] realValues = new float[realSize];
                    float[] errors = new float[realSize];

                    for (int j = 0; j < realValues.Length; j++)
                    {

                        ticks++;

                        byte low = data[j * 3];
                        byte mid = data[j * 3 + 1];
                        byte high = data[j * 3 + 2];

                        float val;
                        float err;

                        val = (float)(low & 0b0011_1111) + (float)((mid & 0b0011_1111) << 6) + (float)((high & 0b0000_1111) << 12);
                        val = 0.01f * ((102f / 65520f) * val - 1f) * 50f;

                        err = high & 0b0111_0000;

                        if (err > 0)
                        {
                            val = float.NaN;
                        }

                        float tt = (float)ticks / 250f;
                        timeValues[j] = tt;

                        realValues[j] = val;

                        errors[j] = err;

                        FileWriter.Write(err);
                        FileWriter.Write(tt);
                        FileWriter.Write(val);

                    }

                    vdata.Push(timeValues, realValues, realSize);

                    Thread.Sleep(100);

                    IsGrabbing = true;

                }
                while (stream.DataAvailable && GrabTrigger); // пока данные есть в потоке

            }
        }

        public void SaveAsCSV()
        {

            var sdir = Settings.getInstance().SaveDir + "\\";

            var fname = sdir + "flow" + PortNum + ".csv";


            using (var csv = new FileStream(fname, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
            using (var writer = new StreamWriter(csv, Encoding.UTF8))
            {
                var header = "\"Иксы\";\"Игреки\"\r\n";
                writer.Write(header);

                for (int i = 0; i < rdata.X.Length; i++)
                {
                    var line = string.Format("\"{0}\";\"{1}\"\r\n", rdata.X[i], rdata.Y[i]);
                    writer.Write(line);
                }

                //writer.Close();
                //csv.Close();
            }
        }

        public void SaveAsTXT()
        {

            var sdir = Settings.getInstance().SaveDir + "\\";

            var fname = sdir + "flow" + PortNum + ".txt";

            using (var csv = new FileStream(fname, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
            using (var writer = new StreamWriter(csv, Encoding.UTF8))
            {
                var header = "\"Иксы\"\t\"Игреки\"\r\n";
                writer.Write(header);

                for (int i = 0; i < rdata.X.Length; i++)
                {
                    var line = string.Format("\"{0}\"\t\"{1}\"\r\n", rdata.X[i], rdata.Y[i]);
                    writer.Write(line);
                }

                //writer.Close();
                //csv.Close();
            }
        }

    }
    class Connectionz
    {

        public delegate void StatusHandler(object sender, ConzEventArgs e);
        public event StatusHandler SendMessage;

        private int[] ports = new int[8] { 4001, 4002, 4003, 4004, 4005, 4006, 4007, 4008 };
        //private int[] localPorts = new int[8] { 31001, 32002, 33003, 34004, 35005, 36006, 37007, 38008 };
        //Dictionary<int, Ellipse> indicators = new Dictionary<int, Ellipse>(8);

        //Ellipse[] indicators;

        private static Connectionz instance;

        public Connection[] cons { get; private set; }
        public int Count { get; private set; } = 0;
        private Connectionz(int num)
        {
            //indicators.Add(1, MainWindow.Instance.EStatus1);

            /*indicators = new Ellipse[8];

            indicators[0] = MainWindow.Instance.EStatus1;
            indicators[1] = MainWindow.Instance.EStatus2;
            indicators[2] = MainWindow.Instance.EStatus3;
            indicators[3] = MainWindow.Instance.EStatus4;
            indicators[4] = MainWindow.Instance.EStatus5;
            indicators[5] = MainWindow.Instance.EStatus6;
            indicators[6] = MainWindow.Instance.EStatus7;
            indicators[7] = MainWindow.Instance.EStatus8;*/

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
                instance = new Connectionz(num);
            return instance;
        }

        public static Connectionz getInstance()
        {
            if (instance == null)
                instance = new Connectionz(8);
            return instance;
        }

        public void Grab()
        {
            for (int i = 0; i < Count; i++)
            {
                cons[i].StartGrab();
            }
        }

        public void Stop()
        {
            for (int i = 0; i < Count; i++)
            {
                cons[i].StopGrab();
            }
            Task.WaitAll(new Task[] {
                cons[0].grabbing,
                cons[1].grabbing,
                cons[2].grabbing,
                cons[3].grabbing,
                cons[4].grabbing,
                cons[5].grabbing,
                cons[6].grabbing,
                cons[7].grabbing,
            });


            float[] startTimes = new float[8];
            int[] startCounts = new int[8];
            int[] endCounts = new int[8];
            int[] lens = new int[8];

            for (int i = 0; i < Count; i++)
            {
                var fname = cons[i].filename;
                var fs = new FileStream(fname, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                var fReader = new BinaryReader(fs);
                lens[i] = (int)fs.Length / 12;

                startTimes[i] = fReader.ReadSingle();
                var s1 = fReader.ReadSingle();
                var s2 = fReader.ReadSingle();

                fReader.Close();
                fs.Close();

            }

            float min = 0f;

            for (int i = 0; i < Count; i++)
            {
                if (startTimes[i] > min)
                    min = startTimes[i];
            }

            //MessageBox.Show(min.ToString());

            for (int i = 0; i < Count; i++)
            {
                //MessageBox.Show(startTimes[i].ToString());
                startTimes[i] = Math.Abs(startTimes[i] - (float)min);
                startCounts[i] = (int)Math.Floor((double)startTimes[i] / 4d);

                //MessageBox.Show(startCounts[i].ToString());
            }

            min = lens[0] - startTimes[0];

            for (int i = 1; i < Count; i++)
            {
                if (lens[i] - startTimes[i] < min)
                {
                    min = lens[i] - startTimes[i];
                }
            }

            for (int i = 0; i < Count; i++)
            {
                var fname = cons[i].filename;
                var fs = new FileStream(fname, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                var fReader = new BinaryReader(fs);
                var n = (int)fs.Length / 12;

                var someData = new RealData(n);

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

                for (int k = 0; k < min; k++)
                {
                    var package = new byte[4 * 2];

                    Buffer.BlockCopy(new float[] { someData.X[k], someData.Y[k] }, 0, package, 0, 8);
                    bw.Write(package);
                }

                bw.Close();
                fs.Close();


            }


            for (int i = 0; i < Count; i++)
            {
                cons[i].PrepareForView();
            }

        }

        public void ConnectAllTask()
        {

            for (int i = 0; i < Count; i++)
            {
                cons[i].Connect(ports[i]);
            }

            SendMessage?.Invoke(this, new ConzEventArgs("AllConnectedSuccess"));

        }

        public void SaveAll(string format)
        {

            //MessageBox.Show("Сохраняем в формате... " + format);

            switch (format)
            {
                case "csv":

                    for (int i = 0; i < Count; i++)
                    {
                        cons[i].SaveAsCSV();
                    }
                    MessageBox.Show("Сохранено в CSV!");
                    break;

                case "txt":
                    for (int i = 0; i < Count; i++)
                    {
                        cons[i].SaveAsTXT();
                    }
                    MessageBox.Show("Сохранено в TXT!");
                    break;

                default:
                    MessageBox.Show("В демоверсии нельзя сохранять в Binary");
                    break;
            }


        }

        public void ConnectAll()
        {

            Thread conThread = new Thread(new ThreadStart(ConnectAllTask));
            conThread.Name = "Connections thread";
            conThread.IsBackground = true;
            conThread.SetApartmentState(ApartmentState.STA);

            conThread.Start();


        }

    }
}
