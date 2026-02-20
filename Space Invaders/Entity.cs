using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Space_Invaders
{
    public class Entity
    {
        public Image Image { get; set; }
        public Rectangle HitBox;
        public double X { get; set; }
        public double Y { get; set; }
        public bool IsAlive { get; set; } = true;


        internal const double Width = 28;
        internal const double Height = 28;
    }
}
