using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AmbilightServer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        [DllImport("user32.dll")]
        static extern bool GetCursorPos(ref Point lpPoint);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int BitBlt(IntPtr hDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);

        Bitmap screenPixel = new Bitmap(1, 1, PixelFormat.Format32bppArgb);
        public Color GetColorAt(Point location)
        {
            using (Graphics gdest = Graphics.FromImage(screenPixel))
            {
                using (Graphics gsrc = Graphics.FromHwnd(IntPtr.Zero))
                {
                    IntPtr hSrcDC = gsrc.GetHdc();
                    IntPtr hDC = gdest.GetHdc();
                    int retval = BitBlt(hDC, 0, 0, 1, 1, hSrcDC, location.X, location.Y, (int)CopyPixelOperation.SourceCopy);
                    gdest.ReleaseHdc();
                    gsrc.ReleaseHdc();
                }
            }

            return screenPixel.GetPixel(0, 0);
        }

        Bitmap Screenshot()
        {
            var bmpScreenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
                                           Screen.PrimaryScreen.Bounds.Height,
                                           PixelFormat.Format32bppArgb);

            var gfxScreenshot = Graphics.FromImage(bmpScreenshot);
            
            gfxScreenshot.CopyFromScreen(Screen.PrimaryScreen.Bounds.X,
                                        Screen.PrimaryScreen.Bounds.Y,
                                        0,
                                        0,
                                        Screen.PrimaryScreen.Bounds.Size,
                                        CopyPixelOperation.SourceCopy);

            return bmpScreenshot;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            var screenSize = Screen.PrimaryScreen.Bounds.Size;

            var imgarray = new Image[9];
            var img = Screenshot();
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    var index = i * 3 + j;
                    imgarray[index] = new Bitmap(screenSize.Width / 3, screenSize.Height / 3);
                    var graphics = Graphics.FromImage(imgarray[index]);
                    graphics.DrawImage(img, new Rectangle(0, 0, screenSize.Width / 3, screenSize.Height / 3), new Rectangle(i * screenSize.Width / 3, j * screenSize.Height / 3, screenSize.Width / 3, screenSize.Height / 3), GraphicsUnit.Pixel);
                    graphics.Dispose();
                }
            }

            var dominantColors = new List<Color>();
            foreach (Image screenPart in imgarray)
            {
                dominantColors.Add(getDominantColor((Bitmap)screenPart));
            }

            pictureBox1.BackColor = dominantColors[0];
            pictureBox2.BackColor = dominantColors[1];
            pictureBox3.BackColor = dominantColors[2];
            pictureBox4.BackColor = dominantColors[3];
            pictureBox5.BackColor = dominantColors[4];
            pictureBox6.BackColor = dominantColors[5];
            pictureBox7.BackColor = dominantColors[6];
            pictureBox8.BackColor = dominantColors[7];
            pictureBox9.BackColor = dominantColors[8];

            //WebClient wc = new WebClient();
            //wc.DownloadString("192.168.1.38/r.php?c=");

            //var leftTop = new Point(screenSize.Width / 8, screenSize.Height / 6);
            //pictureBox1.BackColor = GetColorAt(leftTop);
            //var midTop = new Point(screenSize.Width / 2, screenSize.Height / 6);
            //pictureBox4.BackColor = GetColorAt(midTop);
            //var rightTop = new Point(screenSize.Width - screenSize.Width / 8, screenSize.Height / 6);
            //pictureBox7.BackColor = GetColorAt(rightTop);

            //var leftMid = new Point(screenSize.Width / 8, screenSize.Height / 2);
            //pictureBox2.BackColor = GetColorAt(leftMid);
            //var midMid = new Point(screenSize.Width / 2, screenSize.Height / 2);
            //pictureBox5.BackColor = GetColorAt(midMid);
            //var rightMid = new Point(screenSize.Width - screenSize.Width / 8, screenSize.Height / 2);
            //pictureBox8.BackColor = GetColorAt(rightMid);

            //var leftBottom = new Point(screenSize.Width / 8, screenSize.Height -  screenSize.Height / 6);
            //pictureBox3.BackColor = GetColorAt(leftBottom);
            //var midBottom = new Point(screenSize.Width / 2, screenSize.Height -  screenSize.Height / 6);
            //pictureBox6.BackColor = GetColorAt(midBottom);
            //var rightBottom = new Point(screenSize.Width - screenSize.Width / 8, screenSize.Height - screenSize.Height / 6);
            //pictureBox9.BackColor = GetColorAt(rightBottom);
        }

        public static Color getDominantColor(Bitmap bmp)
        {
            BitmapData srcData = bmp.LockBits(
            new Rectangle(0, 0, bmp.Width, bmp.Height),
            ImageLockMode.ReadOnly,
            PixelFormat.Format32bppArgb);

            int stride = srcData.Stride;

            IntPtr Scan0 = srcData.Scan0;

            long[] totals = new long[] { 0, 0, 0 };

            int width = bmp.Width;
            int height = bmp.Height;

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        for (int color = 0; color < 3; color++)
                        {
                            int idx = (y * stride) + x * 4 + color;

                            totals[color] += p[idx];
                        }
                    }
                }
            }

            int avgB = (int) totals[0] / (width * height);
            int avgG = (int)totals[1] / (width * height);
            int avgR = (int)totals[2] / (width * height);
            bmp.UnlockBits(srcData);
            bmp.Dispose();
            GC.Collect();
            return Color.FromArgb(avgR, avgG, avgB);

            //Bitmap bmp2 = new Bitmap(1, 1);
            //using (Graphics g = Graphics.FromImage(bmp2))
            //{
            //    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            //    g.DrawImage(bmp, new Rectangle(0, 0, 1, 1));
            //    g.Dispose();
            //}
            //Color pixel = bmp2.GetPixel(0, 0);
            //bmp2.Dispose();
            //return pixel;

            ////Used for tally
            //int r = 0;
            //int g = 0;
            //int b = 0;

            //int total = 0;

            //for (int x = 0; x < bmp.Width; x++)
            //{
            //    for (int y = 0; y < bmp.Height; y++)
            //    {
            //        Color clr = bmp.GetPixel(x, y);

            //        r += clr.R;
            //        g += clr.G;
            //        b += clr.B;

            //        total++;
            //    }
            //}

            ////Calculate average
            //r /= total;
            //g /= total;
            //b /= total;

            //return Color.FromArgb(r, g, b);
        }
    }
}
