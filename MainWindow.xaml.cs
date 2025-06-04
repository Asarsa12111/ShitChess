using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
//using System.Windows.Threading; для оптимизма


namespace ChessApp1
{
    public partial class MainWindow : Window
    {
        // Константы для настройки игры
        private const int BOARD_SIZE = 8;
        private const string WHITE_PAWN = "P";
        private const string BLACK_PAWN = "P";

        // Состояние игры
        private string[,] board = new string[BOARD_SIZE, BOARD_SIZE];
        private bool isWhiteTurn = true;
        private bool isBotEnabled = true;
        private int searchDepth = 3;  // Глубина поиска для бота

        // Выбранная фигура
        private (int row, int col)? selectedPiece = null;

        // Флаг окончания игры
        private bool isGameOver = false;

        // Массив для отслеживания, двигались ли фигуры (нужно для рокировки)
        private bool[,] hasMoved = new bool[8, 8];

        // Кортеж для хранения последнего хода пешки (нужно для взятия на проходе)
        private (int row, int col) lastPawnMove;

        // Оценка фигур
        private readonly Dictionary<char, int> pieceValues = new Dictionary<char, int>
        {
            {'P', 110},  // Пешка
            {'N', 320},  // Конь
            {'B', 330},  // Слон
            {'R', 500},  // Ладья
            {'Q', 910},  // Ферзь
            {'K', 100000} // Король
        };

        // Счетчик ходов для дебюта
        private int openingMoveCount = 0;

        // Структура для хранения ходов дебюта
        private class OpeningMove
        {
            public int SrtS { get; set; }
            public int SrtSt { get; set; }
            public int ES { get; set; }
            public int ESt { get; set; }
            public string Pi { get; set; }
            // Str-Start S-Stroka E-end St-Stolbik Pi-фигура
        }

        // Список дебютов, да, да, это можно запихуть в фаил
        private readonly Dictionary<string, List<OpeningMove>> openings = new Dictionary<string, List<OpeningMove>>
        {
            // Дебют Гроба
            ["Grob"] = new List<OpeningMove>
            {
                new OpeningMove { SrtS = 1, SrtSt = 4, ES = 3, ESt = 4, Pi = "p" }, // e2-e4
                new OpeningMove { SrtS = 0, SrtSt = 5, ES = 2, ESt = 4, Pi = "b" }, // Bc1-f4
                new OpeningMove { SrtS = 1, SrtSt = 2, ES = 3, ESt = 2, Pi = "p" }, // c2-c4
                new OpeningMove { SrtS = 0, SrtSt = 1, ES = 2, ESt = 2, Pi = "n" }, // Nb1-c3
                new OpeningMove { SrtS = 0, SrtSt = 6, ES = 2, ESt = 5, Pi = "n" }, // Ng1-f3
                new OpeningMove { SrtS = 0, SrtSt = 5, ES = 1, ESt = 4, Pi = "b" }, // Bf1-e2
                new OpeningMove { SrtS = 0, SrtSt = 4, ES = 0, ESt = 6, Pi = "k" }, // O-O
            },

            // Гамбит Эванса
            ["Evans"] = new List<OpeningMove>
            {
                new OpeningMove { SrtS = 1, SrtSt = 4, ES = 3, ESt = 4, Pi = "p" }, // e2-e4
                new OpeningMove { SrtS = 1, SrtSt = 5, ES = 3, ESt = 5, Pi = "p" }, // f2-f4
                new OpeningMove { SrtS = 0, SrtSt = 6, ES = 2, ESt = 5, Pi = "n" }, // Ng1-f3
                new OpeningMove { SrtS = 0, SrtSt = 5, ES = 2, ESt = 4, Pi = "b" }, // Bf1-c4
                new OpeningMove { SrtS = 0, SrtSt = 4, ES = 0, ESt = 6, Pi = "k" }, // O-O
                new OpeningMove { SrtS = 0, SrtSt = 1, ES = 2, ESt = 2, Pi = "n" }, // Nb1-c3
                new OpeningMove { SrtS = 1, SrtSt = 3, ES = 3, ESt = 3, Pi = "p" }, // d2-d4
                new OpeningMove { SrtS = 0, SrtSt = 3, ES = 1, ESt = 4, Pi = "q" }, // Qd1-e2
            },

            // Славянская защита
            ["Slav"] = new List<OpeningMove>
            {
                new OpeningMove { SrtS = 1, SrtSt = 3, ES = 3, ESt = 3, Pi = "p" }, // d2-d4
                new OpeningMove { SrtS = 1, SrtSt = 4, ES = 3, ESt = 4, Pi = "p" }, // e2-e4
                new OpeningMove { SrtS = 0, SrtSt = 6, ES = 2, ESt = 5, Pi = "n" }, // Ng1-f3
                new OpeningMove { SrtS = 0, SrtSt = 5, ES = 2, ESt = 4, Pi = "b" }, // Bf1-c4
                new OpeningMove { SrtS = 0, SrtSt = 1, ES = 2, ESt = 2, Pi = "n" }, // Nb1-c3
                new OpeningMove { SrtS = 0, SrtSt = 4, ES = 0, ESt = 6, Pi = "k" }, // O-O
            },

            // Скандинавская защита
            ["Scandinavian"] = new List<OpeningMove>
            {
                new OpeningMove { SrtS = 1, SrtSt = 4, ES = 3, ESt = 4, Pi = "p" }, // e2-e4
                new OpeningMove { SrtS = 1, SrtSt = 3, ES = 3, ESt = 3, Pi = "p" }, // d2-d4
                new OpeningMove { SrtS = 0, SrtSt = 6, ES = 2, ESt = 5, Pi = "n" }, // Ng1-f3
                new OpeningMove { SrtS = 0, SrtSt = 5, ES = 2, ESt = 4, Pi = "b" }, // Bf1-c4
                new OpeningMove { SrtS = 0, SrtSt = 1, ES = 2, ESt = 2, Pi = "n" }, // Nb1-c3
                new OpeningMove { SrtS = 0, SrtSt = 4, ES = 0, ESt = 6, Pi = "k" }, // O-O
            },

            // Итальянская партия
            ["Italian"] = new List<OpeningMove>
            {
                new OpeningMove { SrtS = 1, SrtSt = 4, ES = 3, ESt = 4, Pi = "p" }, // e2-e4
                new OpeningMove { SrtS = 1, SrtSt = 3, ES = 3, ESt = 3, Pi = "p" }, // d2-d4
                new OpeningMove { SrtS = 0, SrtSt = 6, ES = 2, ESt = 5, Pi = "n" }, // Ng1-f3
                new OpeningMove { SrtS = 0, SrtSt = 5, ES = 2, ESt = 4, Pi = "b" }, // Bf1-c4
                new OpeningMove { SrtS = 0, SrtSt = 1, ES = 2, ESt = 2, Pi = "n" }, // Nb1-c3
                new OpeningMove { SrtS = 0, SrtSt = 4, ES = 0, ESt = 6, Pi = "k" }, // O-O
            },

            // Защита двух коней
            ["TwoKnights"] = new List<OpeningMove>
            {
                new OpeningMove { SrtS = 1, SrtSt = 4, ES = 3, ESt = 4, Pi = "p" }, // e2-e4
                new OpeningMove { SrtS = 1, SrtSt = 3, ES = 3, ESt = 3, Pi = "p" }, // d2-d4
                new OpeningMove { SrtS = 0, SrtSt = 6, ES = 2, ESt = 5, Pi = "n" }, // Ng1-f3
                new OpeningMove { SrtS = 0, SrtSt = 5, ES = 2, ESt = 4, Pi = "b" }, // Bf1-c4
                new OpeningMove { SrtS = 0, SrtSt = 1, ES = 2, ESt = 2, Pi = "n" }, // Nb1-c3
                new OpeningMove { SrtS = 0, SrtSt = 4, ES = 0, ESt = 6, Pi = "k" }, // O-O
            },

            // Защита Нимцовича
            ["Nimzowitsch"] = new List<OpeningMove>
            {
                new OpeningMove { SrtS = 1, SrtSt = 4, ES = 3, ESt = 4, Pi = "p" }, // e2-e4
                new OpeningMove { SrtS = 1, SrtSt = 3, ES = 3, ESt = 3, Pi = "p" }, // d2-d4
                new OpeningMove { SrtS = 0, SrtSt = 6, ES = 2, ESt = 5, Pi = "n" }, // Ng1-f3
                new OpeningMove { SrtS = 0, SrtSt = 5, ES = 2, ESt = 4, Pi = "b" }, // Bf1-c4
                new OpeningMove { SrtS = 0, SrtSt = 1, ES = 2, ESt = 2, Pi = "n" }, // Nb1-c3
                new OpeningMove { SrtS = 0, SrtSt = 4, ES = 0, ESt = 6, Pi = "k" }, // O-O
            },

            // Шотландская партия
            ["Scotch"] = new List<OpeningMove>
            {
                new OpeningMove { SrtS = 1, SrtSt = 4, ES = 3, ESt = 4, Pi = "p" }, // e2-e4 
                new OpeningMove { SrtS = 1, SrtSt = 3, ES = 3, ESt = 3, Pi = "p" }, // d2-d4
                new OpeningMove { SrtS = 0, SrtSt = 6, ES = 2, ESt = 5, Pi = "n" }, // Ng1-f3
                new OpeningMove { SrtS = 0, SrtSt = 5, ES = 2, ESt = 4, Pi = "b" }, // Bf1-c4
                new OpeningMove { SrtS = 0, SrtSt = 1, ES = 2, ESt = 2, Pi = "n" }, // Nb1-c3
                new OpeningMove { SrtS = 0, SrtSt = 4, ES = 0, ESt = 6, Pi = "k" }, // O-O
            }
        };

