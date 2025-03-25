using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace WinFormsApp3
{
    public partial class Form1 : Form
    {
        private Bitmap triangleBitmap;
        private Bitmap userShapeBitmap;
        private bool isDrawing = false;
        private Point lastPoint;
        private Color currentColor = Color.Black;
        private int penWidth = 2;

        public Form1()
        {
            InitializeComponent();
            InitializeBitmaps();
            SetupEventHandlers();
        }

        private void InitializeBitmaps()
        {
            triangleBitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            userShapeBitmap = new Bitmap(pictureBox2.Width, pictureBox2.Height);

            ClearBitmap(triangleBitmap, Color.White);
            ClearBitmap(userShapeBitmap, Color.White);

            pictureBox1.Image = triangleBitmap;
            pictureBox2.Image = userShapeBitmap;
        }

        private void ClearBitmap(Bitmap bitmap, Color color)
        {
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(color);
            }
        }

        private void SetupEventHandlers()
        {
            pictureBox2.MouseDown += PictureBox2_MouseDown;
            pictureBox2.MouseMove += PictureBox2_MouseMove;
            pictureBox2.MouseUp += PictureBox2_MouseUp;
            pictureBox2.MouseLeave += PictureBox2_MouseLeave;

            button1.Click += Button1_Click;
            button3.Click += Button3_Click;
        }

        private void DrawTriangle()
        {
            using (Graphics g = Graphics.FromImage(triangleBitmap))
            {
                g.Clear(Color.White);

                Point[] points = {
                    new Point(10, 70),
                    new Point(50, 160),
                    new Point(70, 80)
                };

                g.DrawPolygon(new Pen(currentColor, penWidth), points);
                FillTriangle(points[0], points[1], points[2], Color.Red);
            }
            pictureBox1.Refresh();
        }

        private void FillTriangle(Point t0, Point t1, Point t2, Color color)
        {
            if (t0.Y > t1.Y) Swap(ref t0, ref t1);
            if (t0.Y > t2.Y) Swap(ref t0, ref t2);
            if (t1.Y > t2.Y) Swap(ref t1, ref t2);

            int totalHeight = t2.Y - t0.Y;

            for (int i = 0; i < totalHeight; i++)
            {
                bool isSecondHalf = i > t1.Y - t0.Y || t1.Y == t0.Y;
                int segmentHeight = isSecondHalf ? t2.Y - t1.Y : t1.Y - t0.Y;

                float alpha = (float)i / totalHeight;
                float beta = (float)(i - (isSecondHalf ? t1.Y - t0.Y : 0)) / segmentHeight;

                Point A = Lerp(t0, t2, alpha);
                Point B = isSecondHalf ? Lerp(t1, t2, beta) : Lerp(t0, t1, beta);

                if (A.X > B.X) Swap(ref A, ref B);

                for (int j = A.X; j <= B.X; j++)
                {
                    if (j >= 0 && j < triangleBitmap.Width && t0.Y + i >= 0 && t0.Y + i < triangleBitmap.Height)
                    {
                        triangleBitmap.SetPixel(j, t0.Y + i, color);
                    }
                }
            }
        }

        private Point Lerp(Point a, Point b, float t) => new Point(
            (int)(a.X + (b.X - a.X) * t),
            (int)(a.Y + (b.Y - a.Y) * t));

        private void Swap(ref Point a, ref Point b) => (a, b) = (b, a);

        private void PictureBox2_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDrawing = true;
                lastPoint = e.Location;
            }
        }

        private void PictureBox2_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDrawing && e.Button == MouseButtons.Left)
            {
                using (Graphics g = Graphics.FromImage(userShapeBitmap))
                {
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
                    g.DrawLine(new Pen(currentColor, penWidth), lastPoint, e.Location);
                }
                lastPoint = e.Location;
                pictureBox2.Refresh();
            }
        }

        private void PictureBox2_MouseUp(object sender, MouseEventArgs e) => isDrawing = false;
        private void PictureBox2_MouseLeave(object sender, EventArgs e) => isDrawing = false;

        private void FillUserShape(Color fillColor)
        {
            BitmapData bmpData = userShapeBitmap.LockBits(
                new Rectangle(0, 0, userShapeBitmap.Width, userShapeBitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadWrite,
                userShapeBitmap.PixelFormat);

            int bytes = Math.Abs(bmpData.Stride) * userShapeBitmap.Height;
            byte[] rgbValues = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, rgbValues, 0, bytes);

            for (int y = 0; y < userShapeBitmap.Height; y++)
            {
                bool inside = false;
                bool border = false;
                int k = 0;
                int startX = 0;

                for (int x = 0; x < userShapeBitmap.Width; x++)
                {
                    int index = (y * bmpData.Stride) + (x * 4);

                    if (rgbValues[index + 3] == 255 &&
                        (rgbValues[index] != 255 || rgbValues[index + 1] != 255 || rgbValues[index + 2] != 255))
                    {
                        if (!border)
                        {
                            border = true;
                            k++;
                        }

                        if (inside)
                        {
                            inside = false;
                            for (int i = startX + 1; i < x; i++)
                            {
                                int fillIndex = (y * bmpData.Stride) + (i * 4);
                                rgbValues[fillIndex] = fillColor.B;
                                rgbValues[fillIndex + 1] = fillColor.G;
                                rgbValues[fillIndex + 2] = fillColor.R;
                                rgbValues[fillIndex + 3] = fillColor.A;
                            }
                        }
                    }
                    else
                    {
                        if (border)
                        {
                            if (k % 2 != 0)
                            {
                                inside = true;
                                startX = x;
                            }
                            border = false;
                        }
                    }
                }
            }

            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, bmpData.Scan0, bytes);
            userShapeBitmap.UnlockBits(bmpData);
            pictureBox2.Refresh();
        }

        private void Button1_Click(object sender, EventArgs e) => DrawTriangle();
        private void Button3_Click(object sender, EventArgs e) => FillUserShape(Color.Blue);
    }
}