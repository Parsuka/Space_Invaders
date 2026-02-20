using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Space_Invaders
{
    public class Player : Entity
    {
        public int MaxHP { get; } = 3;
        public int HP { get; set; } = 3;
        public bool FlyLeft = false, FlyRight = false;
        public bool ButtonShoot = false;


        public List<Bullet> bullets = new List<Bullet>();
        public List<Barrier> Barriers { get; set; } = new List<Barrier>();
        public double lastShotTime { get; set; } = 0;
        public double CooldownSeconds { get; } = 0.5;
        public double Speed { get; }

        public Player()
        {
            Image = new Image
            {
                Width = 50,
                Height = 50,
                Source = new BitmapImage(new Uri(File.ReadAllLines(Directory.GetCurrentDirectory() + "/Settings.txt")[1].Trim(), UriKind.Absolute))
            };

            HitBox = new Rectangle
            {
                Width = Image.Width,
                Height = Image.Height,
                Stroke = Brushes.Cyan, 
                StrokeThickness = 1,
                Fill = Brushes.Transparent
            };

            Speed = 8;
        }

        public void AddToCanvas(Canvas MyCanvas, double startX, double startY)
        {
            Canvas.SetLeft(Image, startX);
            Canvas.SetTop(Image, startY);
            Canvas.SetLeft(HitBox, startX);
            Canvas.SetTop(HitBox, startY);

            MyCanvas.Children.Add(Image);
            MyCanvas.Children.Add(HitBox);
        }

        public void UpdatePosition(double dx, Canvas MyCanvas)
        {
            double newX = Canvas.GetLeft(Image) + dx;
            Canvas.SetLeft(Image, newX);
            Canvas.SetLeft(HitBox, newX);
        }

        public void CreateBullet(Canvas MyCanvas)
        {
            if (ButtonShoot && Environment.TickCount / 1000.0 - lastShotTime >= CooldownSeconds)
            {
                double XBulletPositions = Canvas.GetLeft(Image) + Image.Width / 2 - 1.5;
                double YBulletPositions = Canvas.GetTop(Image);
                Bullet bullet = new Bullet(XBulletPositions, YBulletPositions);

                Point bulletStart = new Point(
                    Canvas.GetLeft(Image) + Image.Width / 2 - bullet.HitBox.Width / 2,
                    Canvas.GetTop(Image) - bullet.HitBox.Height
                );

                Canvas.SetLeft(bullet.HitBox, bulletStart.X);
                Canvas.SetTop(bullet.HitBox, bulletStart.Y);

                bullets.Add(bullet);
                bullet.AddToCanvas(MyCanvas);

                lastShotTime = Environment.TickCount / 1000.0;
            }
        }

        public void BulletTick(Canvas MyCanvas)
        {
            for (int i = bullets.Count - 1; i >= 0; i--)
            {
                Bullet bullet = bullets[i];
                bullet.MoveUp();

                foreach (var barrier in Barriers)
                {
                    if (barrier.IsCollidingWith(bullet.HitBox))
                    {
                        bullet.Remove();
                        bullets.RemoveAt(i);
                        break;
                    }
                }

                if (bullet.Y < 0)
                {
                    bullet.Remove();
                    bullets.RemoveAt(i);
                }
            }
        }

    }
}