        // Добавляем словарь для кэширования оценок
        private Dictionary<string, int> positionCache = new Dictionary<string, int>();
        private int cacheHits = 0;
        private int totalEvaluations = 0;

        // Добавляем историю позиций для отслеживания повторений
        private List<string> positionHistory = new List<string>();

        // Добавляем кэш для всех часто используемых значков и цветов
        private readonly Dictionary<string, ImageSource> imageCache = new Dictionary<string, ImageSource>();
        private readonly Dictionary<string, ImageBrush> imageBrushCache = new Dictionary<string, ImageBrush>();
        private readonly Dictionary<(int, int), SolidColorBrush> cellBrushCache = new Dictionary<(int, int), SolidColorBrush>();
        private readonly SolidColorBrush whiteBrush = Brushes.White;
        private readonly SolidColorBrush grayBrush = Brushes.Gray;
        private readonly SolidColorBrush yellowBrush = Brushes.Yellow;
        private readonly SolidColorBrush redBrush = Brushes.Red;
        private readonly SolidColorBrush highlightBrush = new SolidColorBrush(Color.FromArgb(120, 30, 144, 255));

        // Добавляем кэш для возможных ходов
        private readonly Dictionary<(string piece, int row, int col), List<(int row, int col)>> movesCache = new Dictionary<(string piece, int row, int col), List<(int row, int col)>>();

        // Добавляем кэш для кнопок доски
        private readonly Dictionary<(int row, int col), Button> boardButtons = new Dictionary<(int row, int col), Button>();

        // Добавляем класс для хранения данных о ходах (использовался для не до нейронки)
        public class MoveData
        {
            public int StartRow { get; set; }
            public int StartCol { get; set; }
            public int EndRow { get; set; }
            public int EndCol { get; set; }
            public bool IsCapture { get; set; }
            public string Piece { get; set; }
        }

        // Добавляем класс для хранения настроек
        public class GameSettings
        {
            public int SearchDepth { get; set; }
            public bool IsBotEnabled { get; set; }
            public string[,] Board { get; set; }
            public bool IsWhiteTurn { get; set; }
            public bool[,] HasMoved { get; set; }
            public (int row, int col) LastPawnMove { get; set; }
        }

        // Добавляем класс для хранения данных о партии


        public class MoveRecord
        {
            public int StartRow { get; set; }
            public int StartCol { get; set; }
            public int EndRow { get; set; }
            public int EndCol { get; set; }
            public string Piece { get; set; }
            public int PositionScore { get; set; }
            public string PositionKey { get; set; }
        }

        // Добавляем список для хранения ходов текущей партии
        private List<MoveRecord> currentGameMoves = new List<MoveRecord>();

        // Инициализация окна и начало новой игры
        public MainWindow()
        {
            InitializeComponent();

            // Добавляем строку и столбец для нумерации
            ChessBoard.RowDefinitions.Add(new RowDefinition { Height = new GridLength(30) });
            ChessBoard.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(30) });

