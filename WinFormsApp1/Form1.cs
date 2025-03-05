using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace WinFormsApp3
{
    public partial class Form1 : Form
    {
        private Bitmap triangleBitmap; // Bitmap ��� ������������
        private Bitmap userShapeBitmap; // Bitmap ��� ������ ������������
        private bool isDrawing = false; // ���� ��� ������������ ���������
        private Point lastPoint; // ��������� ����� ���������

        public Form1()
        {
            InitializeComponent();

            // ������������� Bitmap ��� PictureBox
            triangleBitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            userShapeBitmap = new Bitmap(pictureBox2.Width, pictureBox2.Height);

            // ������� Bitmap ����� ������
            using (Graphics g = Graphics.FromImage(triangleBitmap))
            {
                g.Clear(Color.White);
            }
            using (Graphics g = Graphics.FromImage(userShapeBitmap))
            {
                g.Clear(Color.White);
            }

            pictureBox1.Image = triangleBitmap;
            pictureBox2.Image = userShapeBitmap;

            // ����������� ������� ����
            pictureBox2.MouseDown += pictureBox2_MouseDown;
            pictureBox2.MouseMove += pictureBox2_MouseMove;
            pictureBox2.MouseUp += pictureBox2_MouseUp;
        }

        // ����� ��� ��������� ������������
        private void DrawTriangle()
        {
            using (Graphics g = Graphics.FromImage(triangleBitmap))
            {
                // ������� PictureBox
                g.Clear(Color.White);

                // ����������� ������ ������������
                Point t0 = new Point(10, 70);
                Point t1 = new Point(50, 160);
                Point t2 = new Point(70, 80);

                // ��������� ������������
                Point[] points = { t0, t1, t2 };
                g.DrawPolygon(Pens.Black, points);

                // ������� ������������
                FillTriangle(t0, t1, t2, Color.Red);
            }
            pictureBox1.Refresh(); // ���������� PictureBox
        }

        private void FillTriangle(Point t0, Point t1, Point t2, Color color)
        {
            // ���������� ������ �� Y (t0 -> �������, t2 -> ������)
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
                    triangleBitmap.SetPixel(j, t0.Y + i, color);
                }
            }
        }

        // ��������������� ����� ��� �������� ������������
        private Point Lerp(Point a, Point b, float t)
        {
            return new Point(
                (int)(a.X + (b.X - a.X) * t),
                (int)(a.Y + (b.Y - a.Y) * t)
            );
        }

        // ��������������� ����� ��� ������ ��������
        private void Swap(ref Point a, ref Point b)
        {
            Point temp = a;
            a = b;
            b = temp;
        }

        // ���������� ������� MouseDown ��� pictureBox2
        private void pictureBox2_MouseDown(object sender, MouseEventArgs e)
        {
            isDrawing = true;
            lastPoint = e.Location;
        }

        // ���������� ������� MouseMove ��� pictureBox2
        private void pictureBox2_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDrawing)
            {
                using (Graphics g = Graphics.FromImage(userShapeBitmap))
                {
                    g.DrawLine(Pens.Black, lastPoint, e.Location);
                }
                lastPoint = e.Location;
                pictureBox2.Refresh();
            }
        }

        // ���������� ������� MouseUp ��� pictureBox2
        private void pictureBox2_MouseUp(object sender, MouseEventArgs e)
        {
            isDrawing = false;
        }

        // ����� ��� ������� ������ ������������
        private void FillUserShape(Color fillColor)
        {
            // ������� ��������� ����� ��� ������� (��������, ����� PictureBox)
            Point startPoint = new Point(pictureBox2.Width / 2, pictureBox2.Height / 2);

            // ���������, ��� ��������� ����� �� ��������� �� �������
            if (userShapeBitmap.GetPixel(startPoint.X, startPoint.Y).ToArgb() == Color.Black.ToArgb())
            {
                MessageBox.Show("��������� ����� ��������� �� ������� ������. ���������� ������ �����.");
                return;
            }

            // ��������� �������
            FloodFill(startPoint, fillColor);
            pictureBox2.Refresh();
        }

        // �������� ������� (Flood Fill)
        private void FloodFill(Point startPoint, Color fillColor)
        {
            Color targetColor = userShapeBitmap.GetPixel(startPoint.X, startPoint.Y);
            if (targetColor.ToArgb() == fillColor.ToArgb()) return;

            Stack<Point> pixels = new Stack<Point>();
            pixels.Push(startPoint);

            while (pixels.Count > 0)
            {
                Point current = pixels.Pop();
                if (current.X < 0 || current.X >= userShapeBitmap.Width ||
                    current.Y < 0 || current.Y >= userShapeBitmap.Height)
                    continue;

                Color currentColor = userShapeBitmap.GetPixel(current.X, current.Y);
                if (currentColor.ToArgb() == targetColor.ToArgb())
                {
                    userShapeBitmap.SetPixel(current.X, current.Y, fillColor);

                    pixels.Push(new Point(current.X - 1, current.Y)); // �����
                    pixels.Push(new Point(current.X + 1, current.Y)); // ������
                    pixels.Push(new Point(current.X, current.Y - 1)); // �����
                    pixels.Push(new Point(current.X, current.Y + 1)); // ����
                }
            }
        }

        // ���������� ������ button1 (��������� ������������)
        private void button1_Click(object sender, EventArgs e)
        {
            DrawTriangle();
        }

        // ���������� ������ button3 (������� ������ ������������)
        private void button3_Click(object sender, EventArgs e)
        {
            FillUserShape(Color.Blue); // �������� ������ ����� ������
        }
    }
}