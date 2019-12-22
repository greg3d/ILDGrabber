using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DXTesting
{

    public class Plot
    {
        public float x1;
        public float x2;
        public float y1;
        public float y2;
        public float xMin;
        public float xMax;
        public float yMin;
        public float yMax;

        public Plot()
        {
            x1 = 0;
            x2 = 0;
            y1 = 0;
            y2 = 0;
            xMin = 0;
            xMax = 0;
            yMin = 0;
            yMax = 0;
        }
    }

    public partial class Canvas2DD : Canvas
    {
        public const int NROWS = 20;
        public const int NCOLS = 5;
      
        //private readonly Stopwatch renderTimer = new Stopwatch();
        //private Queue<int> frameCountHist = new Queue<int>();

        public bool DoRedraw = false;
        public bool IsPostProc = false;
        public bool IsGrabbing = false;

        private int oldVisibleCount = 0;

        private int _cursor = -1;

        //private Thread thread;

        float xx = 0.5f;
        float yy = 0.5f;

        float marginLeft = 15;
        float marginBottom = 12;
        float marginTop = 5;
        float marginRight = 40;

        float plotHeight = 100;

        public float xMin = 0f;
        public float xMax = 300;
        public float yMax = -1e9f;
        public float yMin = 1e9f;

        public Plot[] plotList;

        // float nPoints = 5000;

        bool autoYzoom = true;

        private SolidColorBrush bgMain;
        private SolidColorBrush bgPlot;
        private SolidColorBrush strokePlot;
        private SolidColorBrush strokeData;
        private SolidColorBrush strokeTick;

        //public Dictionary<string, SolidColorBrush> canvasBrushes;

        public bool AutoYzoom
        {
            get
            {
                return autoYzoom;
            }
            set
            {
                autoYzoom = value;
            }
        }
        
        
        public Canvas2DD()
        {
            bgPlot = new SolidColorBrush(new Color
            {
                R = 255,
                G = 255,
                B = 255,
                A = 255
            });

            bgPlot = new SolidColorBrush(new Color
            {
                R = 220,
                G = 220,
                B = 220,
                A = 255
            });
            
            strokePlot = new SolidColorBrush(new Color
            {
                R = 180,
                G = 180,
                B = 180,
                A = 255
            });
        }
        

        private Point Normalize(Plot curPlot, double x, double y)
        {
            Point retVec = new Point();


            if (x < curPlot.xMin || x > curPlot.xMax || y < curPlot.yMin || y > curPlot.yMax)
            {
                x = Double.NaN;
                y = Double.NaN;
            }

            retVec.X = curPlot.x1 + (x - curPlot.xMin) * (curPlot.x2 - curPlot.x1) / (curPlot.xMax - curPlot.xMin);
            retVec.Y = curPlot.y2 - (y - curPlot.yMin) * (curPlot.y2 - curPlot.y1) / (curPlot.yMax - curPlot.yMin);
            return retVec;
        }

        private Point NormalizeN(Plot curPlot, double x, double y)
        {
            Point retVec = new Point
            {
                X = curPlot.x1 + (x - curPlot.xMin) * (curPlot.x2 - curPlot.x1) / (curPlot.xMax - curPlot.xMin),
                Y = curPlot.y2 - (y - curPlot.yMin) * (curPlot.y2 - curPlot.y1) / (curPlot.yMax - curPlot.yMin)
            };
            return retVec;
        }

        public void ScaleY(int sign)
        {
            foreach (var item in plotList)
            {
                if (sign > 0)
                {
                    var xx = item.yMax - item.yMin;

                    item.yMin = item.yMin + xx / 20;
                    item.yMax = item.yMax - xx / 20;
                }
                else
                {
                    var xx = item.yMax - item.yMin;

                    item.yMin = item.yMin - xx / 20;
                    item.yMax = item.yMax + xx / 20;
                }
            }
        }

        public void ScaleX(int sign)
        {
            if (sign > 0)
            {
                var xx = xMax - xMin;

                xMin = xMin + xx / 20;
                xMax = xMax - xx / 20;
            }
            else
            {
                var xx = xMax - xMin;

                xMin = xMin - xx / 20;
                xMax = xMax + xx / 20;
            }
        }

        private float OptimalSpacing(double original)
        {
            double[] da = { 1.0, 2.0, 5.0 };
            double multiplier = Math.Pow(10, Math.Floor(Math.Log(original) / Math.Log(10)));
            double dmin = 100 * multiplier;
            double spacing = 0.0;
            double mn = 100;
            foreach (double d in da)
            {

                double delta = Math.Abs(original - d * multiplier);
                if (delta < dmin)
                {
                    dmin = delta;
                    spacing = d * multiplier;
                }
                if (d < mn)
                {
                    mn = d;
                }
            }
            if (Math.Abs(original - 10 * mn * multiplier) < Math.Abs(original - spacing))
            {
                spacing = 10 * mn * multiplier;
            }

            return (float)spacing;
        }

        private void DoAutoScale(ref float[] X, ref float[] Y, ref Plot curPlot, bool AutoScale)
        {

            if (AutoScale)
            {
                curPlot.yMin = 1e9f;
                curPlot.yMax = -1e9f;
                curPlot.xMin = 0f;
                curPlot.xMax = 1f;

                var N = X.Length;

                foreach (var val in Y)
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

                // определяем xMax и xMin
                curPlot.xMax = X[N - 1];
                curPlot.xMin = X[0];

                xMax = curPlot.xMax;
                xMin = curPlot.xMin;
            }
            else
            {
                curPlot.xMin = xMin;
                curPlot.xMax = xMax;
            }

        }


        protected override void OnRender(DrawingContext dc)
        {

           
            
            Redraw(ref dc);
            
            base.OnRender(dc);
        }
        public void Redraw(ref DrawingContext dc)
        {

           
            Connectionz conz = Connectionz.getInstance();

            if (conz.ReadyCount > 0)
            {
                dc.DrawRectangle(bgPlot, new Pen(bgPlot, 1), new Rect(5, 5, 100, 100));
                //int ind = 0;
                IsGrabbing = true;
                IsPostProc = true;

                int visibleCount = 0;
                foreach (var con in conz.cons)
                {
                    if (con.IsReady && con.IsVisible)
                    {
                        visibleCount++;
                        IsGrabbing = IsGrabbing && con.IsGrabbing;
                        IsPostProc = IsPostProc && con.IsPostProc;
                    }
                }

                if (visibleCount != oldVisibleCount)
                {
                    plotList = new Plot[visibleCount];
                    for (int gg = 0; gg < visibleCount; gg++)
                    {
                        plotList[gg] = new Plot();
                    }
                    autoYzoom = true;
                    oldVisibleCount = visibleCount;
                }

                plotHeight = (float)Math.Round(ActualHeight / visibleCount);

                int i = 0;
                foreach (var con in conz.cons)
                {
                    if (con.IsReady && con.IsVisible && (IsGrabbing ^ IsPostProc))
                    {

                        var curPlot = plotList[i];

                        curPlot.x1 = xx + marginLeft;
                        curPlot.x2 = (float)ActualWidth - xx - marginRight;
                        curPlot.y1 = yy + i * plotHeight + marginTop;
                        curPlot.y2 = yy + i * plotHeight + plotHeight - marginBottom;



                        //Polygon PlotArea = new Polygon
                        //{
                        //    Stroke = plotstroke,
                        //    Fill = plotbg
                        //};

                        //PlotArea.Points.Add(new Point(curPlot.x1, curPlot.y1));
                        //PlotArea.Points.Add(new Point(curPlot.x2, curPlot.y1));
                        //PlotArea.Points.Add(new Point(curPlot.x2, curPlot.y2));
                        //PlotArea.Points.Add(new Point(curPlot.x1, curPlot.y2));
                        //PlotArea.Points.Add(new Point(curPlot.x1, curPlot.y1));
                        //canvas.Children.Add(PlotArea);

                        dc.DrawRectangle(bgPlot, new Pen(bgPlot, 1), new Rect(5, 5, 100, 100));
                        dc.DrawRectangle(bgPlot, new Pen(bgPlot, 1), new Rect(curPlot.x1, curPlot.y1, curPlot.x1 + curPlot.x2, curPlot.y1 + curPlot.y2));

        
                        float tickGapX = 5f; //sec

                        float tickGapY = 10f; //mm

                        RealData data;

                        if (IsGrabbing)
                        {
                            data = con.vdata;
                            autoYzoom = true;
                        }
                        else
                        // if (IsPostProc)
                        {
                            data = con.rdata;

                            var xScale = (curPlot.x2 - curPlot.x1) / (curPlot.xMax - curPlot.xMin);
                            var xSpacing = 80 / xScale;

                            tickGapX = OptimalSpacing(xSpacing);

                            var yScale = (curPlot.y2 - curPlot.y1) / (curPlot.yMax - curPlot.yMin);
                            var ySpacing = 20 / yScale;

                            tickGapY = OptimalSpacing(ySpacing);

                        }

                        DoAutoScale(ref data.X, ref data.Y, ref curPlot, autoYzoom);

                        // Определяем насколько точек больше чем на экране
                        var N = data.X.Length;
                        float ww = curPlot.x2 - curPlot.x1;
                        int step = 1;

                        if (N / ww > 2)
                        {
                            step = (int)Math.Floor(N / ww);
                        }

                        float xStart = (float)Math.Ceiling(curPlot.xMin / tickGapX) * tickGapX;
                        float xEnd = (float)Math.Floor(curPlot.xMax / tickGapX) * tickGapX;

                        float yStart = (float)Math.Ceiling(curPlot.yMin / tickGapY) * tickGapY;
                        float yEnd = (float)Math.Floor(curPlot.yMax / tickGapY) * tickGapY;

                        //labelTextFormat.TextAlignment = TextAlignment.Center;
                        //labelTextFormat.ParagraphAlignment = ParagraphAlignment.Near;

                        // рисуем тики X
                        for (var k = xStart - tickGapX; k < xEnd + tickGapX; k = k + tickGapX)
                        {
                           // Polyline line = new Polyline
                           // {
                                //Stroke = plotstroke
                          //  };

                            Point st = NormalizeN(curPlot, k, 0);
                            Point p1 = new Point(st.X, curPlot.y2 - 2);
                            Point p2 = new Point(st.X, curPlot.y1 + 2);


                            if ((st.X > curPlot.x1) & (st.X < curPlot.x2))
                            {

                               // line.Points.Add(p1);
                                //line.Points.Add(p2);

                                //canvas.Children.Add(line);
                            }

                            // target.DrawLine(point1, point2, helperBrush);
                            // drawText(k.ToString("F2"), ref labelTextFormat, ref brushblack, ref target, tickPoint.X, curPlot.y2 + 2);
                        }

                        // рисуем тики для Y 
                        // labelTextFormat.TextAlignment = TextAlignment.Leading;
                        // labelTextFormat.ParagraphAlignment = ParagraphAlignment.Center;

                        for (var k = yStart - tickGapY; k < yEnd + tickGapY; k = k + tickGapY)
                        {

                           // Polyline line = new Polyline
                           // {
                           //     Stroke = plotstroke
                           // };

                            Point st = NormalizeN(curPlot, data.X[(int)N - 1], k);
                            Point p1 = new Point(curPlot.x1 + 1, st.Y);
                            Point p2 = new Point(curPlot.x2 - 1, st.Y);

                            if ((st.Y > curPlot.y1) & (st.Y < curPlot.y2))
                            {
                            //    line.Points.Add(p1);
                             //   line.Points.Add(p2);

                               // canvas.Children.Add(line);

                                //      drawText(k.ToString("F2"), ref labelTextFormat, ref transparentBrush, ref target, point2.X + 3, point2.Y);
                            }
                        }

                        //   labelTextFormat.TextAlignment = TextAlignment.Leading;
                        //  labelTextFormat.ParagraphAlignment = ParagraphAlignment.Center;

                        // рисуем значения yMax и yMin
                        // drawText(curPlot.yMax.ToString("F2"), ref labelTextFormat, ref brushblack, ref target, curPlot.x2 + 4, curPlot.y1 + 6);
                        // drawText(curPlot.yMin.ToString("F2"), ref labelTextFormat, ref brushblack, ref target, curPlot.x2 + 4, curPlot.y2 - 6);


                        float cursorVal = 0;

                        //Polyline dataseries = new Polyline();
                        //dataseries.Stroke = Brushes.Blue;

                        // Рисуем сам график
                        for (int jjj = 0; jjj < N; jjj = jjj + step)
                        {

                            Point pt = NormalizeN(curPlot, data.X[jjj], data.Y[jjj]);
                            //Point endPoint = Normalize(curPlot, data.X[jjj], data.Y[jjj]);

                            //dataseries.Points.Add(pt);

                            //   RawVector2 stPoint = PointToCanvas(curPlot, data.X[jjj], data.Y[jjj]);
                            //   RawVector2 endPoint = PointToCanvas(curPlot, data.X[jjj + step], data.Y[jjj + step]);
                            //   target.DrawLine(stPoint, endPoint, blueBrush);

                            // РИСУЕМ КУРСОР 
                            if (_cursor > 0 && _cursor == Math.Round(pt.X))
                            {
                                Point p1 = new Point(_cursor, curPlot.y1 + 1);
                                Point p2 = new Point(_cursor, curPlot.y2 - 1);

                                //Line line = new Line
                                //{
                                  //  X1 = p1.X,
                                   // X2 = p2.X,
                                   // Y1 = p1.Y,
                                   // Y2 = p2.Y,
                                   // Stroke = Brushes.Black
                                //};

                                //canvas.Children.Add(line);
                                //     RawVector2 stPoint2 = new RawVector2(_cursor, curPlot.y1 + 1);
                                //     RawVector2 endPoint2 = new RawVector2(_cursor, curPlot.y2 - 1);
                                //      target.DrawLine(stPoint2, endPoint2, brushblack);
                                cursorVal = data.Y[jjj];
                            }
                        }

                        //canvas.Children.Add(dataseries);


                        // рисуем текущее значение

                        string curVal;
                        if (IsGrabbing)
                        {
                            curVal = data.Y[N - 1].ToString("F3");
                        }
                        else
                        {
                            curVal = cursorVal.ToString("F3");
                        }

                        //  headerTextFormat.TextAlignment = TextAlignment.Trailing;
                        // headerTextFormat.ParagraphAlignment = ParagraphAlignment.Near;

                        // drawText(curVal, ref headerTextFormat, ref brushblack, ref target, curPlot.x2 - 4, curPlot.y1 + 2);


                        // рисуем вспомогательное 
                        // labelTextFormat.TextAlignment = TextAlignment.Leading;
                        // headerTextFormat.TextAlignment = TextAlignment.Leading;
                        // labelTextFormat.ParagraphAlignment = ParagraphAlignment.Near;
                        //  drawText((con.ConnID + 1).ToString(), ref headerTextFormat, ref brushblack, ref target, curPlot.x1 + 4, curPlot.y1 + 2);
                        //  drawText(con.Serial, ref labelTextFormat, ref brushblack, ref target, curPlot.x1 + 12, curPlot.y1 + 4);

                        i++;
                    }

                }

                if (IsPostProc)
                {
                    autoYzoom = false;
                }

                //DoRedraw = false;
            }
        }
    }
}
