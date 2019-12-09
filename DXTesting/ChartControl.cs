using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;
using System;

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

        //public int Index;
        //public bool visible;

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
    class ChartControl : G2dControl
    {
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

        float nPoints = 5000;

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


        public ChartControl()
        {
            resCache.Add("BlueBrush", t => new SolidColorBrush(t, new RawColor4(0.0f, 0.0f, 1.0f, 1.0f)));
            resCache.Add("BlackBrush", t => new SolidColorBrush(t, new RawColor4(0.0f, 0.0f, 0.0f, 1.0f)));
            resCache.Add("HelperBrush", t => new SolidColorBrush(t, new RawColor4(0.2f, 0.2f, 0.2f, 0.7f)));
            resCache.Add("SemiTransparentBrush", t => new SolidColorBrush(t, new RawColor4(0.2f, 0.2f, 0.2f, 0.3f)));

        }

        private void drawText(string text, ref TextFormat format, ref Brush brush, ref RenderTarget t, float x, float y)
        {

            float w = text.Length * format.FontSize;
            format.WordWrapping = WordWrapping.NoWrap;

            float x1;
            float x2;
            float y1;
            float y2;

            switch (format.ParagraphAlignment)
            {
                case ParagraphAlignment.Near:
                    y1 = y;
                    y2 = y + format.FontSize * 1.5f;
                    break;
                case ParagraphAlignment.Far:
                    y1 = y - format.FontSize * 1.5f;
                    y2 = y;
                    break;
                case ParagraphAlignment.Center:
                    y1 = y - format.FontSize * 1.5f / 2;
                    y2 = y + format.FontSize * 1.5f / 2;
                    break;
                default:
                    y1 = y - format.FontSize * 1.5f / 2;
                    y2 = y + format.FontSize * 1.5f / 2;
                    break;
            }

            switch (format.TextAlignment)
            {
                case TextAlignment.Leading:
                    x1 = x;
                    x2 = x + w;
                    break;
                case TextAlignment.Trailing:
                    x1 = x - w;
                    x2 = x;
                    break;
                case TextAlignment.Center:
                    x1 = x - w / 2;
                    x2 = x + w / 2;
                    break;
                default:
                    x1 = x;
                    x2 = x + w;
                    break;
            }

            t.DrawText(text, format, new RawRectangleF(x1, y1, x2, y2), brush);

        }

        private RawVector2 PointToCanvas(Plot curPlot, float x, float y)
        {
            RawVector2 retVec = new RawVector2();

            if (x < curPlot.xMin || x > curPlot.xMax || y < curPlot.yMin || y > curPlot.yMax)
            {
                x = Single.NaN;
                y = Single.NaN;
            }

            retVec.X = curPlot.x1 + (x - curPlot.xMin) * (curPlot.x2 - curPlot.x1) / (curPlot.xMax - curPlot.xMin);
            retVec.Y = curPlot.y2 - (y - curPlot.yMin) * (curPlot.y2 - curPlot.y1) / (curPlot.yMax - curPlot.yMin);

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

            //foreach (var item in plotList)
            //{
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

            // }

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

        //private void Redraw()

        public void DrawCursor(int x)
        {
            if (_cursor != x)
            {
                _cursor = x;
            }

        }

        public override void Render(RenderTarget target)
        {
            //RenderWait = 2;

            target.AntialiasMode = AntialiasMode.Aliased;

            //target.AntialiasMode = AntialiasMode.Aliased;

            Brush brushblack = resCache["BlackBrush"] as Brush;
            Brush helperBrush = resCache["HelperBrush"] as Brush;
            Brush blueBrush = resCache["BlueBrush"] as Brush;
            Brush transparentBrush = resCache["SemiTransparentBrush"] as Brush;

            var labelTextFormat = new TextFormat(new SharpDX.DirectWrite.Factory(), "Arial", 10)
            {
                ParagraphAlignment = ParagraphAlignment.Far,
                TextAlignment = TextAlignment.Center
            };


            Connectionz conz = Connectionz.getInstance();

            if (conz.ReadyCount > 0 && DoRedraw)
            {
                //int ind = 0;
                IsGrabbing = true;
                IsPostProc = true;

                int visibleCount = 0;
                foreach (var con in conz.cons)
                {
                    if (con.IsReady)
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

                    oldVisibleCount = visibleCount;
                }

                target.Clear(new RawColor4(1.0f, 1.0f, 1.0f, 1.0f));
                plotHeight = (float)Math.Round(ActualHeight / visibleCount);

                int i = 0;
                foreach (var con in conz.cons)
                {
                    if (con.IsReady && (IsGrabbing ^ IsPostProc))
                    {

                        var curPlot = plotList[i];

                        curPlot.x1 = xx + marginLeft;
                        curPlot.x2 = (float)ActualWidth - xx - marginRight;
                        curPlot.y1 = yy + i * plotHeight + marginTop;
                        curPlot.y2 = yy + i * plotHeight + plotHeight - marginBottom;

                        RawRectangleF PlotArea = new RawRectangleF(curPlot.x1, curPlot.y1, curPlot.x2, curPlot.y2);
                        target.DrawRectangle(PlotArea, helperBrush, 1.0f);

                        if (IsGrabbing)
                        {
                            autoYzoom = true;
                            DoAutoScale(ref con.vdata.X, ref con.vdata.Y, ref curPlot, autoYzoom);

                            // расстояние между тиками по X = 5 сек
                            int tickGap = 5; //sec


                            //var tnum = xMax / tickGap;
                            float xStart = (float)Math.Ceiling(xMin / tickGap) * tickGap;
                            float xEnd = (float)Math.Floor(xMax / tickGap) * tickGap;

                            labelTextFormat.TextAlignment = TextAlignment.Center;
                            labelTextFormat.ParagraphAlignment = ParagraphAlignment.Near;

                            // рисуем тики X
                            for (var k = xStart - tickGap; k < xEnd + tickGap; k = k + tickGap)
                            {

                                RawVector2 tickPoint = PointToCanvas(curPlot, k, 0);
                                RawVector2 point1 = new RawVector2(tickPoint.X, curPlot.y2 - 2);
                                RawVector2 point2 = new RawVector2(tickPoint.X, curPlot.y1 + 2);

                                target.DrawLine(point1, point2, helperBrush);

                                drawText(k.ToString("F2"), ref labelTextFormat, ref brushblack, ref target, tickPoint.X, curPlot.y2 + 2);
                            }

                            //  рисуем тики для Y 
                            labelTextFormat.TextAlignment = TextAlignment.Leading;
                            labelTextFormat.ParagraphAlignment = ParagraphAlignment.Center;

                            for (var k = -20; k < 25; k = k + 10)
                            {
                                RawVector2 tickPoint = PointToCanvas(curPlot, con.vdata.X[(int)nPoints - 1], k);
                                RawVector2 point1 = new RawVector2(curPlot.x1 + 1, tickPoint.Y);
                                RawVector2 point2 = new RawVector2(curPlot.x2 + 1, tickPoint.Y);

                                target.DrawLine(point1, point2, transparentBrush);

                                drawText(k.ToString("F2"), ref labelTextFormat, ref transparentBrush, ref target, point2.X + 3, point2.Y);
                            }

                            labelTextFormat.TextAlignment = TextAlignment.Leading;
                            labelTextFormat.ParagraphAlignment = ParagraphAlignment.Center;

                            // рисуем значения yMax и yMin
                            drawText(curPlot.yMax.ToString("F2"), ref labelTextFormat, ref brushblack, ref target, curPlot.x2 + 4, curPlot.y1 + 6);
                            drawText(curPlot.yMin.ToString("F2"), ref labelTextFormat, ref brushblack, ref target, curPlot.x2 + 4, curPlot.y2 - 6);

                            // рисуем текущее значение
                            labelTextFormat.TextAlignment = TextAlignment.Leading;
                            labelTextFormat.ParagraphAlignment = ParagraphAlignment.Near;
                            drawText(con.vdata.Y[(int)nPoints - 1].ToString("F3"), ref labelTextFormat, ref brushblack, ref target, curPlot.x1 + 4, curPlot.y1 + 4);

                            // Рисуем сам график
                            float ww = curPlot.x2 - curPlot.x1;
                            int step = 1;

                            if (nPoints / ww > 2)
                            {
                                step = (int)Math.Floor(nPoints / ww);
                            }

                            for (int jjj = 0; jjj < nPoints - step; jjj = jjj + step)
                            {
                                RawVector2 stPoint = PointToCanvas(curPlot, con.vdata.X[jjj], con.vdata.Y[jjj]);
                                RawVector2 endPoint = PointToCanvas(curPlot, con.vdata.X[jjj + step], con.vdata.Y[jjj + step]);
                                target.DrawLine(stPoint, endPoint, blueBrush);
                            }
                        }

                        if (IsPostProc)
                        {

                            var data = con.rdata;

                            DoAutoScale(ref data.X, ref data.Y, ref curPlot, autoYzoom);

                            // Определяем насколько точек больше чем на экране
                            var N = data.X.Length;
                            float ww = curPlot.x2 - curPlot.x1;
                            int step = 1;

                            if (N / ww > 2)
                            {
                                step = (int)Math.Floor(N / ww);
                            }

                            // расстояние между тиками по X
                            float tickGap = 5; //sec

                            var xScale = (curPlot.x2 - curPlot.x1) / (curPlot.xMax - curPlot.xMin);
                            var xSpacing = 80 / xScale;

                            tickGap = OptimalSpacing(xSpacing);

                            float xStart = (float)Math.Ceiling(curPlot.xMin / tickGap) * tickGap;
                            float xEnd = (float)Math.Floor(curPlot.xMax / tickGap) * tickGap;

                            labelTextFormat.TextAlignment = TextAlignment.Center;
                            labelTextFormat.ParagraphAlignment = ParagraphAlignment.Near;

                            // рисуем тики X
                            for (float k = xStart - tickGap; k < xEnd + tickGap; k = k + tickGap)
                            {

                                RawVector2 tickPoint = PointToCanvas(curPlot, k, 0);
                                RawVector2 point1 = new RawVector2(tickPoint.X, curPlot.y2 - 2);
                                RawVector2 point2 = new RawVector2(tickPoint.X, curPlot.y1 + 2);

                                target.DrawLine(point1, point2, helperBrush);

                                drawText(k.ToString("F2"), ref labelTextFormat, ref brushblack, ref target, tickPoint.X, curPlot.y2 + 2);
                            }

                            //  рисуем тики для Y 
                            labelTextFormat.TextAlignment = TextAlignment.Leading;
                            labelTextFormat.ParagraphAlignment = ParagraphAlignment.Center;

                            for (var k = -20; k < 25; k = k + 10)
                            {
                                RawVector2 tickPoint = PointToCanvas(curPlot, data.X[N - 1], k);
                                RawVector2 point1 = new RawVector2(curPlot.x1 + 1, tickPoint.Y);
                                RawVector2 point2 = new RawVector2(curPlot.x2 + 1, tickPoint.Y);

                                target.DrawLine(point1, point2, transparentBrush);

                                drawText(k.ToString("F2"), ref labelTextFormat, ref transparentBrush, ref target, point2.X + 3, point2.Y);
                            }

                            labelTextFormat.TextAlignment = TextAlignment.Leading;
                            labelTextFormat.ParagraphAlignment = ParagraphAlignment.Center;

                            // рисуем значения yMax и yMin
                            drawText(curPlot.yMax.ToString("F2"), ref labelTextFormat, ref brushblack, ref target, curPlot.x2 + 4, curPlot.y1 + 6);
                            drawText(curPlot.yMin.ToString("F2"), ref labelTextFormat, ref brushblack, ref target, curPlot.x2 + 4, curPlot.y2 - 6);

                            // рисуем текущее значение
                            labelTextFormat.TextAlignment = TextAlignment.Leading;
                            labelTextFormat.ParagraphAlignment = ParagraphAlignment.Near;
                            drawText(data.Y[N - 1].ToString("F3"), ref labelTextFormat, ref brushblack, ref target, curPlot.x1 + 4, curPlot.y1 + 4);

                            // Рисуем сам график
                            for (int jjj = 0; jjj < N - step; jjj = jjj + step)
                            {
                                RawVector2 stPoint = PointToCanvas(curPlot, data.X[jjj], data.Y[jjj]);
                                RawVector2 endPoint = PointToCanvas(curPlot, data.X[jjj + step], data.Y[jjj + step]);
                                target.DrawLine(stPoint, endPoint, blueBrush);

                                // РИСУЕМ КУРСОР 

                                if (_cursor > 0 && _cursor == Math.Round(stPoint.X))
                                {
                                    RawVector2 stPoint2 = new RawVector2(_cursor, curPlot.y1 + 1);
                                    RawVector2 endPoint2 = new RawVector2(_cursor, curPlot.y2 - 1);
                                    target.DrawLine(stPoint2, endPoint2, brushblack);
                                }
                            }

                            autoYzoom = false;

                        }

                        i++;
                    }

                }

                DoRedraw = false;
            }

            //labelTextFormat.Dispose();

        }
    }
}