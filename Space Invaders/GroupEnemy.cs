using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace Space_Invaders
{
    public class EnemyGroup
    {
        private readonly List<Enemy> enemies = new List<Enemy>();
        private readonly Canvas canvas;

        public IReadOnlyList<Enemy> Enemies => enemies.AsReadOnly();
        public int AliveCount => enemies.Count(e => e.IsAlive);

        public EnemyGroup(Canvas canvas)
        {
            this.canvas = canvas ?? throw new ArgumentNullException(nameof(canvas));
        }

        public void CreateEnemies(int rows, int columns, double startX, double startY, double spacingX, double spacingY)
        {
            Random random = new Random();
            enemies.Clear();

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    double x = startX + col * spacingX;
                    double y = startY + row * spacingY;

                    string imagePath = $"Images/invader{random.Next(1, 8)}.gif";
                    Enemy enemy = new Enemy(x, y, imagePath);
                    enemy.Score = GameConfig.Scoring.EnemyBaseScore * (rows - row);
                    
                    enemies.Add(enemy);
                    enemy.AddToCanvas(canvas);
                }
            }
        }

        public void RemoveEnemy(Enemy enemy)
        {
            if (enemies.Contains(enemy))
            {
                enemy.RemoveFromCanvas(canvas);
                enemies.Remove(enemy);
            }
        }

        public List<Enemy> GetBottomEnemiesInColumns()
        {
            return enemies
                .Where(e => e.IsAlive)
                .GroupBy(e => Math.Round(e.X / GameConfig.Layout.EnemySpacingX))
                .Select(group => group.OrderByDescending(e => e.Y).First())
                .ToList();
        }

        public void ClearAll()
        {
            foreach (var enemy in enemies.ToList())
            {
                RemoveEnemy(enemy);
            }
        }
    }
}
