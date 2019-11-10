using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Multimedia;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using System;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Text;
using SharpDX.DirectWrite;

namespace DXTesting
{
    class Plot
    {
        public float x1;
        public float x2;
        public float y1;
        public float y2;
        public float xMin;
        public float xMax;
        public float yMin;
        public float yMax;
    }
    class ChartControl : G2dControl
    {

        private Thread thread;

        float xx = 0.5f;
        float yy = 0.5f;

        float marginLeft = 10;
        float marginBottom = 10;
        float marginTop = 5;
        float marginRight = 50;

        float plotHeight = 100;

        float xMin = 0f;
        float xMax = 300;
        float yMax = -1e9f;
        float yMin = 1e9f;

        float nPoints = 5000;

        //int numberOfPlots = 1;


        int Mode = 0;

        //public RawVector2[] pt1 = new RawVector2[nPoub];
                
        public ChartControl()
        {

            /*
            float nnn =6.28f / num;
            for (int i = 0; i < num; i++)
            {
                pt1[i] = new RawVector2(i * nnn, (float)Math.Sin(i * nnn));
            }*/

            resCache.Add("BlueBrush", t => new SolidColorBrush(t, new RawColor4(0.0f, 0.0f, 1.0f, 1.0f)));
            resCache.Add("BlackBrush", t => new SolidColorBrush(t, new RawColor4(0.0f, 0.0f, 0.0f, 1.0f)));
            resCache.Add("HelperBrush", t => new SolidColorBrush(t, new RawColor4(0.2f, 0.2f, 0.2f, 0.7f)));

        }

        private RawVector2 PointToCanvas(Plot curPlot, float x, float y)
        {
            RawVector2 retVec = new RawVector2();

            /*
            if ( x < xMin || x > xMax || y < yMin || y > yMax)
            {
                x = Single.NaN;
                y = Single.NaN;
            }*/

            retVec.X = curPlot.x1 + (x - curPlot.xMin) * (curPlot.x2 - curPlot.x1) / (curPlot.xMax - curPlot.xMin);
            retVec.Y = curPlot.y2 - (y - curPlot.yMin) * (curPlot.y2 - curPlot.y1) / (curPlot.yMax - curPlot.yMin);

            return retVec;
        }

