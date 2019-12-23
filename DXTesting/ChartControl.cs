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

        private TextFormat labelTextFormat;
        private TextFormat headerTextFormat;

        // float nPoints = 5000;

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
            resCache.Add("BrushBlack",      t => new SolidColorBrush(t, new RawColor4(0f, 0f, 0f, 1f)));
            resCache.Add("BrushData",       t => new SolidColorBrush(t, new RawColor4(0f, 0.0f, 1f, 1f)));
            resCache.Add("BrushTickX",      t => new SolidColorBrush(t, new RawColor4(1f, 1f, 1f, 1f)));
            resCache.Add("BrushTickY",      t => new SolidColorBrush(t, new RawColor4(0.2f, 0.2f, 0.2f, 0.2f)));
            resCache.Add("BrushPlotBG",     t => new SolidColorBrush(t, new RawColor4(0.95f, 0.95f, 0.9f, 1f)));
            resCache.Add("BrushPlotStroke", t => new SolidColorBrush(t, new RawColor4(0.8f, 0.8f, 0.8f, 1f)));

            labelTextFormat = new TextFormat(new SharpDX.DirectWrite.Factory(), "Arial", 10)
            {
                ParagraphAlignment = ParagraphAlignment.Far,
                TextAlignment = TextAlignment.Center
            };

            headerTextFormat = new TextFormat(new SharpDX.DirectWrite.Factory(), "Arial", FontWeight.Bold, FontStyle.Normal, 12)
            {
                ParagraphAlignment = ParagraphAlignment.Far,
                TextAlignment = TextAlignment.Center
            };
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

        private RawVector2 PointToCanvasN(Plot curPlot, float x, float y)
        {
            RawVector2 retVec = new RawVector2();


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

        public void DrawCursor(int x)
        {
            if (_cursor != x)
            {
                _cursor = x;
            }

        }

        public override void Render(RenderTarget target)
        {
            target.AntialiasMode = AntialiasMode.PerPrimitive;

            // brushes
            Brush brushBlack = resCache["BrushBlack"] as Brush;
            Brush brushData  = resCache["BrushData"] as Brush;
            Brush brushTickX = resCache["BrushTickX"] as Brush;
            Brush brushTickY = resCache["BrushTickY"] as Brush;
            Brush brushPlotBG = resCache["BrushPlotBG"] as Brush;
            Brush brushPlotStroke = resCache["BrushPlotStroke"] as Brush;

            Connectionz conz = Connectionz.getInstance();

            if (conz.ReadyCount > 0 && DoRedraw)
            {
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

                // Очистка всего поля
                target.Clear(new RawColor4(1.0f, 1.0f, 1.0f, 1.0f));
                RawRectangleF clearRect = new RawRectangleF(0, 0, (float)ActualWidth, (float)ActualHeight);
                target.FillRectangle(clearRect, new SolidColorBrush(target, new RawColor4(1f, 1f, 1f, 1f)));

                // высота области графика
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
                        
                        // рисуем поле графика
                        RawRectangleF PlotArea = new RawRectangleF(curPlot.x1, curPlot.y1, curPlot.x2, curPlot.y2);
                        target.FillRectangle(PlotArea, brushPlotBG);
                        target.DrawRectangle(PlotArea, brushPlotStroke, 1f);

                        float tickGapX = 5f; //sec

                        float tickGapY = 10f; //mm
                        
                        // данные для отображения
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

                        // автомасштаб
                        DoAutoScale(ref data.X, ref data.Y, ref curPlot, autoYzoom);

                        // Определяем насколько точек больше чем на экране
                        var N = data.X.Length;
                        float ww = curPlot.x2 - curPlot.x1;
                        int step = 1;

                        if (N / ww > 2)
                        {
                            step = (int)Math.Floor(N / ww);
                        }

                        // начальный и конечные тики
                        float xStart = (float)Math.Ceiling(curPlot.xMin / tickGapX) * tickGapX;
                        float xEnd = (float)Math.Floor(curPlot.xMax / tickGapX) * tickGapX;

                        float yStart = (float)Math.Ceiling(curPlot.yMin / tickGapY) * tickGapY;
                        float yEnd = (float)Math.Floor(curPlot.yMax / tickGapY) * tickGapY;

                        // формат текста
                        labelTextFormat.TextAlignment = TextAlignment.Center;
                        labelTextFormat.ParagraphAlignment = ParagraphAlignment.Near;

                        // рисуем тики X
                        for (var k = xStart - tickGapX; k < xEnd + tickGapX; k = k + tickGapX)
                        {
                            RawVector2 tickPoint = PointToCanvasN(curPlot, k, 0);
                            RawVector2 point1 = new RawVector2(tickPoint.X, curPlot.y2 - 2);
                            RawVector2 point2 = new RawVector2(tickPoint.X, curPlot.y1 + 2);

                            if ((tickPoint.X > curPlot.x1) & (tickPoint.X < curPlot.x2))
                            {
                                target.DrawLine(point1, point2, brushTickX);
                                drawText(k.ToString("F2"), ref labelTextFormat, ref brushBlack, ref target, tickPoint.X, curPlot.y2 + 2);
                            }
                        }

                        // формат
                        labelTextFormat.TextAlignment = TextAlignment.Leading;
                        labelTextFormat.ParagraphAlignment = ParagraphAlignment.Center;

                        //  рисуем тики для Y 
                        for (var k = yStart - tickGapY; k < yEnd + tickGapY; k = k + tickGapY)
                        {
                            RawVector2 tickPoint = PointToCanvasN(curPlot, data.X[(int)N - 1], k);
                            RawVector2 point1 = new RawVector2(curPlot.x1 + 1, tickPoint.Y);
                            RawVector2 point2 = new RawVector2(curPlot.x2 + 1, tickPoint.Y);

                            if ((tickPoint.Y > curPlot.y1) & (tickPoint.Y < curPlot.y2))
                            {
                                target.DrawLine(point1, point2, brushTickY);
                                drawText(k.ToString("F2"), ref labelTextFormat, ref brushTickY, ref target, point2.X + 3, point2.Y);
                            }
                        }

                        //
                        labelTextFormat.TextAlignment = TextAlignment.Leading;
                        labelTextFormat.ParagraphAlignment = ParagraphAlignment.Center;

                        // рисуем значения yMax и yMin
                        drawText(curPlot.yMax.ToString("F2"), ref labelTextFormat, ref brushBlack, ref target, curPlot.x2 + 4, curPlot.y1 + 6);
                        drawText(curPlot.yMin.ToString("F2"), ref labelTextFormat, ref brushBlack, ref target, curPlot.x2 + 4, curPlot.y2 - 6);

                        float cursorVal = 0;

                        // Рисуем сам график
                        for (int jjj = 0; jjj < N - step; jjj = jjj + step)
                        {
                            RawVector2 stPoint = PointToCanvas(curPlot, data.X[jjj], data.Y[jjj]);
                            RawVector2 endPoint = PointToCanvas(curPlot, data.X[jjj + step], data.Y[jjj + step]);
                            target.DrawLine(stPoint, endPoint, brushData);

                            // РИСУЕМ КУРСОР 
                            if (_cursor > 0 && _cursor == Math.Round(stPoint.X))
                            {
                                RawVector2 stPoint2 = new RawVector2(_cursor, curPlot.y1 + 1);
                                RawVector2 endPoint2 = new RawVector2(_cursor, curPlot.y2 - 1);
                                target.DrawLine(stPoint2, endPoint2, brushBlack);
                                cursorVal = data.Y[jjj];
                            }
                        }


                        //текущее значение
                        string curVal;
                        if (IsGrabbing)
                        {
                            curVal = data.Y[N - 1].ToString("F3");
                        }
                        else
                        {
                            curVal = cursorVal.ToString("F3");
                        }

                        headerTextFormat.TextAlignment = TextAlignment.Trailing;
                        headerTextFormat.ParagraphAlignment = ParagraphAlignment.Near;

                        // рисуем его
                        drawText(curVal, ref headerTextFormat, ref brushBlack, ref target, curPlot.x2 - 4, curPlot.y1 + 2);

                        // рисуем вспомогательное 
                        labelTextFormat.TextAlignment = TextAlignment.Leading;
                        headerTextFormat.TextAlignment = TextAlignment.Leading;
                        labelTextFormat.ParagraphAlignment = ParagraphAlignment.Near;
                        drawText((con.ConnID + 1).ToString(), ref headerTextFormat, ref brushBlack, ref target, curPlot.x1 + 4, curPlot.y1 + 2);
                        drawText(con.Serial, ref labelTextFormat, ref brushBlack, ref target, curPlot.x1 + 12, curPlot.y1 + 4);

                        i++;
                    }

                }

                if (IsPostProc)
                {
                    autoYzoom = false;
                }

                DoRedraw = false;
            }

            //labelTextFormat.Dispose();

        }
    }
}