            LoadSettings(); // Загружаем настройки при запуске
        }


        /// Начало новой игры

        private void StartNewGame()
        {
            // Очищаем историю ходов
            currentGameMoves.Clear();

            // Очищаем ресурсы перед началом новой игры
            ClearResources();
            InitializeBoard();
            InitializeBoardButtons();
            DrawBoard();
            UpdateStatusText();

            if (DifficultyComboBox != null)
            {
                DifficultyComboBox.SelectedIndex = 2; // Средний уровень по умолчанию
            }

            // Сохраняем начальные настройки
            SaveSettings();
        }
        /// Выбор фигуры и подсветка возможных ходов
        private void SelectPiece(int row, int col)
        {
            if (CanSelectPiece(row, col))
            {
                // Очищаем предыдущую подсветку
                ClearHighlights();

                selectedPiece = (row, col);

                // Подсвечиваем выбранную фигуру
                HighlightSelectedPiece(row, col);

                // Подсвечиваем возможные ходы
                HighlightPossibleMoves(row, col);
            }
        }
        /// Проверка возможности выбора фигуры
        private bool CanSelectPiece(int row, int col)
        {
            // Проверяем границы массива
            if (row < 0 || row >= 8 || col < 0 || col >= 8)
                return false;

            string piece = board[row, col];
            if (string.IsNullOrEmpty(piece))
                return false;
            // Проверяем цвет фигуры и сравниваем  с тем чей ход
            bool isWhitePiece = char.IsUpper(piece[0]);
            return isWhitePiece == isWhiteTurn;
        }
        /// Попытка сделать ход
        private bool TryMakeMove(int targetRow, int targetCol)
        {
            if (!IsValidMove(selectedPiece.Value.row, selectedPiece.Value.col, targetRow, targetCol))
            {
                return false;
            }

            // Сохраняем состояние для проверки
            string oldPiece = board[targetRow, targetCol];
            // string movingPiece = board[selectedRow, selectedCol];

            // Делаем ход
            MakeMove(selectedPiece.Value.row, selectedPiece.Value.col, targetRow, targetCol);

            // Проверяем, не поставил ли игрок себя под шах а то бывают такие
            if (IsKingInCheck(isWhiteTurn))
            {
                // Отменяем ход
                UndoMove(selectedPiece.Value.row, selectedPiece.Value.col, targetRow, targetCol, oldPiece);
                return false;
            }

            // Очищаем подсветку после хода
            ClearHighlights();
            return true;
        }
        /// Изменение сложности бота

        private void DifficultyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DifficultyComboBox.SelectedItem != null)
            {
                // Устанавливаем глубину поиска в зависимости от выбранного уровня
                switch (DifficultyComboBox.SelectedIndex)
                {
                    case 0: // Легкий
                        searchDepth = 3;
                        break;
                    case 1: // Средний
                        searchDepth = 4;
                        break;
                    case 2: // Сложный
                        searchDepth = 5;
                        break;
                    case 3: // Мастер
                        searchDepth = 7;
                        break;
                }
                UpdateStatusText();
                SaveSettings();
            }
        }

        /// <summary>
        /// Обновление текста статуса
        /// </summary>
        private void UpdateStatusText()
        {
            if (isGameOver)
            {
                StatusText.Text = "Игра окончена,хватит! (ваш Бот)";
                return;
            }

            if (isWhiteTurn)
            {
                StatusText.Text = "Ход белых";
            }
            else
            {
                StatusText.Text = isBotEnabled ? "Ход бота" : "Ход черных";
            }
        }

        // Метод инициализации начальной позиции фигур
        private void InitializeBoard()
        {
            // Создаем массивы для доски и отслеживания ходов
            board = new string[8, 8];
            hasMoved = new bool[8, 8];
            lastPawnMove = (-1, -1);

            // Расставляем пешки
            for (int i = 0; i < 8; i++)
            {
                board[1, i] = "P"; // Белые пешки на второй линии
                board[6, i] = "p"; // Черные пешки на седьмой линии
            }

            // Расставляем остальные фигуры
            board[0, 0] = board[0, 7] = "R"; // Белые ладьи
            board[7, 0] = board[7, 7] = "r"; // Черные ладьи

            board[0, 1] = board[0, 6] = "N"; // Белые кони
            board[7, 1] = board[7, 6] = "n"; // Черные кони

            board[0, 2] = board[0, 5] = "B"; // Белые слоны
            board[7, 2] = board[7, 5] = "b"; // Черные слоны

            board[0, 3] = "K"; // Белый король
            board[7, 3] = "k"; // Черный король

            board[0, 4] = "Q"; // Белый ферзь
            board[7, 4] = "q"; // Черный ферзь

            // Заполняем пустые клетки пустыми значениями
            for (int i = 2; i < 6; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    board[i, j] = "";
                }
            }
        }

        // Метод получения изображения для фигуры
        private ImageSource GetImageForPiece(string piece)
        {
            if (string.IsNullOrEmpty(piece))
                return null;

            // Проверяем кэш
            if (imageCache.TryGetValue(piece, out var cachedImage))
            {
                return cachedImage;
            }

            string imageName = "";

            // Определяем имя файла изображения в зависимости от фигуры
            switch (piece)
            {
                case "P": imageName = "wp"; break; // Белая пешка
                case "p": imageName = "bp"; break; // Черная пешка
                case "R": imageName = "wb"; break; // Белая ладья
                case "r": imageName = "bb"; break; // Черная ладья
                case "N": imageName = "wn"; break; // Белый конь
                case "n": imageName = "bn"; break; // Черный конь
                case "B": imageName = "wr"; break; // Белый слон
                case "b": imageName = "br"; break; // Черный слон
                case "Q": imageName = "wq"; break; // Белый ферзь
                case "q": imageName = "bq"; break; // Черный ферзь
                case "K": imageName = "wk"; break; // Белый король
                case "k": imageName = "bk"; break; // Черный король
            }

            // Если имя файла определено, создаем и кэшируем изображение
            if (!string.IsNullOrEmpty(imageName))
            {
                var image = new BitmapImage(new Uri($"pack://application:,,,/Images/{imageName}.png"));
                imageCache[piece] = image;
                return image;
            }

            return null;
        }

        // Метод получения кисти для фигуры
        private ImageBrush GetImageBrushForPiece(string piece)
        {
            if (string.IsNullOrEmpty(piece))
                return null;

            // Проверяем кэш
            if (imageBrushCache.TryGetValue(piece, out var cachedBrush))
            {
                return cachedBrush;
            }

            var imageSource = GetImageForPiece(piece);
            if (imageSource != null)
            {
                var brush = new ImageBrush(imageSource) { Stretch = Stretch.Uniform };
                imageBrushCache[piece] = brush;
                return brush;
            }

            return null;
        }

        // Метод получения кисти для клетки
        private SolidColorBrush GetCellBrush(int row, int col)
        {
            var key = (row, col);
            if (cellBrushCache.TryGetValue(key, out var cachedBrush))
            {
                return cachedBrush;
            }

            var brush = (row + col) % 2 == 0 ? whiteBrush : grayBrush;
            cellBrushCache[key] = brush;
            return brush;
        }

        /// <summary>
        /// Инициализация кнопок доски
        /// </summary>
        private void InitializeBoardButtons()
        {
            boardButtons.Clear();
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    var button = new Button
                    {
                        Template = (ControlTemplate)FindResource("RoundButtonTemplate"),
                        Margin = new Thickness(-1),
                        Background = Brushes.Transparent
                    };

                    button.Tag = new Point(i, j);
                    button.Click += (s, e) =>
                    {
                        var btn = (Button)s;
                        var coords = (Point)btn.Tag;
                        OnButtonClick((int)coords.X, (int)coords.Y);
                    };

                    Grid.SetRow(button, i);
                    Grid.SetColumn(button, j);
                    boardButtons[(i, j)] = button;
                    ChessBoard.Children.Add(button);
                }
            }
        }

        /// <summary>
        /// Получение кнопки по координатам
        /// </summary>
        private Button GetButtonAt(int row, int col)
        {
            return boardButtons.TryGetValue((row, col), out var button) ? button : null;
        }

        // Метод отрисовки шахматной доски
        private void DrawBoard()
        {
            // Очищаем доску
            ChessBoard.Children.Clear();
            boardButtons.Clear();

            // Добавляем буквы (h-a) по горизонтали
            for (int i = 0; i < 8; i++)
            {
                var letterLabel = new TextBlock
                {
                    Text = ((char)('h' - i)).ToString(), // Изменено с ('a' + i) на ('h' - i) ибо так надо
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 14,
                    FontWeight = FontWeights.Bold
                };
                Grid.SetRow(letterLabel, 8);
                Grid.SetColumn(letterLabel, i);
                ChessBoard.Children.Add(letterLabel);
            }

            // Добавляем цифры (1-8) по вертикали
            for (int i = 0; i < 8; i++)
            {
                var numberLabel = new TextBlock
                {
                    Text = (i + 1).ToString(),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 14,
                    FontWeight = FontWeights.Bold
                };
                Grid.SetRow(numberLabel, i);
                Grid.SetColumn(numberLabel, 8);
                ChessBoard.Children.Add(numberLabel);
            }

            // Проходим по всем клеткам доски
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    var button = new Button
                    {
                        Template = (ControlTemplate)FindResource("RoundButtonTemplate"),
                        Margin = new Thickness(-1)
                    };

                    button.Tag = new Point(i, j);
                    button.Click += (s, e) =>
                    {
                        var btn = (Button)s;
                        var coords = (Point)btn.Tag;
                        OnButtonClick((int)coords.X, (int)coords.Y);
                    };

                    string piece = board[i, j];   // Получаем фигуру на этой клетке

                    // Если на клетке есть фигура, создаем изображение
                    var imageBrush = GetImageBrushForPiece(piece);
                    if (imageBrush != null)
                    {
                        button.Background = imageBrush;
                    }
                    else
                    {
                        // Если фигуры нет, задаем цвет клетки
                        button.Background = (i + j) % 2 == 0 ? whiteBrush : grayBrush;
                    }

                    // Если клетка выбрана, подсвечиваем её желтым
                    if (selectedPiece.HasValue && i == selectedPiece.Value.row && j == selectedPiece.Value.col)
                    {
                        button.Background = yellowBrush;
                    }

                    // Размещаем кнопку в сетке
                    Grid.SetRow(button, i);
                    Grid.SetColumn(button, j);
                    ChessBoard.Children.Add(button);
                    boardButtons[(i, j)] = button;
                }
            }
        }

        /// <summary>
        /// Очистка ресурсов
        /// </summary>
        private void ClearResources()
        {
            // Очищаем кэши
            imageCache.Clear();
            imageBrushCache.Clear();
            cellBrushCache.Clear();
            movesCache.Clear();
            boardButtons.Clear();

            // Очищаем доску
            ChessBoard.Children.Clear();

            // Очищаем выбранную фигуру
            selectedPiece = null;
        }

        /// <summary>
        /// Проверка, находится ли король под шахом
        /// </summary>
        private bool IsKingInCheck(bool isWhite)
        {
            // Находим позицию короля
            int kingRow = -1, kingCol = -1;
            string king = isWhite ? "K" : "k";

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    if (board[row, col] == king)
                    {
                        kingRow = row;
                        kingCol = col;
                        break;
                    }
                }
                if (kingRow != -1) break;
            }

            if (kingRow == -1) return false;

            // Проверяем все фигуры противника
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    string piece = board[row, col];
                    if (!string.IsNullOrEmpty(piece) && char.IsUpper(piece[0]) != isWhite)
                    {
                        // Проверяем, может ли фигура атаковать короля
                        if (IsValidMove(row, col, kingRow, kingCol))
                        {
                            // Дополнительная проверка для пешек
                            if (char.ToUpper(piece[0]) == 'P')
                            {
                                // Пешки атакуют только по диагонали
                                int direction = char.IsUpper(piece[0]) ? -1 : 1;
                                if (Math.Abs(col - kingCol) == 1 && row + direction == kingRow)
                                {
                                    return true;
                                }
                            }
                            else
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Проверка мата
        /// </summary>
        private bool IsCheckmate(bool isWhite)
        {
            // Сначала проверяем, находится ли король под шахом
            if (!IsKingInCheck(isWhite))
                return false;

            // Проверяем все возможные ходы всех фигур текущей стороны
            for (int startRow = 0; startRow < 8; startRow++)
            {
                for (int startCol = 0; startCol < 8; startCol++)
                {
                    string piece = board[startRow, startCol];
                    if (!string.IsNullOrEmpty(piece) && char.IsUpper(piece[0]) == isWhite)
                    {
                        // Проверяем все возможные ходы для текущей фигуры
                        for (int endRow = 0; endRow < 8; endRow++)
                        {
                            for (int endCol = 0; endCol < 8; endCol++)
                            {
                                if (IsValidMove(startRow, startCol, endRow, endCol))
                                {
                                    // Сохраняем состояние доски
                                    string[,] boardCopy = (string[,])board.Clone();
                                    bool[,] hasMovedCopy = (bool[,])hasMoved.Clone();
                                    string capturedPiece = board[endRow, endCol];

                                    // Делаем временный ход
                                    board[endRow, endCol] = board[startRow, startCol];
                                    board[startRow, startCol] = "";
                                    hasMoved[endRow, endCol] = true;

                                    // Проверяем, остался ли король под шахом
                                    bool stillInCheck = IsKingInCheck(isWhite);

                                    // Восстанавливаем доску
                                    board = boardCopy;
                                    hasMoved = hasMovedCopy;

                                    if (!stillInCheck)
                                        return false;
                                }
                            }
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Проверка возможности перекрытия линии шаха
        /// </summary>
        private bool CanBlockCheck(bool isWhite)
        {
            // Находим позицию короля
            int kingRow = -1, kingCol = -1;
            string king = isWhite ? "K" : "k";

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    if (board[row, col] == king)
                    {
                        kingRow = row;
                        kingCol = col;
                        break;
                    }
                }
                if (kingRow != -1) break;
            }

            if (kingRow == -1) return false;

            // Находим фигуру, которая ставит шах
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    string piece = board[row, col];
                    if (!string.IsNullOrEmpty(piece) && char.IsUpper(piece[0]) != isWhite)
                    {
                        if (IsValidMove(row, col, kingRow, kingCol))
                        {
                            // Если шах ставит конь или пешка, перекрыть линию нельзя
                            if (char.ToUpper(piece[0]) == 'N' || char.ToUpper(piece[0]) == 'P')
                                continue;

                            // Получаем все клетки между атакующей фигурой и королем
                            var path = GetPathBetweenPieces(row, col, kingRow, kingCol);

                            // Проверяем, можно ли перекрыть линию шаха
                            foreach (var (pathRow, pathCol) in path)
                            {

                                for (int i = 0; i < 64; i++)
                                {
                                    int blockRow = i / 8;
                                    int blockCol = i % 8;

                                    string blockingPiece = board[blockRow, blockCol];
                                    if (!string.IsNullOrEmpty(blockingPiece) && char.IsUpper(blockingPiece[0]) == isWhite)
                                    {
                                        if (IsValidMove(blockRow, blockCol, pathRow, pathCol))
                                        {
                                            // Сохраняем состояние доски
                                            string[,] boardCopy = (string[,])board.Clone();
                                            bool[,] hasMovedCopy = (bool[,])hasMoved.Clone();

                                            // Делаем временный ход
                                            MakeTemporaryMove((blockRow, blockCol, pathRow, pathCol));

                                            // Проверяем, остался ли король под шахом
                                            bool stillInCheck = IsKingInCheck(isWhite);

                                            // Восстанавливаем доску
                                            RestoreBoard(boardCopy, hasMovedCopy);

                                            if (!stillInCheck)
                                                return true;
                                        }
                                    }

                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Получение пути между двумя фигурами
        /// </summary>
        private List<(int row, int col)> GetPathBetweenPieces(int startRow, int startCol, int endRow, int endCol)
        {
            var path = new List<(int row, int col)>();

            int rowDiff = endRow - startRow;
            int colDiff = endCol - startCol;

            int rowStep = rowDiff == 0 ? 0 : rowDiff / Math.Abs(rowDiff);
            int colStep = colDiff == 0 ? 0 : colDiff / Math.Abs(colDiff);

            int currentRow = startRow + rowStep;
            int currentCol = startCol + colStep;

            while (currentRow != endRow || currentCol != endCol)
            {
                path.Add((currentRow, currentCol));
                currentRow += rowStep;
                currentCol += colStep;
            }

            return path;
        }


        // Проверка пата

        private bool IsStalemate(bool isWhite)
        {
            if (IsKingInCheck(isWhite))
                return false;

            // Проверяем все возможные ходы
            for (int i = 0; i < 64; i++)
            {
                int startRow = i / 8;
                int startCol = i % 8;
                string piece = board[startRow, startCol];

                if (!string.IsNullOrEmpty(piece) && char.IsUpper(piece[0]) == isWhite)
                {
                    for (int j = 0; j < 64; j++)
                    {
                        int endRow = j / 8;
                        int endCol = j % 8;

                        if (IsValidMove(startRow, startCol, endRow, endCol))
                        {
                            // Сохраняем состояние доски
                            string[,] boardCopy = (string[,])board.Clone();
                            bool[,] hasMovedCopy = (bool[,])hasMoved.Clone();

                            // Делаем временный ход
                            MakeTemporaryMove((startRow, startCol, endRow, endCol));

                            // Проверяем, не поставит ли ход короля под шах
                            bool inCheck = IsKingInCheck(isWhite);

                            // Восстанавливаем доску
                            RestoreBoard(boardCopy, hasMovedCopy);

                            if (!inCheck)
                                return false;
                        }
                    }
                }
            }
            return true;
        }

        /// Проверка пата по повтору

        private bool IsThreefoldRepetition()
        {
            if (positionHistory.Count < 6) // Нужно минимум 3 повторения (6 ходов)
                return false;

            string currentPosition = GetPositionKey();
            int repetitions = 0;

            // Проверяем последние 50 ходов
            for (int i = positionHistory.Count - 1; i >= Math.Max(0, positionHistory.Count - 50); i--)
            {
                if (positionHistory[i] == currentPosition)
                {
                    repetitions++;
                    if (repetitions >= 3)
                        return true;
                }
            }
            return false;
        }

        /// Проверка окончания игры

        private bool IsGameOver(bool showDialog = true)
        {
            try
            {
                // Проверяем наличие королей (самая приоритетная проверка)
                bool whiteKingExists = IsKingPresent(true);
                bool blackKingExists = IsKingPresent(false);

                if (!whiteKingExists || !blackKingExists)
                {
                    string message = !whiteKingExists ? "Король белых взят! Победа чёрных!"
                                                    : "Король чёрных взят! Победа белых!";
                    isGameOver = true;
                    if (showDialog)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            ShowGameOverDialog(message);
                        });
                    }
                    return true;
                }

                // Проверяем состояние доски для белых и черных
                var whiteState = BoardStateCheck(true);
                var blackState = BoardStateCheck(false);

                if (whiteState.isCheckmate || blackState.isCheckmate)
                {
                    string message = whiteState.isCheckmate ? "Мат! Победа чёрных!"
                                                  : "Мат! Победа белых!";
                    isGameOver = true;
                    if (showDialog)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            ShowGameOverDialog(message);
                        });
                    }
                    return true;
                }

                if (whiteState.isStalemate || blackState.isStalemate)
                {
                    isGameOver = true;
                    if (showDialog)
                    {
                        Dispatcher.Invoke(() => ShowGameOverDialog("Ничья по троекратному повторению!"));
                    }
                    isGameOver = true;
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в IsGameOver: {ex.Message}");
                return false;
            }
        }

        private void ShowGameOverDialog(string message)
        {
            try
            {
                var dialog = new GameOverWindow(message);
                dialog.Owner = this;
                dialog.ShowDialog(); // Блокирующий вызов

                // После закрытия диалога
                if (dialog.PlayAgain)
                {
                    StartNewGame();
                }
                else
                {
                    Application.Current.Shutdown();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при показе диалога: {ex.Message}");
                Application.Current.Shutdown();
            }
        }


        // Метод обработки клика по кнопке

        private void OnButtonClick(int row, int col)
        {
            if (isGameOver || (isBotEnabled && !isWhiteTurn)) return;

            if (selectedPiece.HasValue)
            {
                if (selectedPiece.Value.row == row && selectedPiece.Value.col == col)
                {
                    selectedPiece = null;
                    DrawBoard();
                    return;
                }

                var analysis = AnalyzeMove(selectedPiece.Value.row, selectedPiece.Value.col, row, col, isWhiteTurn);
                if (analysis.isValid)
                {
                    MakeMove(selectedPiece.Value.row, selectedPiece.Value.col, row, col);
                    selectedPiece = null;
                    isWhiteTurn = !isWhiteTurn;
                    UpdateStatusText();

                    if (isBotEnabled && !isWhiteTurn)
                    {
                        MakeBotMove();
                    }
                }
                else if (CanSelectPiece(row, col))
                {
                    SelectPiece(row, col);
                }
            }
            else if (CanSelectPiece(row, col))
            {
                SelectPiece(row, col);
            }
        }

        // Метод проверки допустимости хода
        private bool IsValidMove(int startRow, int startCol, int endRow, int endCol)
        {
            // Проверяем границы доски
            if (startRow < 0 || startRow >= 8 || startCol < 0 || startCol >= 8 ||
                endRow < 0 || endRow >= 8 || endCol < 0 || endCol >= 8)
            {
                return false;
            }

            // Получаем фигуру
            string piece = board[startRow, startCol];
            if (string.IsNullOrEmpty(piece))
                return false;

            // Проверяем, не пытаемся ли мы съесть свою фигуру
            string targetPiece = board[endRow, endCol];
            if (!string.IsNullOrEmpty(targetPiece) &&
                char.IsUpper(piece[0]) == char.IsUpper(targetPiece[0]))
            {
                return false;
            }

            // Проверяем правила хода для каждой фигуры
            switch (char.ToUpper(piece[0]))
            {
                case 'P': return IsValidPawnMove(startRow, startCol, endRow, endCol);
                case 'R': return IsValidRookMove(startRow, startCol, endRow, endCol);
                case 'N': return IsValidKnightMove(startRow, startCol, endRow, endCol);
                case 'B': return IsValidBishopMove(startRow, startCol, endRow, endCol);
                case 'Q': return IsValidQueenMove(startRow, startCol, endRow, endCol);
                case 'K': return IsValidKingMove(startRow, startCol, endRow, endCol);
                default: return false;
            }
        }

        private bool IsValidPawnMove(int startRow, int startCol, int endRow, int endCol)
        {
            if (startRow < 0 || startRow >= 8 || startCol < 0 || startCol >= 8 ||
                endRow < 0 || endRow >= 8 || endCol < 0 || endCol >= 8)
            {
                return false;
            }

            string piece = board[startRow, startCol];
            if (string.IsNullOrEmpty(piece))
                return false;

            bool isWhite = char.IsUpper(piece[0]);
            int direction = isWhite ? 1 : -1;
            int startRowPawn = isWhite ? 1 : 6;

            // Обычный ход вперед
            if (endCol == startCol)
            {
                // Ход на одну клетку
                if (endRow == startRow + direction && board[endRow, endCol] == "")
                    return true;

                // Ход на две клетки с начальной позиции
                if (startRow == startRowPawn && endRow == startRow + 2 * direction &&
                    board[endRow, endCol] == "" && board[startRow + direction, startCol] == "")
                    return true;
            }
            // Взятие (включая взятие на проходе)
            else if (Math.Abs(endCol - startCol) == 1 && endRow == startRow + direction)
            {
                // Обычное взятие
                if (board[endRow, endCol] != "" && char.IsUpper(board[endRow, endCol][0]) != isWhite)
                    return true;

                // Взятие на проходе
                if (board[endRow, endCol] == "" && endRow == (isWhite ? 5 : 2))
                {
                    string lastPawn = board[startRow, endCol];
                    if (!string.IsNullOrEmpty(lastPawn) &&
                        char.ToUpper(lastPawn[0]) == 'P' &&
                        char.IsUpper(lastPawn[0]) != isWhite &&
                        lastPawnMove.row == startRow &&
                        lastPawnMove.col == endCol)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool IsValidRookMove(int startRow, int startCol, int endRow, int endCol)
        {
            if (startRow != endRow && startCol != endCol)
                return false;

            return IsPathClear(startRow, startCol, endRow, endCol);
        }

        private bool IsValidKnightMove(int startRow, int startCol, int endRow, int endCol)
        {
            int rowDiff = Math.Abs(endRow - startRow);
            int colDiff = Math.Abs(endCol - startCol);

            // Проверяем, что ход соответствует правилам движения коня
            if (!((rowDiff == 2 && colDiff == 1) || (rowDiff == 1 && colDiff == 2)))
                return false;

            // Проверяем, что на конечной клетке нет своей фигуры
            string piece = board[startRow, startCol];
            string targetPiece = board[endRow, endCol];
            if (!string.IsNullOrEmpty(targetPiece) && char.IsUpper(piece[0]) == char.IsUpper(targetPiece[0]))
                return false;

            return true;
        }

        private bool IsValidBishopMove(int startRow, int startCol, int endRow, int endCol)
        {
            if (Math.Abs(endRow - startRow) != Math.Abs(endCol - startCol))
                return false;

            return IsPathClear(startRow, startCol, endRow, endCol);
        }

        private bool IsValidQueenMove(int startRow, int startCol, int endRow, int endCol)
        {
            return IsValidRookMove(startRow, startCol, endRow, endCol) ||
                   IsValidBishopMove(startRow, startCol, endRow, endCol);
        }

        private bool IsValidKingMove(int startRow, int startCol, int endRow, int endCol)
        {
            // Проверяем границы массива
            if (startRow < 0 || startRow >= 8 || startCol < 0 || startCol >= 8 ||
                endRow < 0 || endRow >= 8 || endCol < 0 || endCol >= 8)
            {
                return false;
            }

            string piece = board[startRow, startCol];
            if (string.IsNullOrEmpty(piece))
                return false;

            bool isWhite = char.IsUpper(piece[0]);
            int rowDiff = Math.Abs(endRow - startRow);
            int colDiff = Math.Abs(endCol - startCol);

            // Обычный ход короля
            if (rowDiff <= 1 && colDiff <= 1)
            {
                // Проверяем, что на конечной клетке нет своей фигуры
                if (!string.IsNullOrEmpty(board[endRow, endCol]) &&
                    char.IsUpper(board[endRow, endCol][0]) == isWhite)
                {
                    return false;
                }

                // Сохраняем состояние доски
                string[,] boardCopy = (string[,])board.Clone();
                bool[,] hasMovedCopy = (bool[,])hasMoved.Clone();

                // Делаем временный ход
                MakeTemporaryMove((startRow, startCol, endRow, endCol));

                // Проверяем, не поставит ли ход короля под шах
                bool inCheck = IsKingInCheck(isWhite);

                // Восстанавливаем доску
                RestoreBoard(boardCopy, hasMovedCopy);

                return !inCheck;
            }

            // Рокировка
            if (rowDiff == 0 && colDiff == 2)
            {
                // Проверяем, что король и ладья не двигались
                if (hasMoved[startRow, startCol])
                    return false;

                bool isKingSide = endCol > startCol;
                int rookCol = isKingSide ? 7 : 0;

                // Проверяем, что ладья существует и не двигалась
                string rook = board[startRow, rookCol];
                if (string.IsNullOrEmpty(rook) || hasMoved[startRow, rookCol])
                    return false;

                // Проверяем, что ладья правильного цвета
                if (char.IsUpper(rook[0]) != isWhite)
                    return false;

                // Проверяем, свободен ли путь
                int direction = isKingSide ? 1 : -1;
                for (int col = startCol + direction; col != rookCol; col += direction)
                {
                    if (board[startRow, col] != "")
                        return false;
                }

                // Проверяем, не находится ли король под шахом
                if (IsKingInCheck(isWhite))
                    return false;

                // Проверяем, не проходит ли король через поле под шахом
                for (int col = startCol; col != endCol; col += direction)
                {
                    string temp = board[startRow, startCol];
                    board[startRow, startCol] = "";
                    if (IsKingInCheck(isWhite))
                    {
                        board[startRow, startCol] = temp;
                        return false;
                    }
                    board[startRow, startCol] = temp;
                }
                // Проверяем, что король не находится под шахом
                if (IsKingInCheck(isWhite))
                    return false;

                // Проверяем, что клетки между королем и ладьей не находятся под атакой

                for (int col = startCol; col != endCol; col += direction)
                {
                    if (IsSquareUnderAttack(startRow, col, isWhite))
                        return false;
                }
                return true;
            }

            return false;
        }


        /// Получение имени позиции в шахматной нотации

        private string GetPositionName(int row, int col)
        {
            char file = (char)('a' + col);
            int rank = 8 - row;
            return $"{file}{rank}";
        }
        /// <summary>
        /// Обработчик нажатия кнопки бота
        /// </summary>
        private void BotButton_Click(object sender, RoutedEventArgs e)
        {
            isBotEnabled = !isBotEnabled;
            UpdateStatusText();
            BotButton.Content = isBotEnabled ? "БОТ" : "ИГРОК";
            SaveSettings(); // Сохраняем настройки при изменении режима бота

            if (isBotEnabled && !isWhiteTurn)
            {
                MakeBotMove();
            }
        }
        /// Очистка всех подсветок на доске
        private void ClearHighlights()
        { //один цикл, который проходит по всем 64 клеткам.
            for (int i = 0; i < 64; i++)
            {
                int row = i / 8;
                int col = i % 8;
                var button = GetButtonAt(row, col);

                if (button != null)
                {
                    // Восстанавливаем исходный цвет клетки
                    button.Background = (row + col) % 2 == 0 ? whiteBrush : grayBrush;
                    button.BorderBrush =  grayBrush;
                    button.BorderThickness = new Thickness(1);

                    // Если на клетке есть фигура, восстанавливаем её изображение
                    if (!string.IsNullOrEmpty(board[row, col]))
                    {
                        var imageBrush = GetImageBrushForPiece(board[row, col]);
                        if (imageBrush != null)
                        {
                            button.Background = imageBrush;
                        }
                    }
                }
            }
        }
        /// Подсветка выбранной фигуры
        private void HighlightSelectedPiece(int row, int col)
        {
            var button = GetButtonAt(row, col);
            if (button != null)
            {
                button.Background = yellowBrush;
            }
        }
        /// Подсветка возможных ходов
        private void HighlightPossibleMoves(int row, int col)
        {
            // Очищаем предыдущую подсветку
            ClearHighlights();

            // Подсвечиваем выбранную фигуру
            var selectedButton = GetButtonAt(row, col);
            if (selectedButton != null)
            {
                selectedButton.BorderBrush = yellowBrush;
                selectedButton.BorderThickness = new Thickness(3);
            }

            // Получаем фигуру
            string piece = board[row, col];
            if (string.IsNullOrEmpty(piece))
                return;

            // Получаем возможные ходы из кэша или вычисляем их
            var possibleMoves = GetPossibleMovesForPiece(piece, row, col);

            // Подсвечиваем возможные ходы
            foreach (var move in possibleMoves)
            {
                var button = GetButtonAt(move.row, move.col);
                if (button != null)
                {
                    if (!string.IsNullOrEmpty(board[move.row, move.col]))
                    {
                        // Возможное взятие — красная рамка
                        button.BorderBrush = redBrush;
                        button.BorderThickness = new Thickness(3);
                    }
                    else
                    {
                        // Возможный ход — полупрозрачная голубая заливка
                        button.Background = highlightBrush;
                    }
                }
            }
        }
        /// Получение возможных ходов для фигуры
        private List<(int row, int col)> GetPossibleMovesForPiece(string piece, int row, int col)
        {
            var key = (piece, row, col);

            // Проверяем кэш
            if (movesCache.TryGetValue(key, out var cachedMoves))
            {
                return cachedMoves;
            }

            // Вычисляем возможные ходы
            var moves = new List<(int row, int col)>();
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (IsValidMove(row, col, i, j))
                    {
                        moves.Add((i, j));
                    }
                }
            }

            // Сохраняем в кэш
            movesCache[key] = moves;
            return moves;
        }
        /// Очистка кэша ходов при перемещении фигуры
        /// </summary>
        private void ClearMovesCache(int startRow, int startCol, int endRow, int endCol)
        {
            // Очищаем кэш для всех фигур, так как их возможные ходы могли измениться
            movesCache.Clear();
        }

        /// <summary>
        /// Метод проверки свободного пути между фигурами
        /// </summary>
        private bool IsPathClear(int startRow, int startCol, int endRow, int endCol)
        {
            // Не проверяем путь для коня
            if (Math.Abs(startRow - endRow) == 2 && Math.Abs(startCol - endCol) == 1 ||
                Math.Abs(startRow - endRow) == 1 && Math.Abs(startCol - endCol) == 2)
            {
                return true;
            }

            int rowStep = Math.Sign(endRow - startRow);
            int colStep = Math.Sign(endCol - startCol);

            int currentRow = startRow + rowStep;
            int currentCol = startCol + colStep;

            // Проверяем все клетки до конечной (не включая её)
            while (currentRow != endRow || currentCol != endCol)
            {
                if (currentRow < 0 || currentRow >= 8 || currentCol < 0 || currentCol >= 8)
                    return false;

                if (board[currentRow, currentCol] != "")
                    return false;

                currentRow += rowStep;
                currentCol += colStep;
            }

            // Теперь проверяем конечную клетку - если там фигура противника, ход наш
            if (board[endRow, endCol] != "")
            {
                return char.IsUpper(board[startRow, startCol][0]) != char.IsUpper(board[endRow, endCol][0]);
            }

            return true;
        }


        /// Выполнение хода на доске

        private void MakeMove(int startRow, int startCol, int endRow, int endCol)
        {
            // Очищаем кэш позиций перед новым ходом
            ClearPositionCache();

            // Сохраняем текущую позицию в историю
            positionHistory.Add(GetPositionKey());

            // Сохраняем фигуру, которая делает ход
            string movingPiece = board[startRow, startCol];

            // Записываем ход
            var moveRecord = new MoveRecord
            {
                StartRow = startRow,
                StartCol = startCol,
                EndRow = endRow,
                EndCol = endCol,
                Piece = movingPiece,
                PositionKey = GetPositionKey(),
                PositionScore = EvaluatePosition()
            };
            currentGameMoves.Add(moveRecord);

            // Проверяем рокировку
            if (char.ToUpper(movingPiece[0]) == 'K' && Math.Abs(endCol - startCol) == 2)
            {
                bool isKingSide = endCol > startCol;
                int rookCol = isKingSide ? 7 : 0;
                int rookNewCol = isKingSide ? endCol - 1 : endCol + 1;

                // Перемещаем ладью
                string rook = board[startRow, rookCol];
                board[startRow, rookNewCol] = rook;
                board[startRow, rookCol] = "";

                // Обновляем флаги перемещения
                hasMoved[startRow, rookCol] = true;
                hasMoved[startRow, rookNewCol] = true;
            }

            // Проверяем превращение пешки
            if (IsPawnPromotion(startRow, endRow, movingPiece))
            {
                movingPiece = HandlePawnPromotion(movingPiece == "P");
            }

            // Выполняем ход
            board[endRow, endCol] = movingPiece;
            board[startRow, startCol] = "";

            // Отмечаем, что фигура двигалась
            hasMoved[startRow, startCol] = true;
            hasMoved[endRow, endCol] = true;

            // Очищаем кэш ходов
            ClearMovesCache(startRow, startCol, endRow, endCol);

            // Обновляем отображение доски
            DrawBoard();

            // Сохраняем настройки после хода
            SaveSettings();

            // Проверяем окончание игры
            if (IsGameOver())
            {
                isGameOver = true;
                UpdateStatusText();
            }
        }

        /// <summary>
        /// Отмена хода
        /// </summary>
        private void UndoMove(int startRow, int startCol, int endRow, int endCol, string capturedPiece)
        {
            board[startRow, startCol] = board[endRow, endCol];
            board[endRow, endCol] = capturedPiece;
            DrawBoard();
        }

        /// <summary>
        /// Временный ход для оценки
        /// </summary>
        private void MakeTemporaryMove((int startRow, int startCol, int endRow, int endCol) move)
        {
            board[move.endRow, move.endCol] = board[move.startRow, move.startCol];
            board[move.startRow, move.startCol] = "";
            hasMoved[move.startRow, move.startCol] = true;
            hasMoved[move.endRow, move.endCol] = true;
        }

        /// <summary>
        /// Восстановление состояния доски
        /// </summary>
        private void RestoreBoard(string[,] boardCopy, bool[,] hasMovedCopy)
        {
            board = boardCopy;
            hasMoved = hasMovedCopy;
        }

        /// <summary>
        /// Получение ключа для кэширования позиции
        /// </summary>
        private string GetPositionKey()
        {
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    sb.Append(board[i, j] ?? " ");
                    sb.Append(hasMoved[i, j] ? "1" : "0");
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Выполнение хода бота
        /// </summary>
        private void MakeBotMove()
        {
            if (!isBotEnabled || isWhiteTurn || isGameOver)
                return;

            // Сначала пробуем сделать ход из дебюта
            if (!TryOpeningMove())
            {
                // Если не удалось сделать ход из дебюта, ищем лучший ход
                var bestMove = FindBestMove();
                if (bestMove != null)
                {
                    MakeMove(bestMove.Value.startRow, bestMove.Value.startCol, bestMove.Value.endRow, bestMove.Value.endCol);
                }
            }

            isWhiteTurn = true; // Смена хода после хода бота
            UpdateStatusText();
        }

        /// <summary>
        /// Получение имени фигуры
        /// </summary>
        private string GetPieceName(string piece)
        {
            if (string.IsNullOrEmpty(piece))
                return "";

            char pieceChar = char.ToUpper(piece[0]);
            bool isWhite = char.IsUpper(piece[0]);

            string color = isWhite ? "белая" : "черная";
            string name;

            switch (pieceChar)
            {
                case 'P': name = "пешка"; break;
                case 'N': name = "конь"; break;
                case 'B': name = "слон"; break;
                case 'R': name = "ладья"; break;
                case 'Q': name = "ферзь"; break;
                case 'K': name = "король"; break;
                default: name = "неизвестная фигура"; break;
            }

            return $"{color} {name}";
        }

        /// <summary>
        /// Проверка на превращение пешки
        /// </summary>
        private bool IsPawnPromotion(int startRow, int endRow, string piece)
        {
            return (piece == "P" && endRow == 7) || (piece == "p" && endRow == 0);
        }

        /// <summary>
        /// Обработка превращения пешки
        /// </summary>
        private string HandlePawnPromotion(bool isWhite)
        {
            // По умолчанию превращаем в ферзя
            return isWhite ? "Q" : "q";
        }

        /// <summary>
        /// Проверка, защищена ли фигура
        /// </summary>
        private bool IsPieceProtected(int row, int col, bool isWhite)
        {
            // Проверяем все фигуры того же цвета
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    string piece = board[i, j];
                    if (!string.IsNullOrEmpty(piece) && char.IsUpper(piece[0]) == isWhite)
                    {
                        // Проверяем, может ли фигура защитить указанную клетку
                        if (IsValidMove(i, j, row, col))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Оценка безопасности хода
        /// </summary>
        private int EvaluateMoveSafety((int startRow, int startCol, int endRow, int endCol) move, bool isWhite)
        {
            int safetyScore = 0;
            string piece = board[move.startRow, move.startCol];
            bool isKing = char.ToUpper(piece[0]) == 'K';

            // Сохраняем состояние доски
            string[,] boardCopy = (string[,])board.Clone();
            bool[,] hasMovedCopy = (bool[,])hasMoved.Clone();

            // Делаем временный ход
            MakeTemporaryMove(move);

            // Проверяем, не находится ли фигура под атакой после хода
            if (IsSquareUnderAttack(move.endRow, move.endCol, isWhite))
            {
                // Если фигура под атакой, проверяем, защищена ли она
                if (!IsPieceProtected(move.endRow, move.endCol, isWhite))
                {
                    safetyScore -= 1000; // Сильный штраф за небезопасный ход
                }
                else
                {
                    safetyScore -= 100; // Меньший штраф за защищенную фигуру
                }
            }

            // Особый штраф за небезопасные ходы короля
            if (isKing)
            {
                // Проверяем все фигуры противника
                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        string attackingPiece = board[i, j];
                        if (!string.IsNullOrEmpty(attackingPiece) && char.IsUpper(attackingPiece[0]) != isWhite)
                        {
                            // Если фигура противника может атаковать короля
                            if (IsValidMove(i, j, move.endRow, move.endCol))
                            {
                                safetyScore -= 2000; // Очень сильный штраф за небезопасный ход короля
                            }
                        }
                    }
                }
            }

            // Восстанавливаем состояние доски
            RestoreBoard(boardCopy, hasMovedCopy);

            return safetyScore;
        }

        /// <summary>
        /// Метод для выполнения хода дебюта
        /// </summary>
        private bool TryOpeningMove()
        {
            // Проверяем, не превышен ли лимит ходов дебюта
            if (openingMoveCount >= 10)
                return false;

            // Получаем текущую позицию в виде строки
            string currentPosition = GetPositionKey();

            // Ищем подходящий дебют
            foreach (var opening in openings)
            {
                // Проверяем, совпадает ли текущая позиция с позицией в дебюте
                if (openingMoveCount < opening.Value.Count)
                {
                    var move = opening.Value[openingMoveCount];

                    // Проверяем, что фигура на месте и она черная
                    if (board[move.SrtS, move.SrtSt] == move.Pi && char.IsLower(move.Pi[0]))
                    {
                        // Проверяем, что ход возможен
                        if (IsValidMove(move.SrtS, move.SrtSt, move.ES, move.ESt))
                        {
                            // Делаем ход
                            MakeMove(move.SrtS, move.SrtSt, move.ES, move.ESt);
                            openingMoveCount++;
                            return true;
                        }
                    }
                }
            }

            // Если не нашли подходящий дебют, возвращаем false
            return false;
        }

        /// <summary>
        /// Получение списка возможных ходов
        /// </summary>
        private List<(int startRow, int startCol, int endRow, int endCol)> GetPossibleMoves(bool isMaximizing)
        {
            var moves = new List<(int startRow, int startCol, int endRow, int endCol)>();
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    string piece = board[i, j];
                    if (!string.IsNullOrEmpty(piece) && char.IsLower(piece[0]) == !isMaximizing)
                    {
                        for (int k = 0; k < 8; k++)
                        {
                            for (int l = 0; l < 8; l++)
                            {
                                if (IsValidMove(i, j, k, l))
                                {
                                    moves.Add((startRow: i, startCol: j, endRow: k, endCol: l));
                                }
                            }
                        }
                    }
                }
            }
            return moves;
        }

        /// <summary>
        /// Метод минимакс для оценки позиции
        /// </summary>
        private int Minimax(int depth, bool isMaximizing, int alpha, int beta)
        {
            // Проверяем кэш
            string positionKey = GetPositionKey();
            if (positionCache.TryGetValue(positionKey, out int cachedScore))
            {
                cacheHits++;
                return cachedScore;
            }

            // Проверяем состояние доски
            var state = BoardStateCheck(isMaximizing, false);
            if (state.isCheckmate)
            {
                return isMaximizing ? int.MinValue + 1 : int.MaxValue - 1;
            }

            // Достигли максимальной глубины или конец игры
            if (depth == 0 || IsGameOver(false))
            {
                int score = EvaluatePosition();
                positionCache[positionKey] = score;
                totalEvaluations++;
                return score;
            }

            // Получаем список возможных ходов
            var moves = GetPossibleMoves(isMaximizing);

            if (moves.Count == 0)
            {
                int score = EvaluatePosition();
                positionCache[positionKey] = score;
                return score;
            }

            if (isMaximizing)
            {
                int maxScore = int.MinValue;
                foreach (var move in moves)
                {
                    string[,] boardCopy = (string[,])board.Clone();
                    bool[,] hasMovedCopy = (bool[,])hasMoved.Clone();

                    MakeTemporaryMove(move);

                    if (!IsKingInCheck(true))
                    {
                        int score = Minimax(depth - 1, false, alpha, beta);
                        maxScore = Math.Max(maxScore, score);
                        alpha = Math.Max(alpha, score);
                        if (beta <= alpha)
                        {
                            RestoreBoard(boardCopy, hasMovedCopy);
                            break;
                        }
                    }

                    RestoreBoard(boardCopy, hasMovedCopy);
                }
                positionCache[positionKey] = maxScore;
                return maxScore;
            }
            else
            {
                int minScore = int.MaxValue;
                foreach (var move in moves)
                {
                    string[,] boardCopy = (string[,])board.Clone();
                    bool[,] hasMovedCopy = (bool[,])hasMoved.Clone();

                    MakeTemporaryMove(move);

                    if (!IsKingInCheck(false))
                    {
                        int score = Minimax(depth - 1, true, alpha, beta);
                        minScore = Math.Min(minScore, score);
                        beta = Math.Min(beta, score);
                        if (beta <= alpha)
                        {
                            RestoreBoard(boardCopy, hasMovedCopy);
                            break;
                        }
                    }

                    RestoreBoard(boardCopy, hasMovedCopy);
                }
                positionCache[positionKey] = minScore;
                return minScore;
            }
        }

        /// Метод оценки текущей позиции на доске
        private int EvaluatePosition()
        {
            int score = 0;
            int piecesLeft = CountPieces();
            bool isEndgame = piecesLeft < 10;
            bool isOpening = openingMoveCount < 10;

            // Материальная оценка
            for (int z = 0; z < 64; z++)
            {
                int i = z / 8;
                int j = z % 8;

                if (!string.IsNullOrEmpty(board[i, j]))
                {
                    char piece = char.ToUpper(board[i, j][0]);
                    int value = pieceValues[piece];
                    bool isBlack = char.IsLower(board[i, j][0]);
                    bool isWhite = !isBlack;

                    // Для чёрных (бота) все штрафы должны увеличивать value, бонусы - уменьшать
                    int modifier = isBlack ? -1 : 1;

                    // Оценка безопасности короля
                    if (piece == 'K')
                    {
                        // Штраф за открытого короля (одинаковый для обеих сторон)
                        if (!IsKingProtected(i, j, isWhite))
                        {
                            value -= 200 * modifier;
                        }

                        // Штраф за короля в центре в дебюте
                        if (isOpening && i >= 2 && i <= 5 && j >= 2 && j <= 5)
                        {
                            value -= 150 * modifier;
                        }

                        // Бонус за рокировку
                        if (hasMoved[i, j] && (j == 2 || j == 6))
                        {
                            value += 100 * modifier;
                        }
                    }

                    // Оценка для дебюта
                    if (isOpening)
                    {
                        // Бонус за контроль центра
                        if (i >= 2 && i <= 5 && j >= 2 && j <= 5)
                        {
                            value += 20 * modifier;
                        }

                        // Бонус за развитие фигур
                        if (piece == 'N' || piece == 'B')
                        {
                            if (hasMoved[i, j])
                            {
                                value += 30 * modifier;
                            }
                            // Штраф за фигуры на краю
                            if (i == 0 || i == 7 || j == 0 || j == 7)
                            {
                                value -= 35 * modifier;
                            }
                        }

                        // Оценка пешек
                        if (piece == 'P')
                        {
                            // Бонус за центральные пешки
                            if ((j == 3 || j == 4) && (i >= 2 && i <= 5))
                            {
                                value += 25 * modifier;
                            }

                            // Штраф за изолированные пешки
                            bool isIsolated = true;
                            for (int col = Math.Max(0, j - 1); col <= Math.Min(7, j + 1); col++)
                            {
                                if (col != j && board[i, col] == (isWhite ? "P" : "p"))
                                {
                                    isIsolated = false;
                                    break;
                                }
                            }
                            if (isIsolated)
                            {
                                value -= 30 * modifier;
                            }
                        }
                    }

                    // Оценка для эндшпиля
                    if (isEndgame)
                    {
                        if (piece == 'P')
                        {
                            // Бонус за продвинутые пешки
                            int progressBonus = isBlack ? i : 7 - i;
                            value += progressBonus * 10 * modifier;
                        }
                        if (piece == 'K')
                        {
                            // Бонус за активного короля в эндшпиле
                            double centerDistance = Math.Abs(3.5 - i) + Math.Abs(3.5 - j);
                            value += (int)((14 - centerDistance) * 10) * modifier;
                        }
                    }

                    // Итоговый подсчет
                    if (isBlack)
                        score -= value; // Для чёрных вычитаем value (так как бот минимизирует оценку)
                    else
                        score += value;
                }
            }

            // Проверка шаха и мата
            bool whiteInCheck = IsKingInCheck(true);
            bool blackInCheck = IsKingInCheck(false);
            bool whiteCheckmate = whiteInCheck && IsCheckmate(true);
            bool blackCheckmate = blackInCheck && IsCheckmate(false);

            if (whiteCheckmate)
            {
                score += 1000000; // Мат белым - очень хорошо для чёрных (увеличиваем оценку)
            }
            else if (blackCheckmate)
            {
                score -= 1000000; // Мат чёрным - очень плохо (уменьшаем оценку)
            }
            else if (whiteInCheck)
            {
                score += 100; // Шах белым - хорошо для чёрных
            }
            else if (blackInCheck)
            {
                score -= 100; // Шах чёрным - плохо
            }

            return score;
        }

        // Добавляем новый метод для проверки защиты короля
        private bool IsKingProtected(int row, int col, bool isWhite)
        {
            // Проверяем соседние клетки на наличие своих фигур
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i == 0 && j == 0) continue;

                    int newRow = row + i;
                    int newCol = col + j;

                    if (newRow >= 0 && newRow < 8 && newCol >= 0 && newCol < 8)
                    {
                        string piece = board[newRow, newCol];
                        if (!string.IsNullOrEmpty(piece) && char.IsUpper(piece[0]) == isWhite)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        // Оптимизировать
        private int CountPieces()
        {
            int count = 0;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (!string.IsNullOrEmpty(board[i, j]))
                        count++;
                }
            }
            return count;
        }

        // Добавить метод для сохранения и создания возможных ходов в JSON




        /// <summary>
        /// Сохранение настроек в JSON
        /// </summary>
        private void SaveSettings()
        {
            try
            {
                var settings = new GameSettings
                {
                    SearchDepth = searchDepth,
                    IsBotEnabled = isBotEnabled,
                    Board = board,
                    IsWhiteTurn = isWhiteTurn,
                    HasMoved = hasMoved,
                    LastPawnMove = lastPawnMove
                };

                var serializer = new JavaScriptSerializer();
                string json = serializer.Serialize(settings);
                File.WriteAllText("game_settings.json", json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении настроек: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// 
        /// Загрузка настроек из JSON 
        /// 
        private void LoadSettings()
        {
            try
            {
                if (File.Exists("game_settings.json"))
                {
                    string json = File.ReadAllText("game_settings.json");
                    var serializer = new JavaScriptSerializer();
                    var settings = serializer.Deserialize<GameSettings>(json);

                    searchDepth = settings.SearchDepth;
                    isBotEnabled = settings.IsBotEnabled;
                    board = settings.Board;
                    isWhiteTurn = settings.IsWhiteTurn;
                    hasMoved = settings.HasMoved;
                    lastPawnMove = settings.LastPawnMove;

                    // Обновляем интерфейс
                    UpdateStatusText();
                    DrawBoard();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке настроек: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                StartNewGame(); // Если не удалось загрузить настройки, начинаем новую игру
            }
        }

        /// <summary>
        /// Проверка, находится ли клетка под атакой
        /// </summary>
        private bool IsSquareUnderAttack(int row, int col, bool isWhite)
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {// Отсылка
                    string attackingPiece = board[i, j];
                    if (!string.IsNullOrEmpty(attackingPiece) && char.IsUpper(attackingPiece[0]) != isWhite)
                    {
                        if (IsValidMove(i, j, row, col))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Очистка кэша позиций
        /// </summary>
        private void ClearPositionCache()
        {
            positionCache.Clear();
            cacheHits = 0;
            totalEvaluations = 0;
        }

        /// Сохранение завершенной партии
        /// Загрузка сохраненной партии
        /// Отображение списка сохраненных партий
        /// Проверка наличия короля на доске
        
        private bool IsKingPresent(bool isWhite)
        {
            string king = isWhite ? "K" : "k";
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    if (board[row, col] == king)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void SaveGamesButton_Click(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// Комплексная проверка состояния доски
        /// </summary>
        private (bool isCheck, bool isCheckmate, bool isStalemate, bool isThreefoldRepetition) BoardStateCheck(bool isWhite, bool checkThreefold = true)
        {
            bool isCheck = IsKingInCheck(isWhite);
            bool isCheckmate = isCheck && IsCheckmate(isWhite);
            bool isStalemate = !isCheck && IsStalemate(isWhite);
            bool isThreefoldRepetition = checkThreefold && IsThreefoldRepetition();

            return (isCheck, isCheckmate, isStalemate, isThreefoldRepetition);
        }

        /// <summary>
        /// Комплексная проверка безопасности хода
        /// </summary>
        private (bool isSafe, int safetyScore) CheckMoveSafety((int startRow, int startCol, int endRow, int endCol) move, bool isWhite)
        {
            int safetyScore = 0;

            // Проверяем границы массива
            if (move.startRow < 0 || move.startRow >= 8 || move.startCol < 0 || move.startCol >= 8 ||
                move.endRow < 0 || move.endRow >= 8 || move.endCol < 0 || move.endCol >= 8)
            {
                return (false, -1000);
            }

            string piece = board[move.startRow, move.startCol];
            if (string.IsNullOrEmpty(piece))
            {
                return (false, -1000);
            }

            bool isKing = char.ToUpper(piece[0]) == 'K';

            // Сохраняем состояние доски
            string[,] boardCopy = (string[,])board.Clone();
            bool[,] hasMovedCopy = (bool[,])hasMoved.Clone();

            try
            {
                // Делаем временный ход
                MakeTemporaryMove(move);

                // Проверяем состояние после хода
                var state = BoardStateCheck(isWhite, false);
                if (state.isCheck)
                {
                    safetyScore -= 1000;
                }

                // Проверяем, не находится ли фигура под атакой
                if (IsSquareUnderAttack(move.endRow, move.endCol, isWhite))
                {
                    if (!IsPieceProtected(move.endRow, move.endCol, isWhite))
                    {
                        safetyScore -= 1000;
                    }
                    else
                    {
                        safetyScore -= 100;
                    }
                }

                // Особый штраф за небезопасные ходы короля
                if (isKing)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            string attackingPiece = board[i, j];
                            if (!string.IsNullOrEmpty(attackingPiece) && char.IsUpper(attackingPiece[0]) != isWhite)
                            {
                                if (IsValidMove(i, j, move.endRow, move.endCol))
                                {
                                    safetyScore -= 2000;
                                }
                            }
                        }
                    }
                }

                return (safetyScore > -1000, safetyScore);
            }
            finally
            {
                // Восстанавливаем состояние доски в любом случае
                RestoreBoard(boardCopy, hasMovedCopy);
            }
        }

        /// <summary>
        /// Комплексная проверка хода
        /// </summary>


        /// <summary>
        /// Поиск лучшего хода
        /// </summary>
        private (int startRow, int startCol, int endRow, int endCol)? FindBestMove()
        {
            if (TryOpeningMove()) return null;

            var moves = GetPossibleMoves(false);
            if (moves == null || moves.Count == 0) return null;

            int bestScore = int.MaxValue;
            (int startRow, int startCol, int endRow, int endCol)? bestMove = null;

            // Очищаем кэш перед поиском
            ClearPositionCache();

            // Сначала проверяем ходы, которые ведут к мату
            foreach (var move in moves)
            {
                string[,] boardCopy = (string[,])board.Clone();
                bool[,] hasMovedCopy = (bool[,])hasMoved.Clone();

                MakeTemporaryMove(move);

                if (!IsKingInCheck(false))
                {
                    if (IsKingInCheck(true) && IsCheckmate(true))
                    {
                        RestoreBoard(boardCopy, hasMovedCopy);
                        return move;
                    }
                }

                RestoreBoard(boardCopy, hasMovedCopy);
            }

            // Затем проверяем ходы, которые защищают короля
            foreach (var move in moves)
            {
                string[,] boardCopy = (string[,])board.Clone();
                bool[,] hasMovedCopy = (bool[,])hasMoved.Clone();

                MakeTemporaryMove(move);

                if (!IsKingInCheck(false))
                {
                    // Проверяем, защищает ли ход короля
                    bool protectsKing = false;
                    string piece = board[move.startRow, move.startCol];
                    if (!string.IsNullOrEmpty(piece) && char.ToUpper(piece[0]) == 'K')
                    {
                        protectsKing = IsKingProtected(move.endRow, move.endCol, false);
                    }

                    if (protectsKing)
                    {
                        int score = Minimax(searchDepth - 1, true, int.MinValue, int.MaxValue) - 500;
                        if (score < bestScore)
                        {
                            bestScore = score;
                            bestMove = move;
                        }
                    }
                }

                RestoreBoard(boardCopy, hasMovedCopy);
            }

            // Если не нашли защищающий ход, ищем лучший ход по обычной оценке
            if (bestMove == null)
            {
                foreach (var move in moves)
                {
                    string[,] boardCopy = (string[,])board.Clone();
                    bool[,] hasMovedCopy = (bool[,])hasMoved.Clone();

                    MakeTemporaryMove(move);

                    if (!IsKingInCheck(false))
                    {
                        int score = Minimax(searchDepth - 1, true, int.MinValue, int.MaxValue);

                        // Проверяем безопасность хода
                        var safety = CheckMoveSafety(move, false);
                        if (safety.isSafe)
                        {
                            score -= safety.safetyScore;
                        }
                        else
                        {
                            score += 1000;
                        }

                        if (score < bestScore)
                        {
                            bestScore = score;
                            bestMove = move;
                        }
                    }

                    RestoreBoard(boardCopy, hasMovedCopy);
                }
            }

            return bestMove;
        }

        /// <summary>
        /// Комплексная проверка состояния доски и хода
        /// </summary>
        private (bool isValid, string reason, int score) AnalyzeMove(int startRow, int startCol, int endRow, int endCol, bool isWhite)
        {
            // Базовые проверки
            if (startRow < 0 || startRow >= 8 || startCol < 0 || startCol >= 8 ||
                endRow < 0 || endRow >= 8 || endCol < 0 || endCol >= 8)
                return (false, "Ход за пределы доски", 0);

            string piece = board[startRow, startCol];
            if (string.IsNullOrEmpty(piece) || char.IsUpper(piece[0]) != isWhite)
                return (false, "Недопустимая фигура", 0);

            // Проверка правил хода
            bool isValid = false;
            switch (char.ToUpper(piece[0]))
            {
                case 'P':
                    isValid = IsValidPawnMove(startRow, startCol, endRow, endCol);
                    break;
                case 'R':
                    isValid = IsValidRookMove(startRow, startCol, endRow, endCol);
                    break;
                case 'N':
                    isValid = IsValidKnightMove(startRow, startCol, endRow, endCol);
                    break;
                case 'B':
                    isValid = IsValidBishopMove(startRow, startCol, endRow, endCol);
                    break;
                case 'Q':
                    isValid = IsValidQueenMove(startRow, startCol, endRow, endCol);
                    break;
                case 'K':
                    isValid = IsValidKingMove(startRow, startCol, endRow, endCol);
                    break;
            }

            if (!isValid)
                return (false, "Недопустимый ход", 0);

            // Сохраняем состояние
            string[,] boardCopy = (string[,])board.Clone();
            bool[,] hasMovedCopy = (bool[,])hasMoved.Clone();

            // Делаем ход
            MakeTemporaryMove((startRow, startCol, endRow, endCol));

            // Проверяем состояние после хода
            var state = BoardStateCheck(isWhite, false);
            int score = 0;

            if (state.isCheck)
            {
                RestoreBoard(boardCopy, hasMovedCopy);
                return (false, "Ход ставит короля под шах", 0);
            }

            // Оцениваем позицию
            score = EvaluatePosition();

            // Бонусы и штрафы
            if (state.isCheckmate) score += 1000;
            if (IsSquareUnderAttack(endRow, endCol, isWhite))
            {
                score -= IsPieceProtected(endRow, endCol, isWhite) ? 100 : 1000;
            }

            RestoreBoard(boardCopy, hasMovedCopy);
            return (true, "Ход допустим", score);
        }
        /// <summary>
        /// Обработчик нажатия кнопки "Сдаться"
        /// </summary>
        private void SurrenderButton_Click(object sender, RoutedEventArgs e)
        {
            if (isGameOver) return;

            string message = isWhiteTurn ? "Белые сдались! Победа чёрных!" : "Чёрные сдались! Победа белых!";
            isGameOver = true;
            UpdateStatusText();
            ShowGameOverDialog(message);
        }
    }
}
