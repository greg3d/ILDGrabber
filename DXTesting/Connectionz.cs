using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DXTesting

{

    class ViewData
    {
        public float[] X = new float[5000];
        public float[] Y = new float[5000];

        private float[] temp1 = new float[5000];
        private float[] temp2 = new float[5000];
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


    class Connection : IDisposable
    {

        public delegate void StatusHandler(object sender, ConnectionEventArgs e);
        public event StatusHandler Notify;

        public TcpClient client;
        public NetworkStream stream;
        private FileStream fs;
        private BinaryWriter FileWriter;
        //private Ellipse indicator;

        public string filename { get; private set; }

        public int PortNum { get; private set; }

        public bool IsGrabbing { get; private set; } = false;
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
        public ViewData vdata = new ViewData();
        public float Range { get; private set; } = 50f; // mm
        public string Name { get; private set; }
        public string Serial { get; private set; }

        private bool demoMode = false;

        public void Dispose()
        {

            fs.Close();
            FileWriter.Close();
            stream.Close();
            client.Close();

            fs?.Dispose();
            FileWriter?.Dispose();
            stream?.Dispose();
            client?.Dispose();

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

            Settings sets = Settings.getInstance();
            demoMode = sets.Demo;

        }



        public void Connect(int port)
        {

            PortNum = port;

            if (demoMode)
            {
                Thread.Sleep(100);
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

                                    /*
                                    indicator.Fill = System.Windows.Media.Brushes.LightGreen;
                                    MainWindow.Instance.GrabButton.IsEnabled = true;
                                    MainWindow.Instance.StopButton.IsEnabled = false;
                                    MainWindow.Instance.ConnectButton.IsEnabled = false;
                                    */

                                    Notify?.Invoke(this, new ConnectionEventArgs("ConnectionSuccess", ConnID));

                                    IsConnected = true;
                                    localMode = 1;
                                }
                                else
                                {

                                    /*
                                    indicator.Fill = System.Windows.Media.Brushes.Red;
                                    indicator.Stroke = System.Windows.Media.Brushes.Yellow;
                                     */

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

                            // MessageBox.Show(infoOut.Length.ToString());

                            if (infoOut.Length < 1)
                            {
                                stream.Close();
                                client.Close();

                                /*
                                indicator.Fill = System.Windows.Media.Brushes.Yellow;
                                indicator.Stroke = System.Windows.Media.Brushes.Red;
                                */
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

                                /*
                                   indicator.Fill = System.Windows.Media.Brushes.LightGreen;
                                   indicator.Stroke = System.Windows.Media.Brushes.Yellow;*/

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
            TaskFactory tf = new TaskFactory(TaskCreationOptions.LongRunning, TaskContinuationOptions.LongRunning);
            ticks = 0;
            GrabTrigger = true;

            Task grabbing = tf.StartNew(DemoGrabbingTask);
                        
            //Notify?.Invoke(this, new ConnectionEventArgs("StartGrabSuccess", ConnID));
            //grabbing.Start();
            
        }
        /*
        public void StartGrab()
        {
            ticks = 0;

            Notify?.Invoke(this, new ConnectionEventArgs("StartGrabSuccess", ConnID));

            //indicator.Stroke = System.Windows.Media.Brushes.White;

            thread = new Thread(new ThreadStart(GrabbingTask));
            thread.Start();
            GrabTrigger = true;
        }
        */
        public void StopGrab()
        {
            GrabTrigger = false;
            IsGrabbing = false;

            Thread.Sleep(100);
            //thread.Abort();

            if (demoMode)
            {

            } else
            {
                stream.Write(outputnone, 0, outputnone.Length);
                stream.Write(newLine, 0, newLine.Length);
                stream.Close();
                client.Close();
            }

            FileWriter.Close();
            fs.Close();
        }
    
        private void DemoGrabbingTask()
        {
            if (IsReady && IsConnected)
            {
                filename = "D:\\flow_" + PortNum + ".dat";
                fs = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
                FileWriter = new BinaryWriter(fs);

                ticks = 0;

                Thread.Sleep(200);

                do
                {

                    int realSize = 48;

                    Single[] timeValues = new Single[realSize];
                    Single[] realValues = new Single[realSize];
                    Single[] errors = new Single[realSize];

                    for (int j = 0; j < realValues.Length; j++)
                    {

                        ticks++;

                        float val;
                        float err;

                        val = (float)Math.Sin(2f * 3.14f * ((float)ticks / 4096f)) * 25;

                        err = 0b0111_0000;

                        float tt = ticks / 250f;
                        timeValues[j] = tt;

                        realValues[j] = val;

                        errors[j] = err;

                        FileWriter.Write(err);
                        FileWriter.Write(tt);
                        FileWriter.Write(val);

                    }

                    Task pushing = Task.Factory.StartNew(() =>
                    {
                        vdata.Push(timeValues, realValues, realSize);
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

                filename = "D:\\flow_" + PortNum + ".dat";

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

                    Single[] timeValues = new Single[realSize];
                    Single[] realValues = new Single[realSize];
                    Single[] errors = new Single[realSize];

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



    }
    class Connectionz
    {

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
        }

        public void ConnectAllTask()
        {

            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                for (int i = 0; i < Count; i++)
                {
                    cons[i].Connect(ports[i]);
                }
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }

        }

        public void ConnectAll()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {

                Thread conThread = new Thread(new ThreadStart(ConnectAllTask));
                conThread.Name = "Connections thread";
                conThread.IsBackground = true;
                conThread.SetApartmentState(ApartmentState.STA);

                conThread.Start();
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }



        }

    }
}
