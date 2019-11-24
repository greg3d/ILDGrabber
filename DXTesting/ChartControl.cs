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
    }
    class ChartControl : G2dControl
    {



        //private Thread thread;

        float xx = 0.5f;
        float yy = 0.5f;

        float marginLeft = 5;
        float marginBottom = 12;
        float marginTop = 5;
        float marginRight = 40;

        float plotHeight = 100;

        //float xMin = 0f;
        float xMax = 300;
        float yMax = -1e9f;
        float yMin = 1e9f;

        float nPoints = 5000;

        //int numberOfPlots = 1;


        //int Mode = 0;

        //public RawVector2[] pt1 = new RawVector2[nPoub];

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
                
            if ( x < curPlot.xMin || x > curPlot.xMax || y < curPlot.yMin || y > curPlot.yMax)
            {
                x = Single.NaN;
                y = Single.NaN;
            }

            retVec.X = curPlot.x1 + (x - curPlot.xMin) * (curPlot.x2 - curPlot.x1) / (curPlot.xMax - curPlot.xMin);
            retVec.Y = curPlot.y2 - (y - curPlot.yMin) * (curPlot.y2 - curPlot.y1) / (curPlot.yMax - curPlot.yMin);
            
            return retVec;
        }

        //private void Redraw()

        public override void Render(RenderTarget target)
        {
            //RenderWait = 2;
            target.Clear(new RawColor4(1.0f, 1.0f, 1.0f, 1.0f));

            plotHeight = (float)ActualHeight / 8;

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
                    target.DrawRectangle(PlotArea, helperBrush, 1.0f);

                    Connection con = conz.cons[i];

                    if (con.IsGrabbing)
                    {

                        //Trace.Write('d');

                        //FileStream fs = new FileStream("D:\\file.txt",FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                        // Определяем YMax и YMin 
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

                        // определяем xMax и xMin
                        curPlot.xMax = con.vdata.X[(int)nPoints - 1];
                        curPlot.xMin = con.vdata.X[0];


                        // расстояние между тиками по X = 5 сек
                        int tickGap = 5; //sec
                        var tnum = xMax / tickGap;

                        labelTextFormat.TextAlignment = TextAlignment.Center;
                        labelTextFormat.ParagraphAlignment = ParagraphAlignment.Near;

                        // рисуем тики
                        for (var k = curPlot.xMin + tickGap; k < curPlot.xMax; k = k + tickGap)
                        {

                            RawVector2 tickPoint = PointToCanvas(curPlot, k, 0);
                            RawVector2 point1 = new RawVector2(tickPoint.X, curPlot.y2 - 2);
                            RawVector2 point2 = new RawVector2(tickPoint.X, curPlot.y1 + 2);

                            target.DrawLine(point1, point2, helperBrush);

                            drawText(k.ToString("F2"), ref labelTextFormat, ref brushblack, ref target, tickPoint.X, curPlot.y2 + 2);
                        }

                        // крайний нижний тик справа
                        //drawText(curPlot.xMax.ToString("{0:C}"), ref labelTextFormat, ref brushblack, ref target, curPlot.x2, curPlot.y2 + 2);

                        /*
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


                        //  рисуем тики для Y 
                        labelTextFormat.TextAlignment = TextAlignment.Leading;
                        labelTextFormat.ParagraphAlignment = ParagraphAlignment.Center;

                        for (var k = minYTick + realYTickGap; k < curPlot.yMax; k = k + realYTickGap)
                        {

                            RawVector2 tickPoint = PointToCanvas(curPlot, con.vdata.Y[(int)nPoints-1], (float)k);
                            RawVector2 point1 = new RawVector2(curPlot.x1 + 1, tickPoint.Y);
                            RawVector2 point2 = new RawVector2(curPlot.x2 + 1, tickPoint.Y);

                            target.DrawLine(point1, point2, helperBrush);

                            drawText(k.ToString("F2"), ref labelTextFormat, ref brushblack, ref target, curPlot.x2, curPlot.y2 + 2);
                        }*/
                        //  рисуем тики для Y 
                        labelTextFormat.TextAlignment = TextAlignment.Leading;
                        labelTextFormat.ParagraphAlignment = ParagraphAlignment.Center;

                        for (var k = -20; k < 25; k = k + 10)
                        {
                            RawVector2 tickPoint = PointToCanvas(curPlot, con.vdata.X[(int)nPoints - 1], (float)k);
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

                    if (con.IsPostProc)
                    {

                        var data = con.rdata;

                        var N = data.X.Length;

                        foreach (var val in data.Y)
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
                        curPlot.xMax = data.X[N - 1];
                        curPlot.xMin = data.X[0];


                        // расстояние между тиками по X = 5 сек
                        int tickGap = 5; //sec
                        var tnum = xMax / tickGap;

                        labelTextFormat.TextAlignment = TextAlignment.Center;
                        labelTextFormat.ParagraphAlignment = ParagraphAlignment.Near;

                        // рисуем тики
                        for (var k = curPlot.xMin + tickGap; k < curPlot.xMax; k = k + tickGap)
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
                        float ww = curPlot.x2 - curPlot.x1;
                        int step = 1;

                        if (N / ww > 2)
                        {
                            step = (int)Math.Floor(N / ww);
                        }

                        for (int jjj = 0; jjj < N - step; jjj = jjj + step)
                        {
                            RawVector2 stPoint = PointToCanvas(curPlot, data.X[jjj], data.Y[jjj]);
                            RawVector2 endPoint = PointToCanvas(curPlot, data.X[jjj + step], data.Y[jjj + step]);
                            target.DrawLine(stPoint, endPoint, blueBrush);
                        }
                    }

                }
            }

            labelTextFormat.Dispose();

        }
    }
}