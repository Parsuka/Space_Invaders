using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.IO;

namespace Space_Invaders
{
    /// <summary>
    /// Interaction logic for GameMode2.xaml
    /// </summary>
    public partial class GameMode2 : Window
    {

        public Player player = new Player();
        List<Image> ListHPImage = new List<Image>();
        private DispatcherTimer timer = new DispatcherTimer();
        private GroupEnemy groupEnemy = new GroupEnemy();


        public GameMode2()
        {
            InitializeComponent();
            SetImageHP();

            groupEnemy.MyCanvas = MyCanvas;
            groupEnemy.Score();
            groupEnemy.Hard = true; // Встановлюємо Hard в true для цього режиму

            MyCanvas.Children.Add(player.Image);
            Canvas.SetZIndex(player.Image, 5);

            // Створення бар’єрів
            Barrier barrier1 = new Barrier(200, 300);
            Barrier barrier2 = new Barrier(500, 300);
            barrier1.AddToCanvas(MyCanvas);
            barrier2.AddToCanvas(MyCanvas);

            var barriers = new List<Barrier> { barrier1, barrier2 };

            // Додаємо до гравця і до ворогів
            player.Barriers = barriers;
            groupEnemy.Barriers = barriers;

            timer.Interval = TimeSpan.FromMilliseconds(16);
            timer.Tick += Timer_Tick;
            timer.Start();

            groupEnemy.CreateEnemies(1, 1); // Створення ворогів для першої хвилі
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
        private void UpdateHeartsUI()
        {

            if (player.HP >= 0 && player.HP < ListHPImage.Count)
            {
                // Приховати останнє серце (або те, що відповідає HP)
                ListHPImage[player.HP].Visibility = Visibility.Hidden;
            }
            if (player.HP == 0)
            {
                var element = MyCanvas.Children
    .OfType<FrameworkElement>()
    .FirstOrDefault(e => e.Tag != null && e.Tag.ToString() == "ScoreText");

                if (element is TextBlock Socore)
                {
                    string filePath = Directory.GetCurrentDirectory() + "/TableRecords.txt";
                    string name = File.ReadAllLines(Directory.GetCurrentDirectory() + "/Settings.txt")[0].Trim();
                    File.AppendAllText(filePath, name + " " + Socore.Text + "\n");

                }
                this.Hide();
                EndWindow endWindow = new EndWindow(true);

                endWindow.ShowDialog();
                this.Show();

                timer.Stop();
                this.Close();
            }
            if (player.HP == -99)
            {
                var element = MyCanvas.Children
    .OfType<FrameworkElement>()
    .FirstOrDefault(e => e.Tag != null && e.Tag.ToString() == "ScoreText");

                if (element is TextBlock Socore)
                {
                    string filePath = Directory.GetCurrentDirectory() + "/TableRecords.txt";
                    string name = File.ReadAllLines(Directory.GetCurrentDirectory() + "/Settings.txt")[0].Trim();
                    File.AppendAllText(filePath, name + " " + Socore.Text + "\n");

                }
                this.Hide();
                EndWindow endWindow = new EndWindow(true);

                endWindow.ShowDialog();
                this.Show();

                timer.Stop();
                this.Close();
            }
        }

        private void SetImageHP()
        {

            for (int i = 0; i < player.MaxHP; i++)
            {
                Image HPImage = new Image
                {
                    Width = 30,
                    Height = 30,
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

            double bottomY = this.ActualHeight - player.Image.ActualHeight - 40;
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