        public override void Render(RenderTarget target)
        {
            //RenderWait = 2;
            target.Clear(new RawColor4(1.0f, 1.0f, 1.0f, 1.0f));

            target.AntialiasMode = AntialiasMode.Aliased;

            Brush brushblack = resCache["BlackBrush"] as Brush;
            Brush helperBrush = resCache["HelperBrush"] as Brush;
            Brush blueBrush = resCache["BlueBrush"] as Brush;

            TextFormat labelTextFormat = new TextFormat(new SharpDX.DirectWrite.Factory(), "Arial", 12);
            labelTextFormat.ParagraphAlignment = ParagraphAlignment.Center;
            labelTextFormat.TextAlignment = TextAlignment.Center;


            Connectionz conz = Connectionz.getInstance();
            if (conz.Count > 0)
            {
                for (int i = 0; i < conz.Count; i++)
                {

                    Plot curPlot = new Plot
                    {
                        x1 = xx + marginLeft,
                        x2 = (float)ActualWidth - xx - marginRight,
                        y1 = yy + i * plotHeight + marginTop,
                        y2 = yy + i * plotHeight + plotHeight - marginBottom,
                        yMin = yMin,
                        yMax = yMax,
                        xMin = 0f,
                        xMax = 10f
                    };

                    RawRectangleF PlotArea = new RawRectangleF(curPlot.x1, curPlot.y1, curPlot.x2, curPlot.y2);
                    target.DrawRectangle(PlotArea, brushblack, 1.0f);

                    Connection con = conz.cons[i];
                    //Trace.WriteLine(con.IsGrabbing);

                    /*
                    switch (Mode)
                    {
                        case 1:
                            Console.WriteLine("Case 1");
                            break;
                        case 2:
                            Console.WriteLine("Case 2");
                            break;
                        default:
                            Console.WriteLine("Default case");
                            break;
                    }*/

                    if (con.IsGrabbing)
                    {

                        //Trace.Write('d');
                        
                        //FileStream fs = new FileStream("D:\\file.txt",FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        foreach (var val in con.vdata.Y)
                        {
                            if (val > curPlot.yMax)
                            {
                                curPlot.yMax = val;
                            }

                            if (val < curPlot.yMin)
                            {
                                curPlot.yMin = val;
                            }

                            if (curPlot.yMax == curPlot.yMin)
                            {
                                curPlot.yMax = curPlot.yMax + 1;
                                curPlot.yMin = curPlot.yMin - 1;
                            }
                        }

                        curPlot.xMax = con.vdata.X[(int)nPoints-1];
                        curPlot.xMin = con.vdata.X[0];

                        int tickGap = 5; //sec
                        var tnum = xMax / tickGap;

                        for (var k = curPlot.xMin + tickGap; k < curPlot.xMax; k = k + tickGap)
                        {

                            RawVector2 tickPoint = PointToCanvas(curPlot, k, 0);
                            RawVector2 point1 = new RawVector2(tickPoint.X, curPlot.y2 - 2);
                            RawVector2 point2 = new RawVector2(tickPoint.X, curPlot.y1 + 2);

                            target.DrawLine(point1, point2, helperBrush);

                           

                            //target.DrawText(k.ToString(), labelTextFormat, 

                            //ctx.fillText(k, tickPoint.x, curPlot.y2 + bottomMargin / 2);
                        }

                        // крайний нижний тик справа
                        // ctx.fillText(curPlot.xMax, curPlot.x2, curPlot.y2 + bottomMargin / 2);

                        // расчет тиков для Y
                        float realPlotHeight = curPlot.y2 - curPlot.y1;
                        float yTickCount = (float)Math.Ceiling(realPlotHeight / 20);

                        double yTickGap = (curPlot.yMax - curPlot.yMin) / yTickCount;
                        var realYTickGap = 1.0e+020;
                        while (realYTickGap > yTickGap)
                        {
                            realYTickGap = realYTickGap / 10;
                        }

                        realYTickGap = realYTickGap * 10;

                        if (realYTickGap > (curPlot.yMax - curPlot.yMin))
                        {
                            realYTickGap = realYTickGap / 2;
                        }

                        var minYTick = Math.Floor(curPlot.yMin / realYTickGap) * realYTickGap;

                        for (var k = minYTick + realYTickGap; k < curPlot.yMax; k = k + realYTickGap)
                        {

                            RawVector2 tickPoint = PointToCanvas(curPlot, con.vdata.Y[4999], (float)k);
                            RawVector2 point1 = new RawVector2(curPlot.x1 + 1, tickPoint.Y);
                            RawVector2 point2 = new RawVector2(curPlot.x2 + 1, tickPoint.Y);

                            target.DrawLine(point1, point2, helperBrush);


                            //ctx.fillStyle = "#CCC";
                            //ctx.textAlign = "left";
                            //ctx.textBaseline = "middle";
                            //ctx.font = "10px Arial";
                            //ctx.fillText(k, curPlot.x2 + 4, tickPoint.y);
                        }

                        // yMax и yMin
                        //ctx.fillStyle = "#000";
                        //ctx.textAlign = "left";
                        //ctx.textBaseline = "middle";
                        //ctx.font = "10px Arial";
                        //ctx.fillText(curPlot.yMax, curPlot.x2 + 4, curPlot.y1);
                        //ctx.fillText(curPlot.yMin, curPlot.x2 + 4, curPlot.y2);

                        // текущее значение
                        //ctx.fillStyle = "#eee";
                        //ctx.fillRect(curPlot.x1 + 1, curPlot.y1 + 1, 150, 22);
                        //ctx.fillStyle = "#000";
                        //ctx.textAlign = "left";
                        //ctx.textBaseline = "top";
                        //ctx.font = "14px Arial";
                        //ctx.fillText(channel.name + ": " + channel.y[nPoints - 1], curPlot.x1 + 4, curPlot.y1 + 4);

                        // Drawing plot 



                        /*
                        ctx.beginPath();
                        ctx.strokeStyle = colors[kk];
                        kk++;
                        if (kk >= colors.length)
                        {
                            kk = 0;
                        }*/

                        //ctx.moveTo(stPoint.x, stPoint.y);

                        //Trace.Write("st");

                        float ww = curPlot.x2 - curPlot.x1;
                        int step = 1;

                        if (nPoints/ww > 2)
                        {
                            step = (int)Math.Floor(nPoints / ww);
                        }
                        
                        for (int jjj = 0; jjj < nPoints-step; jjj = jjj + step)
                        {
                            RawVector2 stPoint = PointToCanvas(curPlot, con.vdata.X[jjj], con.vdata.Y[jjj]);
                            RawVector2 endPoint = PointToCanvas(curPlot, con.vdata.X[jjj+step], con.vdata.Y[jjj + step]);
                            target.DrawLine(stPoint, endPoint, blueBrush);
                        }
                        //Thread.Sleep(1);
                        //Trace.Write("en");

                        //RawVector2 stPoint = PointToCanvas(curPlot, con.vdata.X[0], con.vdata.Y[0]);
                        //RawVector2 endPoint = PointToCanvas(curPlot, con.vdata.X[4999], con.vdata.Y[4999]);

                        //target.DrawLine(new RawVector2(5, curPlot.y1 / 2 + curPlot.y2 / 2), new RawVector2(5 + 50, curPlot.y1 / 2 + curPlot.y2 / 2), blueBrush);


                        /*
                        for (int jj = 5000 - 1; jj > 5; jj=-5)
                        {
                            RawVector2 stPoint = PointToCanvas(curPlot, con.vdata.X[jj], con.vdata.Y[jj]);
                            RawVector2 endPoint = PointToCanvas(curPlot, con.vdata.X[jj-5], con.vdata.Y[jj-5]);
                            target.DrawLine(new RawVector2(5, curPlot.y1/2 + curPlot.y2/2), new RawVector2(5+50, curPlot.y1 / 2 + curPlot.y2 / 2), blueBrush);
                            target.DrawLine(stPoint, endPoint, blueBrush);
                        }*/


                    }

                }
            }
            /*
            
            RawRectangleF PlotArea = new RawRectangleF(offset+0.5f, offset+ 0.5f, r+0.5f, b+0.5f);

            w = PlotArea.Right - PlotArea.Left;
            h = PlotArea.Bottom - PlotArea.Top;

            Brush brush = resCache["BlueBrush"] as Brush;
            Brush brushblack = resCache["BlackBrush"] as Brush;

            target.DrawRectangle(PlotArea, brushblack, 1.0f);
            //target.DrawRectangle(new RawRectangleF(x, y, x + w, y + h), brush);
            //int newNum = 0;
            float part = num / w;
            */
            
            /*
            if (num > w)
            {
                newNum = (int)w;

                
                for (int i = 0; i < newNum; i++)
                {

                    float rrr = 25f * rnd.NextFloat(-0.5f, 0.5f);
                    

                    target.DrawLine(new RawVector2(PlotArea.Left+i+0.5f,PlotArea.Bottom/2+PlotArea.Top/2-rrr), new RawVector2(PlotArea.Left + i + 0.5f, PlotArea.Bottom / 2 + PlotArea.Top / 2 + rrr), brush, 1.0f);
                }

            }*/

            /*
            for (int i = 1; i < num; i++)
            {
                target.DrawLine(PointToCanvas(pt1[i - 1]), PointToCanvas(pt1[i]), brush);
            }*/



            /*

            x = x + dx;
            y = y + dy;
            if (x >= ActualWidth - w || x <= 0)
            {
                dx = -dx;
            }
            if (y >= ActualHeight - h || y <= 0)
            {
                dy = -dy;
            }*/

        }
    }
}