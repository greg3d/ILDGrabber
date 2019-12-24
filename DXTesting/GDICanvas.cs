using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;

namespace DXTesting
{
    partial class GDICanvas
    {
        protected int ActualWidth;
        protected int ActualHeight;
        private Panel cont;
        private System.Windows.Controls.Image img;

        private Bitmap gdiBitmap;
        protected Graphics graphics;

        public InteropBitmap interopBitmap { get; private set; }

        const uint FILE_MAP_ALL_ACCESS = 0xF001F;
        const uint PAGE_READWRITE = 0x04;

        private int bpp = PixelFormats.Bgr32.BitsPerPixel / 8;

        //private System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();

        public Dictionary<string, SolidBrush> myBrushes = new Dictionary<string, SolidBrush>()
        {
            { "brushBlack" , new SolidBrush(System.Drawing.Color.Black) },
            { "brushRed" , new SolidBrush(System.Drawing.Color.Red) }
        };

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr CreateFileMapping(IntPtr hFile,
        IntPtr lpFileMappingAttributes,
        uint flProtect,
        uint dwMaximumSizeHigh,
        uint dwMaximumSizeLow,
        string lpName);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr MapViewOfFile(IntPtr hFileMappingObject,
        uint dwDesiredAccess,
        uint dwFileOffsetHigh,
        uint dwFileOffsetLow,
        uint dwNumberOfBytesToMap);

        public GDICanvas(Panel c, System.Windows.Controls.Image image)
        {
            cont = c;
            img = image;
            createBitmaps();
        }

        public void createBitmaps()
        {

            ActualWidth = (int)cont.ActualWidth;
            ActualHeight = (int)cont.ActualHeight;

            uint byteCount = (uint)(ActualWidth * ActualHeight * bpp);

            //Allocate/reserve memory to write to

            var sectionPointer = CreateFileMapping(new IntPtr(-1), IntPtr.Zero, PAGE_READWRITE, 0, byteCount, null);

            var mapPointer = MapViewOfFile(sectionPointer, FILE_MAP_ALL_ACCESS, 0, 0, byteCount);

            var format = PixelFormats.Bgr32;

            //create the InteropBitmap

            interopBitmap = Imaging.CreateBitmapSourceFromMemorySection(sectionPointer, ActualWidth, ActualHeight, format,
                (ActualWidth * format.BitsPerPixel / 8), 0) as InteropBitmap;


            //create the GDI Bitmap

            gdiBitmap = new System.Drawing.Bitmap(
                ActualWidth,
                ActualHeight,
                ActualWidth * bpp,
                System.Drawing.Imaging.PixelFormat.Format32bppPArgb,
                mapPointer
               );

            // Get good old GDI Graphics

            graphics = Graphics.FromImage(gdiBitmap);

            img.Source = interopBitmap;
        }

        protected virtual void draw()
        {

            graphics.Clear(System.Drawing.Color.White);

            interopBitmap.Invalidate();
        }

        public void Redraw()
        {
            //createBitmaps((int)cont.ActualWidth, (int)cont.ActualHeight);
            draw();
        }
    }
}
