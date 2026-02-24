namespace Space_Invaders
{
    public static class GameConfig
    {
        public static class Sizes
        {
            public const double PlayerWidth = 50;
            public const double PlayerHeight = 50;
            public const double EnemyWidth = 28;
            public const double EnemyHeight = 28;
            public const double BulletOffset = 1.5;
        }

        public static class Speeds
        {
            public const double PlayerSpeed = 8;
            public const double EnemySpeed = 20;
            public const double BulletSpeed = 12;
        }

        public static class Layout
        {
            public const double EnemyStartX = 50;
            public const double EnemyStartY = 50;
            public const double EnemySpacingX = 45;
            public const double EnemySpacingY = 35;
        }

        public static class States
        {
            public const int VictoryHP = -99;
            public const int DeadHP = 0;
        }
    }
}
