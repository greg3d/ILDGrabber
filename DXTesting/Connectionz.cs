using System;
using System.Text;
using System.Net.Sockets;
using System.Windows.Shapes;
using System.Windows;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

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

    class Connection
    {
        public TcpClient client;
        public NetworkStream stream;
        private FileStream fs;
        private BinaryWriter FileWriter;
        private Ellipse indicator;

        public string filename { get; private set; }

        public int PortNum { get; private set; }

        public bool IsGrabbing { get; private set; } = false;
        public bool IsConnected { get; private set; } = false;
        public bool IsReady { get; private set; } = false;

        public bool GrabTrigger = false;

        private long ticks = 0;
        private Thread thread;

        private byte[] measrate;
        private byte[] outputrs;
        private byte[] outputnone;
        private byte[] newLine;

        // p
        public ViewData vdata = new ViewData();
        public float Range { get; private set; } = 50f; // mm
        public string Name { get; private set; }
        public string Serial { get; private set; }


  
        public Connection(Ellipse indi)
        {
            client = new TcpClient();
            indicator = indi;

            string command = "MEASRATE 0.25";
            measrate = Encoding.ASCII.GetBytes(command);

            command = "OUTPUT RS422";
            outputrs = Encoding.ASCII.GetBytes(command);
            
            command = "OUTPUT NONE";
            outputnone = Encoding.ASCII.GetBytes(command);

            newLine = Encoding.ASCII.GetBytes(Environment.NewLine);

            
            
        }

        private void connectTask(object _port)
        {

            int port = (int)_port;

        }

        public void Connect(int port)
        {
            PortNum = port;

            int localMode = 0;

            while (localMode < 10)
            {

                switch (localMode)
                {
                    case 0:
                        try
                        {
                            Task connTa = client.ConnectAsync("192.168.0.252", port);
                            bool result = connTa.Wait(1000);

                            if (result)
                            {

                                indicator.Fill = System.Windows.Media.Brushes.LightGreen;
                                MainWindow.Instance.GrabButton.IsEnabled = true;
                                MainWindow.Instance.StopButton.IsEnabled = false;
                                MainWindow.Instance.ConnectButton.IsEnabled = false;

                                IsConnected = true;
                                localMode = 1;
                            }
                            else
                            {
                                indicator.Fill = System.Windows.Media.Brushes.Red;
                                indicator.Stroke = System.Windows.Media.Brushes.Yellow;

                                localMode = 10;
                                IsConnected = false;
                            }
                        }

                        catch (SocketException e)
                        {
                            MessageBox.Show(e.Message);
                            localMode = 10;
                        }
                        catch (Exception e)
                        {
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
                            indicator.Fill = System.Windows.Media.Brushes.Yellow;
                            indicator.Stroke = System.Windows.Media.Brushes.Red;
                            localMode = 10;
                        }
                        else
                        {
                            stream.Flush();

                            var sArr = infoOut.Split("\n".ToCharArray());

                            

                            MessageBox.Show(sArr[1]); // name
                            MessageBox.Show(sArr[2]); // serial
                            MessageBox.Show(sArr[3]);
                            MessageBox.Show(sArr[4]);
                            MessageBox.Show(sArr[5]);
                            MessageBox.Show(sArr[6]); // measure range
                            //IsReady = true;
                            localMode = 2;
                            //localMode = 10;
                            indicator.Fill = System.Windows.Media.Brushes.LightGreen;
                            indicator.Stroke = System.Windows.Media.Brushes.Yellow;
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
                            indicator.Fill = System.Windows.Media.Brushes.Yellow;
                            indicator.Stroke = System.Windows.Media.Brushes.Red;
                        } else
                        {
                            localMode = 10;
                            IsConnected = true;
                            IsReady = true;
                            indicator.Fill = System.Windows.Media.Brushes.LightGreen;
                            indicator.Stroke = System.Windows.Media.Brushes.Green;
                        }

                        break;


                    default:
                        break;
                }

                //MessageBox.Show(localMode.ToString());
            } // endwhile



            //Thread conThread = new Thread(new ParameterizedThreadStart(connectTask));
            //conThread.Start(port);           
        }

        public FileStream GetFileStream()
        {
            return fs;

        }

        public void StartGrab()
        {
            ticks = 0;
            indicator.Stroke = System.Windows.Media.Brushes.White;

            thread = new Thread(new ThreadStart(GrabbingTask));
            thread.Start();
            GrabTrigger = true;
        }

        public void StopGrab()
        {
            GrabTrigger = false;
            IsGrabbing = false;

            Thread.Sleep(100);
            //thread.Abort();

            stream.Write(outputnone, 0, outputnone.Length);
            stream.Write(newLine, 0, newLine.Length);

            //stream.Close();
            //client.Close();

            FileWriter.Close();
            fs.Close();
        }

        public void Close()
        {
             
           
            GrabTrigger = false;
            IsGrabbing = false;
            IsConnected = false;
            IsReady = false;

            Thread.Sleep(100);

            if (thread.IsAlive)
            {
                thread.Abort();
            }

            FileWriter.Close();
            fs.Close();
            stream.Close();
            client.Close();
            

        }

        private void GrabbingTask()
        {
            if (client.Connected && IsReady)
            {

                filename = "D:\\flow_"+ PortNum + ".dat";

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

                    for (int j=0; j < realValues.Length; j++)
                    {

                        ticks++;

                        byte low = data[j * 3];
                        byte mid = data[j * 3 + 1];
                        byte high = data[j * 3 + 2];

                        float val;
                        float err;

                        val = (float)(low & 0b0011_1111) + (float)((mid & 0b0011_1111) << 6) + (float)((high & 0b0000_1111) << 12);
                        val = 0.01f * ( (102f / 65520f) * val - 1f) * 50f;

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

        Ellipse[] indicators;
      
        private static Connectionz instance;

        public Connection[] cons { get; private set; }
        public int Count { get; private set; } = 0;
        private Connectionz(int num)
        {
            //indicators.Add(1, MainWindow.Instance.EStatus1);

            indicators = new Ellipse[8];

            indicators[0] = MainWindow.Instance.EStatus1;
            indicators[1] = MainWindow.Instance.EStatus2;
            indicators[2] = MainWindow.Instance.EStatus3;
            indicators[3] = MainWindow.Instance.EStatus4;
            indicators[4] = MainWindow.Instance.EStatus5;
            indicators[5] = MainWindow.Instance.EStatus6;
            indicators[6] = MainWindow.Instance.EStatus7;
            indicators[7] = MainWindow.Instance.EStatus8;

            cons = new Connection[num];

            for (int i = 0; i < num; i++)
            {
                cons[i] = new Connection(indicators[i]);
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
            int i = 0;
            cons[i].StartGrab();
        }

        public void Stop()
        {
            int i = 0;
            cons[i].StopGrab();
        }

        public void Close()
        {
            for (int i = 0; i < Count; i++)
            {
                cons[i].Close();
            }


        }

        public void ConnectAllTask()
        {
            
        }

        public void ConnectAll()
        {

            for (int i = 0; i < Count; i++)
            {
                cons[i].Connect(ports[i]);
            }
            
            /*

            Thread conThread = new Thread(new ThreadStart(ConnectAllTask));
            conThread.Name = "Connections thread";
            conThread.IsBackground = true;

            conThread.Start();*/
            
        }

    }
}
