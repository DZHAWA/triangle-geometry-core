using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfAppTriangle
{
    // Логика взаимодействия для MainWindow.xaml
    public partial class MainWindow : Window
    {
        private Point pt1 = new Point(50, 200);
        private Point pt2 = new Point(250, 200);
        private Point pt3 = new Point(150, 50);

        private Ellipse selectedVertex = null; // Вершина, которую перетаскивают в данный момент
        private bool isDragginTriangle = false;
        private Point lastMousePosition;
        public MainWindow()
        {
            InitializeComponent();
        }
        private void BtnCalculate_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs(out double a, out double b, out double c))
                return;

            // Строим треугольник по сторонам и размещаем вершины на Canvas
            BuildTriangleFromSides(a, b, c);
        }

        private bool TryParseDouble(string text, out double value)
        {
            string cleanText = (text ?? "").Replace('.', ',').Trim();
            return double.TryParse(cleanText, out value);
        }

        private bool ValidateInputs(out double a, out double b, out double c)
        {
            a = b = c = 0;
            TxtError.Text = "";

            if (string.IsNullOrWhiteSpace(TxtSideA.Text) ||
                string.IsNullOrWhiteSpace(TxtSideB.Text) ||
                string.IsNullOrWhiteSpace(TxtSideC.Text))
            {
                TxtError.Text = "Заполните все три стороны!";
                ClearAngles();
                return false;
            }

            if (!TryParseDouble(TxtSideA.Text, out a) ||
                !TryParseDouble(TxtSideB.Text, out b) ||
                !TryParseDouble(TxtSideC.Text, out c))
            {
                TxtError.Text = "Ошибка: вводите только числа.";
                ClearAngles();
                return false;
            }

            if (a <= 0 || b <= 0 || c <= 0)
            {
                TxtError.Text = "Ошибка: стороны должны быть больше нуля.";
                ClearAngles();
                return false;
            }

            if (a + b <= c || a + c <= b || b + c <= a)
            {
                TxtError.Text = "Треугольник с такими сторонами не существует.";
                ClearAngles();
                return false;
            }

            return true;
        }

        private void ClearAngles()
        {
            TxtAngleA.Text = "";
            TxtAngleB.Text = "";
            TxtAngleC.Text = "";
        }

        // Первоначальное размещение вершин на Canvas по заданным сторонам
        private void BuildTriangleFromSides(double a, double b, double c)
        {
            double canvasWidth = TriangleCanvas.ActualWidth > 50 ? TriangleCanvas.ActualWidth : 400;
            double canvasHeight = TriangleCanvas.ActualHeight > 50 ? TriangleCanvas.ActualHeight : 350;

            // Геометрический расчет вершин
            double x3 = (b * b + c * c - a * a) / (2 * c);
            double y3Squared = b * b - x3 * x3;
            double y3 = y3Squared > 0 ? Math.Sqrt(y3Squared) : 0;

            double minX = Math.Min(0, Math.Min(c, x3));
            double maxX = Math.Max(0, Math.Max(c, x3));
            double minY = 0;
            double maxY = y3;

            double triWidth = maxX - minX;
            double triHeight = maxY - minY;

            double padding = 50;
            double scaleX = (canvasWidth - padding * 2) / (triWidth == 0 ? 1 : triWidth);
            double scaleY = (canvasHeight - padding * 2) / (triHeight == 0 ? 1 : triHeight);
            double scale = Math.Min(scaleX, scaleY);

            double offsetX = (canvasWidth - triWidth * scale) / 2.0 - minX * scale;
            double offsetY = (canvasHeight - triHeight * scale) / 2.0;

            pt1 = new Point(0 * scale + offsetX, canvasHeight - (0 * scale + offsetY));
            pt2 = new Point(c * scale + offsetX, canvasHeight - (0 * scale + offsetY));
            pt3 = new Point(x3 * scale + offsetX, canvasHeight - (y3 * scale + offsetY));

            RedrawScene();
            UpdateAnglesAndSidesFromPoints();
        }

        // Перерисовка полигона и интерактивных вершин
        private void RedrawScene()
        {
            TriangleCanvas.Children.Clear();

            // Рисуем сам треугольник
            Polygon triangle = new Polygon
            {
                Points = new PointCollection { pt1, pt2, pt3 },
                Stroke = Brushes.DodgerBlue,
                StrokeThickness = 2.5,
                Fill = new SolidColorBrush(Color.FromArgb(60, 30, 144, 255)),
                StrokeLineJoin = PenLineJoin.Round
            };
            TriangleCanvas.Children.Add(triangle);




            // Создаем интерактивные точки-вершины
            CreateVertexEllipse(pt1, "P1");
            CreateVertexEllipse(pt2, "P2");
            CreateVertexEllipse(pt3, "P3");
        }

        private void CreateVertexEllipse(Point p, string name)
        {
            Ellipse ellipse = new Ellipse
            {
                Width = 14,
                Height = 14,
                Fill = Brushes.Crimson,
                Stroke = Brushes.White,
                StrokeThickness = 2,
                Tag = name
            };

            Canvas.SetLeft(ellipse, p.X - 7);
            Canvas.SetTop(ellipse, p.Y - 7);

            // Обработчик нажатия на конкретную вершину для перетаскивания
            ellipse.MouseLeftButtonDown += (s, e) =>
            {
                lastMousePosition = e.GetPosition(TriangleCanvas);
                selectedVertex = ellipse;
                TriangleCanvas.CaptureMouse();
                e.Handled = true;
            };

            TriangleCanvas.Children.Add(ellipse);
            DrawAreaText();
        }

        // ЛОГИКА 

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Если кликнули мимо вершин, ничего не делаем
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (selectedVertex != null && e.LeftButton == MouseButtonState.Pressed)
            {
                Point mousePos = e.GetPosition(TriangleCanvas);

                // Обновляем координаты соответствующей вершины
                string tag = selectedVertex.Tag.ToString();
                if (tag == "P1") pt1 = mousePos;
                else if (tag == "P2") pt2 = mousePos;
                else if (tag == "P3") pt3 = mousePos;

                // Перерисовываем сцену и пересчитываем данные «на лету»
                RedrawScene();
                UpdateAnglesAndSidesFromPoints();
            }
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (selectedVertex != null)
            {
                selectedVertex = null;
                TriangleCanvas.ReleaseMouseCapture();
            }
        }

        // ОБРАТНЫЙ ПЕРЕСЧЕТ ПО КООРДИНАТАМ ВЕРШИН

        private void UpdateAnglesAndSidesFromPoints(bool updateTextFields = true)
        {
            // Считаем длины сторон по формуле расстояния между точками на плоскости
            double a = Math.Sqrt(Math.Pow(pt3.X - pt2.X, 2) + Math.Pow(pt3.Y - pt2.Y, 2)); // напротив pt1
            double b = Math.Sqrt(Math.Pow(pt3.X - pt1.X, 2) + Math.Pow(pt3.Y - pt1.Y, 2)); // напротив pt2
            double c = Math.Sqrt(Math.Pow(pt2.X - pt1.X, 2) + Math.Pow(pt2.Y - pt1.Y, 2)); // напротив pt3

            // Если стороны сошлись в линию
            if (a + b <= c || a + c <= b || b + c <= a)
            {
                TxtError.Text = "Внимание: треугольник выродился в отрезок!";
                return;
            }
            else
            {
                TxtError.Text = "";
            }

            // Заполняем текстовые поля ввода сторон актуальными размерами в пикселях
            if (updateTextFields)
            {
                TxtSideA.Text = Math.Round(a, 1).ToString();
                TxtSideB.Text = Math.Round(b, 1).ToString();
                TxtSideC.Text = Math.Round(c, 1).ToString();
            }

            // Считаем углы по теореме косинусов
            double cosA = (b * b + c * c - a * a) / (2 * b * c);
            double cosB = (a * a + c * c - b * b) / (2 * a * c);
            double cosC = (a * a + b * b - c * c) / (2 * a * b);

            double angleA = Math.Acos(Math.Max(-1.0, Math.Min(1.0, cosA))) * (180.0 / Math.PI);
            double angleB = Math.Acos(Math.Max(-1.0, Math.Min(1.0, cosB))) * (180.0 / Math.PI);
            double angleC = Math.Acos(Math.Max(-1.0, Math.Min(1.0, cosC))) * (180.0 / Math.PI);

            TxtAngleA.Text = $"{Math.Round(angleA, 2)}°";
            TxtAngleB.Text = $"{Math.Round(angleB, 2)}°";
            TxtAngleC.Text = $"{Math.Round(angleC, 2)}°";
        }
        private void DrawAreaText()
        {
            double S = Math.Abs(pt1.X *(pt2.Y-pt3.Y) + pt2.X * (pt3.Y - pt1.Y) + pt3.X * (pt1.Y - pt2.Y)) / 2.0;

            double centerX = (pt1.X + pt2.X + pt3.X) / 3.0;
            double centerY = (pt1.Y + pt2.Y + pt3.Y) / 3.0;

            TextBox TextS = new TextBox();
            {
                Text = $"площадь:  {Math.Round(area, 1)}";
                Foreground = Brushes.White;
                FontWeight = FontWeights.Bold;
                IsHitTestVisible = false;
            }
            TextS.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Canvas.SetLeft(TextS, centerX - TextS .DesiredSize.Width / 2);
            Canvas.SetTop(TextS, centerY - TextS.DesiredSize.Height / 2);
            TriangleCanvas.Children.Add(TextS);
        }
    }
}