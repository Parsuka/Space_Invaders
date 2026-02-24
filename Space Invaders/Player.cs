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
    // Було public bool FlyLeft; — стало:
    private bool _isMovingLeft;
    private bool _isMovingRight;
    private readonly List<Bullet> _bullets = new List<Bullet>();

    // Доступ тільки для читання
    public IReadOnlyList<Bullet> ActiveBullets => _bullets.AsReadOnly();

    public void MoveLeft(bool state) => _isMovingLeft = state;
    public void MoveRight(bool state) => _isMovingRight = state;

    // Використання констант замість чисел
    public Player()
    {
        // Замість Width = 50;
        Image.Width = GameConfig.Sizes.PlayerWidth;
        Image.Height = GameConfig.Sizes.PlayerHeight;
    }

}
