using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Space_Invaders
{
    public class Barrier
    {
        public Rectangle Body { get; private set; }
        public double X { get; set; }
        public double Y { get; set; }

        public Barrier(double x, double y)
        {
            X = x;
            Y = y;
            Body = new Rectangle
            {
                Width = 60,
                Height = 30,
                Fill = Brushes.LightGreen
            };

            Canvas.SetLeft(Body, X);
            Canvas.SetTop(Body, Y);
        }

        public void AddToCanvas(Canvas canvas)
        {
            canvas.Children.Add(Body);
        }

        public void RemoveFromCanvas(Canvas canvas)
        {
            canvas.Children.Remove(Body);
        }

        public bool IsCollidingWith(UIElement element)
        {
            if (element == null) return false;

            Rect barrierRect = new Rect(
                Canvas.GetLeft(Body),
                Canvas.GetTop(Body),
                Body.Width,
                Body.Height
            );

            Rect elementRect = new Rect(
                Canvas.GetLeft(element),
                Canvas.GetTop(element),
                ((FrameworkElement)element).ActualWidth,
                ((FrameworkElement)element).ActualHeight
            );

            return barrierRect.IntersectsWith(elementRect);
        }
    }
}