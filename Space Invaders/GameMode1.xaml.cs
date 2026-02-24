using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Linq;
using System.IO;

namespace Space_Invaders
{
    public partial class GameMode1 : Window
    {
        public Player player = new Player();
        List<Image> ListHPImage = new List<Image>();
        private DispatcherTimer timer = new DispatcherTimer();
        private GroupEnemy groupEnemy = new GroupEnemy();
        
        // Поля менеджерів для закриття Issue #2 та #3
        private ScoreManager _scoreManager = new ScoreManager();
        private GameState _gameState = new GameState();

        public GameMode1()
        {
            InitializeComponent();
            SetImageHP();

            groupEnemy.MyCanvas = MyCanvas;
            groupEnemy.Score();

            MyCanvas.Children.Add(player.Image);
            Canvas.SetZIndex(player.Image, 5);

            // Використання константи з GameConfig (Issue #3)
            timer.Interval = TimeSpan.FromMilliseconds(GameConfig.Timing.GameTickMilliseconds);
            timer.Tick += Timer_Tick;
            timer.Start();

            // Використання позицій з GameConfig (Issue #3)
            Barrier barrier1 = new Barrier(GameConfig.Layout.Barrier1X, 300);
            Barrier barrier2 = new Barrier(GameConfig.Layout.Barrier2X, 300);
            barrier1.AddToCanvas(MyCanvas);
            barrier2.AddToCanvas(MyCanvas);

            var barriers = new List<Barrier> { barrier1, barrier2 };

            player.Barriers = barriers;
            groupEnemy.Barriers = barriers;

            groupEnemy.CreateEnemies();
            groupEnemy.StartEnemyMovement(player);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateHeartsUI();
            Move_Ship();
            player.CreateBullet(MyCanvas);
            groupEnemy.CheckBulletCollisions(player);
            player.BulletTick(MyCanvas);
        }

        // Issue #2: DRY Violation — тепер логіка в одному місці
        private void UpdateHeartsUI()
        {
            // Оновлення візуального відображення HP
            if (player.HP >= 0 && player.HP < ListHPImage.Count)
            {
                ListHPImage[player.HP].Visibility = Visibility.Hidden;
            }

            // Об'єднана перевірка: смерть або перемога (Issue #2)
            // Використання констант станів (Issue #3)
            if (player.HP <= GameConfig.States.DeadHP || player.HP == GameConfig.States.VictoryHP)
            {
                HandleGameOver(player.HP == GameConfig.States.VictoryHP);
            }
        }

        // Винесена спільна логіка завершення гри (Issue #2)
        private void HandleGameOver(bool isVictory)
        {
            timer.Stop();
            
            // Збереження результату через окремий менеджер (SRP)
            _scoreManager.SaveScore(MyCanvas);
            
            this.Hide();
            // Створення вікна завершення з відповідним результатом
            EndWindow endWindow = new EndWindow(isVictory);
            endWindow.ShowDialog();
            
            this.Close();
        }

        private void SetImageHP()
        {
            for (int i = 0; i < player.MaxHP; i++)
            {
                Image HPImage = new Image
                {
                    Width = GameConfig.Sizes.HeartSize,
                    Height = GameConfig.Sizes.HeartSize,
                    Source = new BitmapImage(new Uri("Images/Heart.png", UriKind.Relative))
                };
                Canvas.SetLeft(HPImage, i * HPImage.Width + 5);
                Canvas.SetTop(HPImage, 5);
                Canvas.SetZIndex(HPImage, 99);
                MyCanvas.Children.Add(HPImage);
                ListHPImage.Add(HPImage);
            }
        }

        private void Move_Ship()
        {
            double left = Canvas.GetLeft(player.Image);
            if (double.IsNaN(left))
                left = (this.ActualWidth - player.Image.ActualWidth) / 2;

            double minX = 0;
            double maxX = this.ActualWidth - player.Image.ActualWidth;
            double step = player.Speed;

            if (player.FlyLeft && left > minX)
                left -= step;

            if (player.FlyRight && left < maxX)
                left += step;

            Canvas.SetLeft(player.Image, left);

            double bottomY = this.ActualHeight - player.Image.ActualHeight - GameConfig.Layout.PlayerBottomOffset;
            Canvas.SetTop(player.Image, bottomY);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            double centerX = (this.ActualWidth - player.Image.ActualWidth) / 2;
            Canvas.SetLeft(player.Image, centerX);

            double bottomPosition = this.ActualHeight - player.Image.ActualHeight;
            Canvas.SetTop(player.Image, bottomPosition);
        }

        private void KeyIsDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.A: player.FlyLeft = true; break;
                case Key.D: player.FlyRight = true; break;
                case Key.Space: player.ButtonShoot = true; break;
                case Key.Escape: this.Close(); break;
            }
        }

        private void KeyIsUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.A: player.FlyLeft = false; break;
                case Key.D: player.FlyRight = false; break;
                case Key.Space: player.ButtonShoot = false; break;
                case Key.Escape: this.Close(); break;
            }
        }
    }
}
