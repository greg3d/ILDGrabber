using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DXTesting

{
    class Connection : BaseViewModel, IDisposable
    {
        //Events and event delegates
        public delegate void StatusHandler(object sender, ConnectionEventArgs e);
        public event StatusHandler Notify;

        // privates
        private string _status;
        private TcpClient client;
        private NetworkStream stream;
        private FileStream fs;
        private BinaryWriter FileWriter;
        private BinaryReader FReader;
        private int PortNum;
        private string ipaddr;
        private Settings settings;

        private long ticks = 0;
        private byte[] newLine = Encoding.ASCII.GetBytes(Environment.NewLine);
        private string console = "";

        private bool GrabTrigger = false;
        private bool demoMode = false;

        private TaskFactory tf;

        public Task grabbing;

        // 
        public int ConnID { get; private set; }
        public string filename { get; private set; }
        public bool IsGrabbing { get; private set; } = false;
        public bool IsPostProc { get; private set; } = false;
        public bool IsConnected { get; private set; } = false;
        public bool IsReady { get; private set; } = false;
        public bool IsVisible { get; set; } = true;

        public RealData vdata { get; private set; }
        public RealData rdata { get; private set; }
        public float Range { get; private set; }
        public double Offset { get; private set; }
        public string Name { get; private set; }
        public string Serial { get; private set; }
        public double Rate { get; private set; }
        public string TextStatus
        {
            get
            {
                return _status;
            }
            set
            {
                _status = value;
                OnPropertyChanged("TextStatus");
            }
        }

        public double CurVal { get; private set; } = 0;

        public void Dispose()
        {

            fs?.Close();
            FileWriter?.Close();
            stream?.Close();
            client?.Close();

            grabbing?.Dispose();
            fs?.Dispose();
            FileWriter?.Dispose();
            stream?.Dispose();
        }

        public Connection(int id)
        {
            ConnID = id;
            TextStatus = "x";
        }

        async void SendCmdAsync(string sss)
        {
            await Task.Run(() => SendCmd(sss));

        }

        void rebootTcpStream()
        {
            stream?.Close();
            client?.Close();
            client = new TcpClient();

            client.Connect(ipaddr, PortNum);
            stream = client.GetStream();

        }

        void SendCmd(string scmd)
        {
            //bool result = false;
            //cmdAck = false;
            var cmd = Encoding.ASCII.GetBytes(scmd);
            stream.Write(cmd, 0, cmd.Length);
            stream.Write(newLine, 0, newLine.Length);

            var awaitAck = true;
            console = "";

            //Thread.Sleep(5);

            byte prevByte = 0;
            int counter = 0;
            do
            {
                if (stream.DataAvailable)
                {

                    var size = 0;

                    if (!GrabTrigger)
                    {
                        byte[] b = new byte[1];
                        size = stream.Read(b, 0, 1);

                        console = console + Encoding.ASCII.GetString(b, 0, 1);

                        if (b[0] == 0x3E && prevByte == 0x2D)
                        {
                            awaitAck = false;
                            //result = true;
                            if (scmd == "OUTPUT RS422")
                            {
                                //
                            }
                        }

                        prevByte = b[0];
                    }
                }
                else
                {
                    Thread.Sleep(10);
                    counter += 10;
                }


            } while (awaitAck && counter < 1000);

            //return result;  && counter < 1000
            //MessageBox.Show(console);
        }

        public int Connect(int port, Settings sets)
        {
            int localMode = 0;

            settings = sets;
            demoMode = settings.Demo;

            PortNum = port;
            ipaddr = settings.IpAddress;

            var sdir = settings.SaveDir + "\\";

            if (!Directory.Exists(sdir))
            {
                Directory.CreateDirectory(sdir);
            }

            filename = sdir + "flow" + PortNum + ".dat";

            if (demoMode)
            {
                Thread.Sleep(50);
                IsConnected = true;
                IsReady = true;
                Range = 50;
                Offset = 0;

                Name = "test laser" + ConnID.ToString();
                Serial = "0000";

                if (IsReady)
                {
                    Notify?.Invoke(this, new ConnectionEventArgs("PrepareSuccess", ConnID));
                } else
                {
                    Notify?.Invoke(this, new ConnectionEventArgs("ConnectionError", ConnID));
                }
                
                localMode = 100;
            }
            else
            {

                while (localMode < 10)
                {
                    switch (localMode)
                    {
                        case 0:
                            try
                            {
                                client = new TcpClient();
                                Task connTa = client.ConnectAsync(ipaddr, PortNum);

                                var res = connTa.Wait(500);

                                if (res)
                                {
                                    IsConnected = true;
                                    localMode = 1;
                                }
                                else
                                {
                                    Notify?.Invoke(this, new ConnectionEventArgs("ConnectionError", ConnID));

                                    localMode = 11;
                                    IsConnected = false;
                                    client?.Close();
                                }

                            }

                            catch (SocketException e)
                            {
                                Notify?.Invoke(this, new ConnectionEventArgs("ConnectionError", ConnID));
                                MessageBox.Show(e.Message);
                                localMode = 12;
                            }

                            catch (Exception e)
                            {
                                Notify?.Invoke(this, new ConnectionEventArgs("ConnectionError", ConnID));
                                MessageBox.Show(e.Message);
                                localMode = 13;
                            }

                            break;

                        case 1: // пытаемся чекнуть лазер 

                            stream = client.GetStream();

                            SendCmd("OUTPUT NONE");

                            rebootTcpStream();

                            SendCmd("GETINFO");

                            string infoOut = console;

                            if (infoOut.Length < 1)
                            {
                                stream.Close();
                                client.Close();

                                Notify?.Invoke(this, new ConnectionEventArgs("GetInfoError", ConnID));
                                localMode = 15;
                            }
                            else
                            {
                                var sArr = infoOut.Split("\n".ToCharArray());
                                /*
                                 MessageBox.Show(sArr[1]); // name
                                 MessageBox.Show(sArr[2]); // serial
                                 MessageBox.Show(sArr[3]);
                                 MessageBox.Show(sArr[4]);
                                 MessageBox.Show(sArr[5]);
                                 MessageBox.Show(sArr[6]); // measure range
                                 */

                                string s = sArr[6];
                                Regex r = new Regex(@"(\d+)");
                                MatchCollection matches = r.Matches(s);

                                Name = sArr[1].Split(":".ToCharArray())[1];
                                Serial = sArr[2].Split(":".ToCharArray())[1];
                                Range = float.Parse(matches[0].Value);

                                Offset = Range / 2;

                                Notify?.Invoke(this, new ConnectionEventArgs("GetInfoSuccess", ConnID));
                                localMode = 2;
                            }

                            break;

                        case 2:

                            SendCmd("OUTADD_RS422 NONE");
                            SendCmd("RESETCNT TIMESTAMP MEASCNT");
                            SendCmd("ECHO OFF");


                            localMode = 10;
                            IsConnected = true;
                            IsReady = true;

                            Notify?.Invoke(this, new ConnectionEventArgs("PrepareSuccess", ConnID));

                            break;


                        default:
                            break;
                    }

                    TextStatus = localMode.ToString();
                } // endwhile
            }


            return localMode;
        }

        public void Disconnect()
        {

            if (IsConnected)
            {
                stream.Close();
                client.Close();
                stream = null;
                client = null;

            }


        }
        public void StartGrab()
        {

            var measrate = (int)settings.Fs;

            Rate = measrate; //*1000
            double drate = measrate / 1000d;

            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = ".";
            //nfi.NumberDecimalDigits = 2;

            var srate = drate.ToString(nfi);

            //Trace.WriteLine(srate);

            IsPostProc = false;
            IsGrabbing = false;

            if (IsReady)
            {

                vdata = new RealData(5000, true);

                tf = new TaskFactory(
                    TaskCreationOptions.LongRunning,
                    TaskContinuationOptions.LongRunning
                );

                ticks = 0;

                if (demoMode)
                {
                    grabbing = tf.StartNew(DemoGrabbingTask);
                }
                else
                {
                    SendCmd("MEASRATE " + srate);
                    grabbing = tf.StartNew(GrabbingTask);
                }

                Notify?.Invoke(this, new ConnectionEventArgs("StartGrabSuccess", ConnID));

            }

        }

        public void StopGrab()
        {
            GrabTrigger = false;

            if (IsGrabbing)
            {
                grabbing.Wait();

                if (demoMode)
                {

                }
                else
                {

                    SendCmd("OUTPUT NONE");
                }

                IsGrabbing = false;

                FileWriter?.Close();
                fs?.Close();

                FileWriter?.Dispose();
                fs?.Dispose();

                //NeedRedraw?.Invoke(this);
                Notify?.Invoke(this, new ConnectionEventArgs("GrabbedSuccess", ConnID));
            }
        }

        public void StartCalibrate()
        {
            TextStatus = "0";
            Rate = 500;
            var srate = 0.5;

            IsPostProc = false;
            IsGrabbing = false;

            if (IsReady)
            {


                tf = new TaskFactory(
                    TaskCreationOptions.LongRunning,
                    TaskContinuationOptions.LongRunning
                );

                if (demoMode)
                {
                    //grabbing = tf.StartNew(DemoGrabbingTask);
                    CurVal = new Random().NextDouble() * Range / 4 + Range / 2;
                }
                else
                {
                    SendCmd("MEASRATE " + srate);
                    grabbing = tf.StartNew(CalibrateTask);
                    grabbing.Wait();
                    SendCmd("OUTPUT NONE");
                }

                IsGrabbing = false;


                settings.setOffset(ConnID + 1, Math.Round(CurVal + ConnID, 3));



                //Notify?.Invoke(this, new ConnectionEventArgs("StartGrabSuccess", ConnID));
            }

        }

        public void PrepareForView()
        {

            IsGrabbing = false;

            fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            FReader = new BinaryReader(fs);

            long n = fs.Length / 8;

            rdata = new RealData(n, false);

            var i = 0;

            while (fs.Position != fs.Length)
            {
                rdata.X[i] = FReader.ReadSingle();
                rdata.Y[i] = FReader.ReadSingle();

                i++;

            }

            FReader.Close();
            fs.Close();

            IsPostProc = true;
        }

        private void DemoGrabbingTask()
        {
            if (IsReady && IsConnected)
            {

                //Rate = 0.5;

                if (settings.OffsetMode == OffsetModeList.Standart)
                {
                    Offset = 0;
                }

                if (settings.OffsetMode == OffsetModeList.Symmetric)
                {
                    Offset = Range / 2;
                }

                if (settings.OffsetMode == OffsetModeList.Assymetric)
                {
                    Offset = settings.getOffset(ConnID + 1);
                }

                fs = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
                FileWriter = new BinaryWriter(fs);

                GrabTrigger = true;

                Thread.Sleep(50);

                do
                {

                    DateTime currentDate = DateTime.Now;
                    float curTick = currentDate.Second * 1000 + currentDate.Millisecond;

                    

                    int realSize = (int)Rate / 10;


                    float[] internalCount = new float[realSize];
                    float[] realValues = new float[realSize];
                    float[] timeValues = new float[realSize];

                    //Trace.WriteLine(Rate.ToString());

                    for (int j = 0; j < realValues.Length; j++)
                    {

                        float tt = ticks / (float)Rate;
                        float val = (float) (Math.Sin(2.0 * 3.14 * tt) * Range/2 + Range/2 - Offset);

                        ticks++;

                        //float val = (float)Rate;

                        internalCount[j] = curTick;
                        realValues[j] = val;
                        timeValues[j] = tt;

                        var package = new byte[4 * 3];

                        Buffer.BlockCopy(new float[] { curTick, tt, val }, 0, package, 0, 12);
                        FileWriter.Write(package);
                    }


                    Task.Factory.StartNew(() =>
                    {
                        vdata.Push(timeValues, realValues, realSize);
                    });

                    IsGrabbing = true;

                    Thread.Sleep(100);

                    //Trace.WriteLine(ticks.ToString());
                    //Trace.WriteLine(GrabTrigger);

                }
                while (GrabTrigger); // пока данные есть в потоке и не отменена операция
            }
        }

        private void GrabbingTask()
        {
            if (IsConnected && IsReady)
            {

                if (settings.OffsetMode == OffsetModeList.Standart)
                {
                    Offset = 0;
                }

                if (settings.OffsetMode == OffsetModeList.Symmetric)
                {
                    Offset = Range / 2;
                }

                if (settings.OffsetMode == OffsetModeList.Assymetric)
                {
                    Offset = settings.getOffset(ConnID + 1);
                }

                fs = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
                FileWriter = new BinaryWriter(fs);

                byte[] data = new byte[4096];

                int size = 0;

                //MessageBox.Show("ok");

                SendCmd("OUTPUT RS422");

                GrabTrigger = true;

                float preval = 0;

                bool startCheck = true;

                do
                {
                    byte[] bbbb = new byte[1];
                    int ff = stream.Read(bbbb, 0, 1);

                    if (ff > 0)
                    {
                        byte H = bbbb[0];
                        var HH = H & 0b1100_0000;


                        //Trace.WriteLine(stream.ToString());

                        //Trace.WriteLine(HH.ToString());

                        if (HH == 192 || HH == 128)
                        {
                            startCheck = false;
                        }

                    }

                }
                while (startCheck && GrabTrigger);

                rebootTcpStream();

                do
                {
                    DateTime currentDate = DateTime.Now;
                    float curTick = currentDate.Second * 1000 + currentDate.Millisecond;

                    size = stream.Read(data, 0, 2048);

                    if (size > 0)
                    {

                        int realSize = size / 3;

                        float[] internalCount = new float[realSize];
                        float[] realValues = new float[realSize];
                        float[] timeValues = new float[realSize];

                        for (int j = 0; j < realValues.Length; j++)
                        {
                            ticks++;

                            byte low = data[j * 3];
                            byte mid = data[j * 3 + 1];
                            byte high = data[j * 3 + 2];

                            float val = (low & 0b0011_1111) + (float)((mid & 0b0011_1111) << 6) + ((high & 0b0000_1111) << 12);
                            val = 0.01f * ((102f / 65520f) * val - 1f) * Range;

                            var err = high & 0b0011_0000;
                            ///err <<= 4;

                            if (err > 0)
                            {

                                if (preval < Range / 2f)
                                {
                                    val = float.NegativeInfinity;
                                }
                                else
                                {
                                    val = float.PositiveInfinity;
                                }

                            }
                            else
                            {
                                preval = val;
                                val = val - (float)Offset;
                            }


                            float tt = (float)ticks / (float)Rate;

                            internalCount[j] = curTick;
                            realValues[j] = val;
                            timeValues[j] = tt;

                            var package = new byte[4 * 3];

                            Buffer.BlockCopy(new float[] { curTick, tt, val }, 0, package, 0, 12);
                            FileWriter.Write(package);
                        }

                        Task.Factory.StartNew(() =>
                        {
                            vdata.Push(timeValues, realValues, realSize);
                        });

                        //}                                            

                    }
                    IsGrabbing = true;
                }
                while (GrabTrigger); // пока данные есть в потоке
            }
        }

        private void CalibrateTask()
        {

            if (IsConnected && IsReady)
            {

                byte[] data = new byte[4096];

                int size = 0;

                SendCmd("OUTPUT RS422");

                GrabTrigger = true;

                float preval = 0;

                bool startCheck = true;

                do
                {
                    byte[] bbbb = new byte[1];
                    int ff = stream.Read(bbbb, 0, 1);

                    if (ff > 0)
                    {
                        byte H = bbbb[0];
                        var HH = H & 0b1100_0000;

                        if (HH == 192 || HH == 128)
                        {
                            startCheck = false;
                        }

                    }

                }
                while (startCheck && GrabTrigger);

                rebootTcpStream();

                do
                {
                    size = stream.Read(data, 0, 2048);

                    if (size > 0)
                    {

                        int realSize = size / 3;

                        for (int j = 0; j < realSize; j++)
                        {
                            ticks++;

                            byte low = data[j * 3];
                            byte mid = data[j * 3 + 1];
                            byte high = data[j * 3 + 2];

                            float val = (low & 0b0011_1111) + (float)((mid & 0b0011_1111) << 6) + ((high & 0b0000_1111) << 12);
                            val = 0.01f * ((102f / 65520f) * val - 1f) * Range;

                            preval = val;

                            if (j == 0)
                            {
                                CurVal = val;
                            }
                            else
                            {
                                CurVal += val;
                            }
                        }

                        CurVal = CurVal / size;

                    }
                    IsGrabbing = false;
                    GrabTrigger = false;
                }
                while (GrabTrigger); // пока данные есть в потоке


            }
        }
    }
}
