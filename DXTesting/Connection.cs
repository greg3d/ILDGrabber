﻿using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DXTesting

{
    class Connection : IDisposable
    {
        //Events and event delegates
        public delegate void StatusHandler(object sender, ConnectionEventArgs e);
        public event StatusHandler Notify;

        // privates
        private TcpClient client;
        private NetworkStream stream;
        private MemoryStream bstream;
        private FileStream fs;
        private BinaryWriter FileWriter;
        private BinaryReader FReader;
        private int PortNum;
        private string ipaddr;

        private long ticks = 0;
        private byte[] newLine = Encoding.ASCII.GetBytes(Environment.NewLine);
        private string console = "";

        private bool GrabTrigger = false;
        private bool cmdAck = false;
        private bool demoMode = false;

        public Task grabbing;
        private TaskFactory tf;

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
        public string Name { get; private set; }
        public string Serial { get; private set; }
        public double Rate { get; private set; }

        public void Dispose()
        {
            fs?.Close();
            FileWriter?.Close();
            stream?.Close();
            bstream?.Close();
            client?.Close();

            fs?.Dispose();
            FileWriter?.Dispose();
            stream?.Dispose();
            bstream?.Dispose();
        }

        public Connection(int id)
        {
            ConnID = id;
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
            //cmdAck = false;
            var cmd = Encoding.ASCII.GetBytes(scmd);
            stream.Write(cmd, 0, cmd.Length);
            stream.Write(newLine, 0, newLine.Length);

            var awaitAck = true;
            console = "";

            //Thread.Sleep(5);

            byte prevByte = 0;

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
                            if (scmd == "OUTPUT RS422")
                            {
                               //
                            }
                        }

                        prevByte = b[0];

                    }
                }

            } while (awaitAck);

            //MessageBox.Show(console);
        }

        public void Connect(int port, string srate, double mrate)
        {

            Rate = mrate * 1000;

            Settings sets = Settings.getInstance();
            demoMode = sets.Demo;

            PortNum = port;
            ipaddr = Properties.Settings.Default.IpAddress;

            var sdir = Properties.Settings.Default.SaveDir + "\\";

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
                Name = "test laser" + ConnID.ToString();
                Serial = "0000";
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

                                    localMode = 10;
                                    IsConnected = false;
                                    client?.Close();
                                }

                            }

                            catch (SocketException e)
                            {
                                Notify?.Invoke(this, new ConnectionEventArgs("ConnectionError", ConnID));
                                MessageBox.Show(e.Message);
                                localMode = 10;
                            }
                            
                           /* catch (Exception e)
                            {
                                Notify?.Invoke(this, new ConnectionEventArgs("ConnectionError", ConnID));
                                MessageBox.Show(e.Message);
                                localMode = 10;
                            }*/

                            break;

                        case 1: // пытаемся чекнуть лазер 
                           
                            stream = client.GetStream();

                            SendCmd("OUTPUT NONE");
                            SendCmd("GETINFO");

                            string infoOut = console;

                            if (infoOut.Length < 1)
                            {
                                stream.Close();
                                client.Close();

                                Notify?.Invoke(this, new ConnectionEventArgs("GetInfoError", ConnID));
                                localMode = 10;
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

                                Notify?.Invoke(this, new ConnectionEventArgs("GetInfoSuccess", ConnID));
                                localMode = 2;
                            }

                            break;

                        case 2:

                            SendCmd("OUTADD_RS422 NONE");
                            SendCmd("RESETCNT TIMESTAMP MEASCNT");
                            SendCmd("MEASRATE " + srate);
                            SendCmd("ECHO OFF");


                            localMode = 10;
                            IsConnected = true;
                            IsReady = true;

                            Notify?.Invoke(this, new ConnectionEventArgs("PrepareSuccess", ConnID));
                            
                            break;


                        default:
                            break;
                    }


                } // endwhile
            }

        }


        public void StartGrab()
        {
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

                fs?.Dispose();
                FileWriter?.Dispose();

                //NeedRedraw?.Invoke(this);
                Notify?.Invoke(this, new ConnectionEventArgs("GrabbedSuccess", ConnID));
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

                        float tt = (float)ticks / (float)Rate;
                        float val = (float)Math.Sin(2f * 3.14f * tt) * Range + PortNum - 4000;

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

                    Thread.Sleep(100);

                    IsGrabbing = true;

                }
                while (GrabTrigger); // пока данные есть в потоке и не отменена операция
            }
        }

        private void GrabbingTask()
        {

            if (IsConnected && IsReady)
            {

                fs = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
                FileWriter = new BinaryWriter(fs);

                byte[] data = new byte[4096];

                int size = 0;

                //MessageBox.Show("ok");

                SendCmd("OUTPUT RS422");

                GrabTrigger = true;

                float preval = -1;

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
                                byte  mid = data[j * 3 + 1];
                                byte  high = data[j * 3 + 2];

                                float val = (low & 0b0011_1111) + (float)((mid & 0b0011_1111) << 6) + ((high & 0b0000_1111) << 12);
                                val = 0.01f * ((102f / 65520f) * val - 1f) * Range;

                                var err = high & 0b0111_0000;

                                if (err > 0)
                                {
                                    if (preval < Range / 2f)
                                    {
                                        val = -1;
                                    }
                                    else
                                    {
                                        val = Range + 1;
                                    }

                                }

                                preval = val;

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
    }
}
