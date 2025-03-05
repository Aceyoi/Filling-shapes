using System.Drawing;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace WinFormsApp3
{
    public partial class Form1 : Form
    {
        private Bitmap triangleBitmap; // Bitmap для треугольника
        private Bitmap userShapeBitmap; // Bitmap для фигуры пользователя
        private bool isDrawing = false; // Флаг для отслеживания рисования
        private Point lastPoint; // Последняя точка рисования
        public Form1()
        {
            InitializeComponent();
            // Инициализация Bitmap для PictureBox
            triangleBitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            userShapeBitmap = new Bitmap(pictureBox2.Width, pictureBox2.Height);
            // Привязка Bitmap к PictureBox
            pictureBox1.Image = triangleBitmap;
            pictureBox2.Image = userShapeBitmap;
        }

        // Метод для рисования треугольника
        private void DrawTriangle()
        {
            using (Graphics g = Graphics.FromImage(triangleBitmap))
            {
                // Определение вершин треугольника
                Point t0 = new Point(10, 70);
                Point t1 = new Point(50, 160);
                Point t2 = new Point(70, 80);

                // Рисование треугольника
                Point[] points = { t0, t1, t2 };
                g.DrawPolygon(Pens.Black, points);

                // Заливка треугольника
                FillTriangle(t0, t1, t2, Color.Red);
            }
            pictureBox1.Refresh(); // Обновление PictureBox
        }

        private void FillTriangle(Point t0, Point t1, Point t2, Color color)
        {
            // Сортировка вершин по Y (t0 -> верхняя, t2 -> нижняя)
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

        // Вспомогательный метод для линейной интерполяции
        private Point Lerp(Point a, Point b, float t)
        {
            return new Point(
                (int)(a.X + (b.X - a.X) * t),
                (int)(a.Y + (b.Y - a.Y) * t)
            );
        }

        // Вспомогательный метод для обмена значений
        private void Swap(ref Point a, ref Point b)
        {
            Point temp = a;
            a = b;
            b = temp;
        }


        // Обработчик события MouseDown для pictureBox2
        private void pictureBox2_MouseDown(object sender, MouseEventArgs e)
        {
            isDrawing = true;
            lastPoint = e.Location;
        }

        // Обработчик события MouseMove для pictureBox2
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

        // Обработчик события MouseUp для pictureBox2
        private void pictureBox2_MouseUp(object sender, MouseEventArgs e)
        {
            isDrawing = false;
        }

        // Метод для построчной заливки фигуры пользователя
        private void FillUserShapeLineByLine()
        {
            for (int y = 0; y < userShapeBitmap.Height; y++)
            {
                bool inside = false;
                int xStart = 0;
                for (int x = 0; x < userShapeBitmap.Width; x++)
                {
                    if (userShapeBitmap.GetPixel(x, y).ToArgb() == Color.Black.ToArgb())
                    {
                        if (!inside)
                        {
                            xStart = x;
                            inside = true;
                        }
                    }
                    else if (inside)
                    {
                        using (Graphics g = Graphics.FromImage(userShapeBitmap))
                        {
                            g.DrawLine(Pens.Blue, xStart, y, x - 1, y);
                        }
                        inside = false;
                    }
                }
            }
            pictureBox2.Refresh(); // Обновление PictureBox
        }

        // Обработчик кнопки button1 (Рисование треугольника)
        private void button1_Click(object sender, EventArgs e)
        {
            DrawTriangle();
        }

        // Обработчик кнопки button3 (Заливка фигуры пользователя)
        private void button3_Click(object sender, EventArgs e)
        {
            FillUserShapeLineByLine();
        }
    }
}