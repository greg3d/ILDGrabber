using System;
//using System.Windows;
//using System.Windows.Media;
using System.Drawing;

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

    public class Canvas2DD : GDICanvas
    {

        public bool DoRedraw = false;
        public bool IsPostProc = false;
        public bool IsGrabbing = false;

        private Font smallFont = new Font("Arial", 8f);
        private Font normalFont = new Font("Arial", 10f);
        private Font bigFont = new Font("Arial", 12f);

        private int oldVisibleCount = 0;

        private int _cursor = -1;

        float xx = 0.0f;
        float yy = 0.0f;

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

        bool autoYzoom = true;

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

        public void DrawCursor(int x)
        {
            if (_cursor != x)
            {
                _cursor = x;
            }
        }

        private Point Normalize(Plot curPlot, double x, double y)
        {
            Point retVec = new Point();

            if (x < curPlot.xMin || x > curPlot.xMax || y < curPlot.yMin || y > curPlot.yMax)
            {
                x = Double.NaN;
                y = Double.NaN;
            }

            retVec.X = (int)(curPlot.x1 + (x - curPlot.xMin) * (curPlot.x2 - curPlot.x1) / (curPlot.xMax - curPlot.xMin));
            retVec.Y = (int)(curPlot.y2 - (y - curPlot.yMin) * (curPlot.y2 - curPlot.y1) / (curPlot.yMax - curPlot.yMin));
            return retVec;
        }

        private Point NormalizeN(Plot curPlot, double x, double y)
        {
            Point retVec = new Point
            {
                X = (int)(curPlot.x1 + (x - curPlot.xMin) * (curPlot.x2 - curPlot.x1) / (curPlot.xMax - curPlot.xMin)),
                Y = (int)(curPlot.y2 - (y - curPlot.yMin) * (curPlot.y2 - curPlot.y1) / (curPlot.yMax - curPlot.yMin))
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

        public Canvas2DD(System.Windows.Controls.Panel c, System.Windows.Controls.Image image) : base(c, image)
        {

        }

        protected override void draw()
        {
            var bgcol = Color.FromArgb(255, 0xEE, 0xEE, 0xF2);

            graphics.Clear(bgcol);
            Connectionz conz = Connectionz.getInstance();

            if (conz.ReadyCount > 0 && ActualWidth > 0)
            {

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

                plotHeight = (float)Math.Round((float)ActualHeight / visibleCount);

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


                        graphics.DrawRectangle(
                            Pens.Gray,
                            curPlot.x1,
                            curPlot.y1,
                            curPlot.x2 - curPlot.x1,
                            curPlot.y2 - curPlot.y1);
                        graphics.FillRectangle(
                            Brushes.White,
                            curPlot.x1,
                            curPlot.y1,
                            curPlot.x2 - curPlot.x1,
                            curPlot.y2 - curPlot.y1);

                        float tickGapX = 5f; //sec
                        float tickGapY = 10f; //mm

                        RealData data;

                        if (IsGrabbing)
                        {
                            data = con.vdata;
                            autoYzoom = true;
                        }
                        else
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

                        var sf = new StringFormat();

                        // рисуем тики X
                        sf.Alignment = StringAlignment.Center;
                        sf.LineAlignment = StringAlignment.Near;

                        for (var k = xStart - tickGapX; k < xEnd + tickGapX; k = k + tickGapX)
                        {
                            Point st = NormalizeN(curPlot, k, 0);
                            Point p1 = new Point(st.X, (int)curPlot.y2 - 2);
                            Point p2 = new Point(st.X, (int)curPlot.y1 + 2);

                            if ((st.X > curPlot.x1) & (st.X < curPlot.x2))
                            {
                                graphics.DrawLine(Pens.LightGray, p1, p2);
                                graphics.DrawString(k.ToString("F1"), smallFont, Brushes.Black, st.X, curPlot.y2 + 2, sf);
                            }
                        }

                        // рисуем тики для Y
                        sf.Alignment = StringAlignment.Near;
                        sf.LineAlignment = StringAlignment.Center;

                        for (var k = yStart - tickGapY; k < yEnd + tickGapY; k = k + tickGapY)
                        {
                            Point st = NormalizeN(curPlot, data.X[(int)N - 1], k);
                            Point p1 = new Point((int)curPlot.x1 + 1, st.Y);
                            Point p2 = new Point((int)curPlot.x2 - 1, st.Y);

                            if ((st.Y > curPlot.y1) & (st.Y < curPlot.y2))
                            {
                                graphics.DrawLine(Pens.LightGray, p1, p2);
                                graphics.DrawString(k.ToString("F1"), smallFont, Brushes.Gray, curPlot.x2 + 2, p2.Y, sf);
                            }
                        }

                        // рисуем значения yMax и yMin
                        graphics.DrawString(curPlot.yMax.ToString("F2"), smallFont, Brushes.Black, curPlot.x2 + 2, curPlot.y1, sf);
                        graphics.DrawString(curPlot.yMin.ToString("F2"), smallFont, Brushes.Black, curPlot.x2 + 2, curPlot.y2, sf);

                        float cursorVal = 0;


                        // Рисуем сам график
                        for (int jjj = 0; jjj < N - step; jjj = jjj + step)
                        {
                            var a = data.Y[jjj];
                            var b = data.Y[jjj + step];
                            var c = data.X[jjj];
                            
                            var rule = true;
                            rule = rule && a < curPlot.yMax && a > curPlot.yMin;
                            rule = rule && b < curPlot.yMax && b > curPlot.yMin;
                            rule = rule && c < curPlot.xMax && c > curPlot.xMin;


                            if ( rule )
                            {
                                Point pt = NormalizeN(curPlot, data.X[jjj], data.Y[jjj]);
                                Point pt2 = NormalizeN(curPlot, data.X[jjj + step], data.Y[jjj + step]);

                                graphics.DrawLine(Pens.Blue, pt, pt2);


                                // РИСУЕМ КУРСОР 
                                if (IsPostProc && _cursor > 0 && _cursor == Math.Round((double)pt.X))
                                {
                                    Point p1 = new Point(_cursor, (int)curPlot.y1 + 1);
                                    Point p2 = new Point(_cursor, (int)curPlot.y2 - 1);

                                    graphics.DrawLine(Pens.Black, p1, p2);

                                    cursorVal = data.Y[jjj];
                                }
                            }

                            
                        }

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

                        sf.Alignment = StringAlignment.Far;
                        sf.LineAlignment = StringAlignment.Near;
                        graphics.DrawString(curVal, normalFont, Brushes.Blue, curPlot.x2 - 2, curPlot.y1 + 2, sf);

                        // рисуем вспомогательное 
                        sf.Alignment = StringAlignment.Near;
                        sf.LineAlignment = StringAlignment.Near;

                        graphics.DrawString((con.ConnID + 1).ToString() + " | " + con.Serial, normalFont, Brushes.Black, curPlot.x1 + 2, curPlot.y1 + 2, sf);

                        i++;
                    }

                }

                if (IsPostProc)
                {
                    autoYzoom = false;
                }


                DoRedraw = false;
            }


            //base.OnRender(dc);
        }

    }
}
