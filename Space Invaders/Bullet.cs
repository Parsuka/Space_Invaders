using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Space_Invaders
{
    public class Bullet
    {

        public Rectangle HitBox { get; }

        public double X { get; private set; }
        public double Y { get; private set; }
        public double Speed { get; set; } = 20;

        public const double Width = 3;
        public const double Height = 10;
       
        public Bullet(double startX, double startY, bool? enemy = false)
        {
            X = startX;
            Y = startY;

            HitBox = new Rectangle
            {
                Width = Width,
                Height = Height,
                Fill = enemy.HasValue && enemy.Value ? Brushes.Red : Brushes.Green

            };

            Canvas.SetLeft(HitBox, X);
            Canvas.SetTop(HitBox, Y);


        }
        public void AddToCanvas(Canvas gameCanvas)
        {
            gameCanvas.Children.Add(HitBox);

        }

        public void Move()
        {
            Y -= Speed;
            Canvas.SetLeft(HitBox, X);
            Canvas.SetTop(HitBox, Y);
        }

        public void RemoveFromCanvas(Canvas gameCanvas)
        {

            gameCanvas.Children.Remove(HitBox);
        }
        public void MoveUp()
        {
            double currentY = Canvas.GetTop(HitBox);
            Canvas.SetTop(HitBox, currentY - Speed);
            Y = currentY - Speed;
        }

        public void Remove()
        {
            if (HitBox != null && HitBox.Parent is Canvas canvas)
            {
                canvas.Children.Remove(HitBox);
            }
        }
    }
}
