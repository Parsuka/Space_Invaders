using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Space_Invaders
{
    internal class GroupEnemy
    {
        public List<Barrier> Barriers { get; set; } = new List<Barrier>();

        TextBlock scoreText;
        public Canvas MyCanvas;
        private List<Enemy> enemies = new List<Enemy>();
        public bool Hard = false;

        private DispatcherTimer enemyTimer;
        private bool movingRight = true;
        private double enemySpeed = 20;
        private double enemyMoveDown = new Enemy().Image.Height + 20;

        public int Wave = 1;
        public double EnemySpeedMultiplier = 1.0;

        public void CreateBarriers()
        {
          
            Barrier barrier1 = new Barrier(100, MyCanvas.ActualHeight - 150);
            Barrier barrier2 = new Barrier(400, MyCanvas.ActualHeight - 150);

            barrier1.AddToCanvas(MyCanvas);
            barrier2.AddToCanvas(MyCanvas);

            Barriers.Add(barrier1);
            Barriers.Add(barrier2);

        }

        public void CreateEnemies(int rows = 4, int columns = 7)
        {
            double startX = 50;
            double startY = 50;
            double spacingX = 45;
            double spacingY = 35;
            Random random = new Random();

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    double x = startX + col * spacingX;
                    double y = startY + row * spacingY;

                    Enemy enemy = new Enemy(x, y, $"Images/invader{random.Next(1, 8)}.gif");
                    enemy.Score = 10 * (rows - row);
                    enemies.Add(enemy);
                    MyCanvas.Children.Add(enemy.Image);
                }
            }
        }


        public void StartEnemyMovement(Player player)
        {
            if (enemyTimer != null)
            {
                enemyTimer.Stop();
            }

            enemyTimer = new DispatcherTimer();
            enemyTimer.Interval = TimeSpan.FromMilliseconds(500);
            enemyTimer.Tick += MoveEnemies;
            enemyTimer.Start();

            Random random = new Random();

            enemyTimer.Tick += (s, e) =>
            {
                var shooters = GetCurrentShootingEnemies();
                if (shooters.Count <= 0)
                {
                    player.HP = -99;
                    return;
                }

                Enemy shooter = shooters[random.Next(0, shooters.Count)];
                if (shooter.IsAlive)
                {
                    EnemyShoot(shooter, player, Barriers);
                }
            };
        }

        private void EnemyShoot(Enemy enemy, Player player, List<Barrier> barriers)
        {
            Bullet bullet = new Bullet(enemy.X + 15, enemy.Y + 40, true);
            bullet.Speed = Hard ? 12 : 12;
            MyCanvas.Children.Add(bullet.HitBox);

            DispatcherTimer bulletTimer = new DispatcherTimer();
            bulletTimer.Interval = TimeSpan.FromMilliseconds(20);
            bulletTimer.Tick += (s, e) =>
            {
                double top = Canvas.GetTop(bullet.HitBox);
                Canvas.SetTop(bullet.HitBox, top + bullet.Speed);

                if (top > MyCanvas.ActualHeight)
                {
                    MyCanvas.Children.Remove(bullet.HitBox);
                    bulletTimer.Stop();
                    return;
                }

                foreach (var barrier in barriers)
                {
                    if (barrier.IsCollidingWith(bullet.HitBox))
                    {
                        MyCanvas.Children.Remove(bullet.HitBox);
                        bulletTimer.Stop();
                        return;
                    }
                }

                Rect bulletRect = new Rect(Canvas.GetLeft(bullet.HitBox), Canvas.GetTop(bullet.HitBox), bullet.HitBox.Width, bullet.HitBox.Height);
                Rect playerRect = new Rect(Canvas.GetLeft(player.Image), Canvas.GetTop(player.Image), player.Image.Width, player.Image.Height);

                if (bulletRect.IntersectsWith(playerRect))
                {
                    MyCanvas.Children.Remove(bullet.HitBox);
                    bulletTimer.Stop();
                    if (player.HP > 0)
                    {
                        player.HP -= 1;
                    }
                }
            };
            bulletTimer.Start();
        }

        public void MoveEnemies(object sender, EventArgs e)
        {
            if (!Hard)
                MoveEnemiesEasy();
            else
                MoveEnemiesHard();
        }
        public void MoveEnemiesEasy()
        {
            bool shouldMoveDown = false;

            foreach (var enemy in enemies)
            {
                if (!enemy.IsAlive) continue;

                double nextX = enemy.X + (movingRight ? enemySpeed * EnemySpeedMultiplier : -enemySpeed * EnemySpeedMultiplier);
                if (nextX < 0 || nextX + 40 > MyCanvas.ActualWidth)
                {
                    shouldMoveDown = true;
                    movingRight = !movingRight;
                    break;
                }
            }

            foreach (var enemy in enemies)
            {
                if (!enemy.IsAlive) continue;

                if (shouldMoveDown)
                    enemy.Move(0, enemyMoveDown);
                else
                    enemy.Move(movingRight ? enemySpeed * EnemySpeedMultiplier : -enemySpeed * EnemySpeedMultiplier, 0);
            }
        }
        public void MoveEnemiesHard()
        {
            bool shouldMoveDown = false;
            foreach (var enemy in enemies)
            {
                if (!enemy.IsAlive) continue;
                double nextX = enemy.X + (movingRight ? enemySpeed * 2 * EnemySpeedMultiplier : -enemySpeed * 2 * EnemySpeedMultiplier);
                if (nextX < -enemy.Image.Width || nextX - 10 > MyCanvas.ActualWidth)
                {
                    shouldMoveDown = true;
                    break;
                }
            }

            foreach (var enemy in enemies)
            {
                if (!enemy.IsAlive) continue;

                if (shouldMoveDown && enemy.X - 10 > MyCanvas.ActualWidth)
                {
                    enemy.Move(0, enemyMoveDown);
                    enemy.SetPosition(-enemy.Image.Width, enemy.Y);
                }

                else
                {
                    enemy.Move(movingRight ? enemySpeed * 2 * EnemySpeedMultiplier : -enemySpeed * 2 * EnemySpeedMultiplier, 0);
                }
            }
        }
        public void CheckBulletCollisions(Player player)
        {
            for (int i = player.bullets.Count - 1; i >= 0; i--)
            {
                Bullet bullet = player.bullets[i];

                for (int j = 0; j < enemies.Count; j++)
                {
                    if (enemies[j].IsAlive && enemies[j].IsHitBy(bullet.HitBox))
                    {
                        if (int.TryParse(scoreText.Text, out int currentScore))
                        {
                            currentScore += enemies[j].Score;
                            scoreText.Text = currentScore.ToString();
                            UpdateScorePosition();
                        }

                        MyCanvas.Children.Remove(bullet.HitBox);
                        enemies[j].RemoveFromCanvas(MyCanvas);
                        player.bullets.RemoveAt(i);
                        enemies.Remove(enemies[j]);
                        break;
                    }
                }
            }

            if (enemies.Count == 0)
            {
                Wave++;
                EnemySpeedMultiplier += 0.15;
                enemySpeed += 1.5;

                if (!Hard)
                {
                    CreateEnemies();
                }
                else
                {
                    CreateEnemies(1, 12); 
                }
                StartEnemyMovement(player);
            }

            CheckForBottomReached();
        }
        public void CheckForBottomReached()
        {
            foreach (var enemy in enemies)
            {
                if (!enemy.IsAlive) continue;

                if (Canvas.GetTop(enemy.Image) >= MyCanvas.ActualHeight - enemy.Image.ActualHeight - 40)
                {
                    EndGame();
                    return;
                }

                foreach (var barrier in Barriers)
                {
                    Rect enemyRect = new Rect(Canvas.GetLeft(enemy.Image), Canvas.GetTop(enemy.Image),
                        enemy.Image.Width, enemy.Image.Height);
                    Rect barrierRect = new Rect(Canvas.GetLeft(barrier.Body), Canvas.GetTop(barrier.Body),
                        barrier.Body.Width, barrier.Body.Height);

                    if (enemyRect.IntersectsWith(barrierRect))
                    {
                        EndGame();
                        return;
                    }
                }
            }
        }
        private void EndGame()
        {
            enemyTimer?.Stop();

            var gameWindow = Application.Current.Windows
                .OfType<Window>()
                .FirstOrDefault(w => w is GameMode1 || w is GameMode2);

            if (gameWindow != null)
            {
                if (gameWindow is GameMode1 gameMode1)
                {
                    gameMode1.player.HP = 0;
                }
                else if (gameWindow is GameMode2 gameMode2)
                {
                    gameMode2.player.HP = 0;
                }
            }
        }
        public void Score()
        {
            FontFamily pixelFont = new FontFamily(new Uri("pack://application:,,,/"), "./Fonts/#Press Start 2P");

            scoreText = new TextBlock
            {
                Text = "0",
                FontFamily = pixelFont,
                FontSize = 12,
                Foreground = Brushes.White,
                Tag = "ScoreText"
            };

            MyCanvas.Children.Add(scoreText);
            Canvas.SetTop(scoreText, 10);

            scoreText.Loaded += (s, e) => UpdateScorePosition();
            MyCanvas.SizeChanged += (s, e) => UpdateScorePosition();
        }
        private void UpdateScorePosition()
        {
            double rightMargin = 10;
            double x = MyCanvas.ActualWidth - scoreText.ActualWidth - rightMargin;
            Canvas.SetLeft(scoreText, x);
        }
        public void CheckForBarrierCollision()
        {
            foreach (var enemy in enemies.ToList())
            {
                if (!enemy.IsAlive) continue;

                foreach (var barrier in Barriers)
                {
                    if (barrier.IsCollidingWith(enemy.Image))
                    {
                        EndGame();
                        return;
                    }
                }
            }
        }
        public List<Enemy> GetCurrentShootingEnemies()
        {
            Dictionary<int, Enemy> columnShooters = new Dictionary<int, Enemy>();

            foreach (var enemy in enemies.Where(e => e.IsAlive))
            {
                int column = (int)Math.Round((enemy.X - 50) / 60.0);

                if (!columnShooters.ContainsKey(column) || enemy.Y > columnShooters[column].Y)
                {
                    columnShooters[column] = enemy;
                }
            }

            return columnShooters.Values.ToList();
        }
    }
}