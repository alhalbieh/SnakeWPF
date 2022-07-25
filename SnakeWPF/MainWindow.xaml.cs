using Ellipse = System.Windows.Shapes.Ellipse;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace SnakeWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        public enum SnakeDirection
        { Left, Right, Up, Down };

        public event PropertyChangedEventHandler? PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private readonly Random random = new();
        private readonly DispatcherTimer gameTickTimer = new();
        //private readonly SpeechSynthesizer speechSynthesizer = new();

        private const int SnakeSquareSize = 20;
        private const int SnakeStartLength = 3;
        private const int SnakeStartSpeed = 400;
        private const int SnakeSpeedThreshold = 100;
        private const int MaxHighScoreListEntryCount = 5;

        private readonly SolidColorBrush foodBrush = Brushes.Red;

        private readonly List<SnakePart> snakeParts = new();
        private SnakeDirection snakeDirection = SnakeDirection.Right;
        private int snakeLength;
        private int currentScore;

        public int CurrentScore
        {
            get { return currentScore; }
            set
            {
                if (currentScore != value)
                {
                    currentScore = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public TimeSpan TickerInterval
        {
            get { return gameTickTimer.Interval; }
            set { gameTickTimer.Interval = value; NotifyPropertyChanged(); }
        }

        private UIElement? snakeFood = null;
        public ObservableCollection<HighScore> HighScoreList { get; set; } = new();

        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
            gameTickTimer.Tick += GameTickTime_Tick;
            Utilities.LoadList("highscorelist.xml", HighScoreList);
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            //DrawGameArea();
        }

        private void GameTickTime_Tick(object? sender, EventArgs e)
        {
            MoveSnake();
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            SnakeDirection originalSnakeDirection = snakeDirection;
            switch (e.Key)
            {
                case Key.Up:
                    if (snakeDirection != SnakeDirection.Down)
                        snakeDirection = SnakeDirection.Up;
                    break;

                case Key.Down:
                    if (snakeDirection != SnakeDirection.Up)
                        snakeDirection = SnakeDirection.Down;
                    break;

                case Key.Left:
                    if (snakeDirection != SnakeDirection.Right)
                        snakeDirection = SnakeDirection.Left;
                    break;

                case Key.Right:
                    if (snakeDirection != SnakeDirection.Left)
                        snakeDirection = SnakeDirection.Right;
                    break;

                case Key.Space:
                    StartNewGame();
                    break;

                default:
                    break;
            }
            if (snakeDirection != originalSnakeDirection)
            {
                MoveSnake();
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnShowHighscoreList_Click(object sender, RoutedEventArgs e)
        {
            bdrWelcomeMessage.Visibility = Visibility.Collapsed;
            bdrHighScoreList.Visibility = Visibility.Visible;
        }

        private void BtnAddToHighscoreList_Click(object sender, RoutedEventArgs e)
        {
            var highScore = HighScoreList.Select((hs, index) => new { hs, index }).FirstOrDefault(x => CurrentScore >= x.hs.Score);
            int ind = highScore?.index ?? HighScoreList.Count;
            for (int i = ind; i < HighScoreList.Count; i++)
            {
                if (HighScoreList[i].Score == CurrentScore)
                    ind++;
                else
                    break;
            }
            HighScoreList.Insert(ind, new HighScore() { Name = txtPlayerName.Text, Score = CurrentScore });
            if (HighScoreList.Count > MaxHighScoreListEntryCount)
                HighScoreList.RemoveAt(HighScoreList.Count - 1);

            Utilities.SaveList("highscorelist.xml", HighScoreList);

            bdrNewHighscore.Visibility = Visibility.Collapsed;
            bdrHighScoreList.Visibility = Visibility.Visible;
        }

        /*private void DrawGameArea()
        {
            bool doneDrawingBackground = false;
            int nextX = 0, nextY = 0;
            int rowCounter = 0;
            bool nextIsOdd = false;

            while (doneDrawingBackground == false)
            {
                Rectangle rect = new()
                {
                    Width = SnakeSquareSize,
                    Height = SnakeSquareSize,
                    Fill = nextIsOdd ? Brushes.White : Brushes.Black,
                };
                GameArea.Children.Add(rect);
                Canvas.SetTop(rect, nextY);
                Canvas.SetLeft(rect, nextX);

                nextIsOdd = !nextIsOdd;
                nextX += SnakeSquareSize;
                if (nextX >= GameArea.ActualWidth)
                {
                    nextX = 0;
                    nextY += SnakeSquareSize;
                    rowCounter++;
                    nextIsOdd = (rowCounter % 2 != 0);
                }

                if (nextY > GameArea.ActualHeight)
                {
                    doneDrawingBackground = true;
                }
            }
            {
            }
        }*/

        private void DrawSnake()
        {
            foreach (SnakePart snakePart in snakeParts)
            {
                if (snakePart.UiElement == null)
                {
                    snakePart.UiElement = new Rectangle()
                    {
                        Width = SnakeSquareSize,
                        Height = SnakeSquareSize,
                        Fill = (snakePart.IsHead ? snakePart.snakeHeadBrush : snakePart.snakeBodyBrush)
                    };
                    GameArea.Children.Add(snakePart.UiElement);
                    Canvas.SetTop(snakePart.UiElement, snakePart.Position.Y);
                    Canvas.SetLeft(snakePart.UiElement, snakePart.Position.X);
                }
            }
        }

        private void MoveSnake()
        {
            while (snakeParts.Count >= snakeLength)
            {
                GameArea.Children.Remove(snakeParts[0].UiElement);
                snakeParts.RemoveAt(0);
            }

            foreach (SnakePart snakePart in snakeParts)
            {
                if ((Rectangle?)snakePart.UiElement != null)
                    ((Rectangle)snakePart.UiElement).Fill = snakePart.snakeBodyBrush;
                snakePart.IsHead = false;
            }

            SnakePart snakeHead = snakeParts.Last();
            double nextX = snakeHead.Position.X;
            double nextY = snakeHead.Position.Y;

            switch (snakeDirection)
            {
                case SnakeDirection.Left:
                    nextX -= SnakeSquareSize;
                    break;

                case SnakeDirection.Right:
                    nextX += SnakeSquareSize;
                    break;

                case SnakeDirection.Up:
                    nextY -= SnakeSquareSize;
                    break;

                case SnakeDirection.Down:
                    nextY += SnakeSquareSize;
                    break;
            }

            snakeParts.Add(new SnakePart()
            {
                Position = new Point(nextX, nextY),
                IsHead = true
            });

            DrawSnake();
            DoCollisionCheck();
        }

        private void DoCollisionCheck()
        {
            SnakePart snakeHead = snakeParts.Last();
            double snakeHeadX = snakeHead.Position.X;
            double snakeHeadY = snakeHead.Position.Y;

            double snakeFoodX = Canvas.GetLeft(snakeFood);
            double snakeFoodY = Canvas.GetTop(snakeFood);
            if (snakeHeadX == snakeFoodX && snakeHeadY == snakeFoodY) EatSnakeFood();
            else if (snakeHeadX >= GameArea.ActualWidth || snakeHeadY >= GameArea.ActualHeight || snakeHeadY < 0 || snakeHeadX < 0) EndGame();
            else if (snakeParts.Take(snakeParts.Count - 1).Any(part => part.Position.X == snakeHeadX && part.Position.Y == snakeHeadY)) EndGame();
        }

        private Point GetNextFoodPosition()
        {
            int maxX = (int)(GameArea.ActualWidth / SnakeSquareSize);
            int maxY = (int)(GameArea.ActualHeight / SnakeSquareSize);
            int foodX = random.Next(0, maxX) * SnakeSquareSize;
            int foodY = random.Next(0, maxY) * SnakeSquareSize;

            foreach (SnakePart snakePart in snakeParts)
            {
                if (snakePart.Position.X == foodX && snakePart.Position.Y == foodY)
                {
                    return GetNextFoodPosition();
                }
            }
            return new Point(foodX, foodY);
        }

        private void DrawSnakeFood()
        {
            Point foodPosition = GetNextFoodPosition();
            snakeFood = new Ellipse()
            {
                Width = SnakeSquareSize,
                Height = SnakeSquareSize,
                Fill = foodBrush
            };

            GameArea.Children.Add(snakeFood);
            Canvas.SetTop(snakeFood, foodPosition.Y);
            Canvas.SetLeft(snakeFood, foodPosition.X);
        }

        private void EatSnakeFood()
        {
            snakeLength++;
            CurrentScore++;
            int timerInterval = Math.Max(SnakeSpeedThreshold, (int)TickerInterval.TotalMilliseconds - (CurrentScore * 2));
            TickerInterval = TimeSpan.FromMilliseconds(timerInterval);
            GameArea.Children.Remove(snakeFood);
            DrawSnakeFood();
        }

        private void StartNewGame()
        {
            bdrWelcomeMessage.Visibility = Visibility.Collapsed;
            bdrHighScoreList.Visibility = Visibility.Collapsed;
            bdrEndOfGame.Visibility = Visibility.Collapsed;

            foreach (SnakePart snakeBodyPart in snakeParts)
            {
                if (snakeBodyPart.UiElement != null)
                    GameArea.Children.Remove(snakeBodyPart.UiElement);
            }
            snakeParts.Clear();

            if (snakeFood != null) //Or GameArea.Children.Contains(snakeFood)
            {
                GameArea.Children.Remove(snakeFood);
            }
            CurrentScore = 0;
            snakeLength = SnakeStartLength;
            snakeDirection = SnakeDirection.Right;
            snakeParts.Add(new SnakePart() { Position = new Point(SnakeSquareSize * 5, SnakeSquareSize * 5) });
            TickerInterval = TimeSpan.FromMilliseconds(SnakeStartSpeed);

            DrawSnake();
            DrawSnakeFood();

            gameTickTimer.IsEnabled = true;
        }

        private void EndGame()
        {
            if (CurrentScore > 0)
            {
                if (HighScoreList.Any(x => CurrentScore > x.Score) || HighScoreList.Count < MaxHighScoreListEntryCount)
                {
                    bdrNewHighscore.Visibility = Visibility.Visible;
                    txtPlayerName.Focus();
                }
            }
            else
            {
                bdrEndOfGame.Visibility = Visibility.Visible;
            }
            gameTickTimer.IsEnabled = false;
            bdrEndOfGame.Visibility = Visibility.Visible;
        }
    }
}