using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MinecraftPathGenerator
{
    public partial class MainWindow : Window
    {
        private int[,] terrain;
        private const int rows = 20, columns = 20, cellSize = 30, waterWidth = 2;
        private Random random = new Random();
        private int[,] highPoints = new int[4, 2]; // Массив для хранения координат высоких точек

        public MainWindow()
        {
            InitializeComponent();
            GenerateTerrain();
            CreateWaterBorders();
            GenerateRandomHighPoints();
            ConnectHighPoints();
            GenerateRandomPaths();
            DisplayTerrain();
            AdjustWindowSize();
        }

        private void GenerateTerrain()
        {
            terrain = new int[rows, columns];
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < columns; j++)
                    terrain[i, j] = random.Next(10);
        }

        private void CreateWaterBorders()
        {
            // Создание горизонтальной и вертикальной водных границ шириной 2 ячейки
            for (int i = 0; i < rows; i++)
                for (int w = 0; w < waterWidth; w++)
                {
                    terrain[i, columns / 2 - waterWidth / 2 + w] = -1;
                    terrain[rows / 2 - waterWidth / 2 + w, i] = -1;
                }
        }

        private void GenerateRandomHighPoints()
        {
            // Определение центрального смещения для перемещения квадрата
            int rowOffset = random.Next(1, rows / 4 - 2);
            int colOffset = random.Next(1, columns / 4 - 2);

            // Генерация высоких точек для квадратного моста с центральным смещением
            highPoints[0, 0] = rowOffset;
            highPoints[0, 1] = colOffset;

            highPoints[1, 0] = rowOffset;
            highPoints[1, 1] = columns / 2 + waterWidth / 2 + colOffset;

            highPoints[2, 0] = rows / 2 + waterWidth / 2 + rowOffset;
            highPoints[2, 1] = colOffset;

            highPoints[3, 0] = rows / 2 + waterWidth / 2 + rowOffset;
            highPoints[3, 1] = columns / 2 + waterWidth / 2 + colOffset;

            for (int i = 0; i < 4; i++)
            {
                terrain[highPoints[i, 0], highPoints[i, 1]] = 11; // Установка самой высокой точки на 11
            }
        }

        private void ConnectHighPoints()
        {
            // Создание мостов между высокими точками, чтобы образовать квадрат
            CreatePath(highPoints[0, 0], highPoints[0, 1], highPoints[1, 0], highPoints[1, 1]);
            CreatePath(highPoints[0, 0], highPoints[0, 1], highPoints[2, 0], highPoints[2, 1]);
            CreatePath(highPoints[1, 0], highPoints[1, 1], highPoints[3, 0], highPoints[3, 1]);
            CreatePath(highPoints[2, 0], highPoints[2, 1], highPoints[3, 0], highPoints[3, 1]);
        }

        private void CreatePath(int row1, int col1, int row2, int col2)
        {
            // Создание горизонтального моста
            for (int col = Math.Min(col1, col2); col <= Math.Max(col1, col2); col++)
            {
                terrain[row1, col] = 11;
            }

            // Создание вертикального моста
            for (int row = Math.Min(row1, row2); row <= Math.Max(row1, row2); row++)
            {
                terrain[row, col2] = 11;
            }
        }

        private void GenerateRandomPaths()
        {
            // Генерация случайных горизонтальных дорожек
            int numHorizontalPaths = random.Next(1, 3); // Уменьшено до 1-2
            for (int i = 0; i < numHorizontalPaths; i++)
            {
                int row = random.Next(0, rows);
                for (int col = 0; col < columns; col++)
                {
                    if (terrain[row, col] != -1 && terrain[row, col] != 11)
                    {
                        terrain[row, col] = 0; // Установка значения дорожки на 0
                    }
                }
            }

            // Генерация случайных вертикальных дорожек
            int numVerticalPaths = random.Next(1, 3); // Уменьшено до 1-2
            for (int i = 0; i < numVerticalPaths; i++)
            {
                int col = random.Next(0, columns);
                for (int row = 0; row < rows; row++)
                {
                    if (terrain[row, col] != -1 && terrain[row, col] != 11)
                    {
                        terrain[row, col] = 0; // Установка значения дорожки на 0
                    }
                }
            }
        }

        private void DisplayTerrain()
        {
            MainGrid.Children.Clear();
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    SolidColorBrush brush;
                    if (terrain[i, j] == -1)
                    {
                        brush = new SolidColorBrush(Color.FromRgb(0, 0, 255)); // Синий цвет для воды
                    }
                    else if (terrain[i, j] == 11)
                    {
                        brush = new SolidColorBrush(Color.FromRgb(255, 0, 0)); // Красный цвет для самой высокой точки и мостов
                    }
                    else if (terrain[i, j] == 0)
                    {
                        brush = new SolidColorBrush(Color.FromRgb(139, 69, 19)); // Коричневый цвет для дорожек
                    }
                    else
                    {
                        byte colorValue = (byte)(255 - terrain[i, j] * 25); // Чем выше точка, тем темнее цвет
                        brush = terrain[i, j] > 6
                            ? new SolidColorBrush(Color.FromRgb(colorValue, 139, 34))  // Зеленый цвет для гор с изменением яркости
                            : new SolidColorBrush(Color.FromRgb(139, colorValue, 19));  // Коричневый цвет для земли с изменением яркости
                    }

                    MainGrid.Children.Add(new Label
                    {
                        VerticalAlignment = VerticalAlignment.Top,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(cellSize * j, cellSize * i, 0, 0),
                        Width = cellSize,
                        Height = cellSize,
                        Background = brush,
                        Foreground = Brushes.White,  // Белый цвет для цифр
                        Content = terrain[i, j] >= 0 ? terrain[i, j].ToString() : "",  // Не отображать цифры для воды
                        HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center,
                        VerticalContentAlignment = System.Windows.VerticalAlignment.Center,
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(1)
                    });
                }
            }
        }

        private void AdjustWindowSize()
        {
            this.Width = columns * cellSize + 40;
            this.Height = rows * cellSize + 60;
        }
    }
}
