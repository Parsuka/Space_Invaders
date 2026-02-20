using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Space_Invaders
{
    public class Enemy : Entity
    {

        public int Score { get; set; }
        public Enemy()
        {

            Image = new Image
            {
                Width = Width,
                Height = Height,

            };
            Canvas.SetLeft(Image, X);
            Canvas.SetTop(Image, Y);
        }
        public Enemy(double x, double y, string imagePath)
        {
            X = x;
            Y = y;

            Image = new Image
            {
                Width = Width,
                Height = Height,
                Source = new BitmapImage(new Uri(imagePath, UriKind.Relative))
            };
            Canvas.SetLeft(Image, X);
            Canvas.SetTop(Image, Y);

            HitBox = new Rectangle
            {
                Width = Width,
                Height = Height,
                Stroke = Brushes.Red,
                StrokeThickness = 1,
                Fill = Brushes.Transparent
            };
            Canvas.SetLeft(HitBox, X);
            Canvas.SetTop(HitBox, Y);
        }

        public void AddToCanvas(Canvas gameCanvas)
        {
            gameCanvas.Children.Add(Image);
            gameCanvas.Children.Add(HitBox);
        }

        public void Move(double dx, double dy)
        {
            X += dx;
            Y += dy;

            Canvas.SetLeft(Image, X);
            Canvas.SetTop(Image, Y);
            Canvas.SetLeft(HitBox, X);
            Canvas.SetTop(HitBox, Y);
        }
        public void SetPosition(double dx, double dy)
        {
            X = dx;
            Y = dy;

            Canvas.SetLeft(Image, X);
            Canvas.SetTop(Image, Y);
            Canvas.SetLeft(HitBox, X);
            Canvas.SetTop(HitBox, Y);
        }
        public void RemoveFromCanvas(Canvas gameCanvas)
        {
            if (IsAlive)
            {
                IsAlive = false;
                gameCanvas.Children.Remove(Image);
                gameCanvas.Children.Remove(HitBox);
            }
        }


        public bool IsHitBy(Rectangle bulletHitBox)
        {
            Rect enemyRect = new Rect(Canvas.GetLeft(HitBox), Canvas.GetTop(HitBox), HitBox.Width, HitBox.Height);
            Rect bulletRect = new Rect(Canvas.GetLeft(bulletHitBox), Canvas.GetTop(bulletHitBox), bulletHitBox.Width, bulletHitBox.Height);

            return enemyRect.IntersectsWith(bulletRect);
        }

    }
}
