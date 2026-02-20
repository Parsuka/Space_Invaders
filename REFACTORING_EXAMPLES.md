# Приклади рефакторингу для Space Invaders

## 📋 Зміст
1. [Issue #1: Розділення відповідальностей](#issue-1-розділення-відповідальностей)
2. [Issue #2: Усунення дублювання](#issue-2-усунення-дублювання)
3. [Issue #3: Константи та інкапсуляція](#issue-3-константи-та-інкапсуляція)

---

## Issue #1: Розділення відповідальностей

### Було (GroupEnemy.cs):
```csharp
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
    
    // Створення бар'єрів (не відноситься до ворогів!)
    public void CreateBarriers() { /* ... */ }
    
    // Створення ворогів
    public void CreateEnemies(int rows = 4, int columns = 7) { /* ... */ }
    
    // Управління рухом
    public void StartEnemyMovement(Player player) { /* ... */ }
    public void MoveEnemies(object sender, EventArgs e) { /* ... */ }
    public void MoveEnemiesEasy() { /* ... */ }
    public void MoveEnemiesHard() { /* ... */ }
    
    // Стрільба
    private void EnemyShoot(Enemy enemy, Player player, List<Barrier> barriers) { /* ... */ }
    
    // Колізії
    public void CheckBulletCollisions(Player player) { /* ... */ }
    
    // UI (рахунок)
    public void Score() { /* ... */ }
}
```

### Стало:

#### 1. EnemyGroup.cs - Управління групою ворогів
```csharp
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
        
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                double x = startX + col * spacingX;
                double y = startY + row * spacingY;
                
                Enemy enemy = new Enemy(x, y, $"Images/invader{random.Next(1, 8)}.gif");
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
}
```

#### 2. EnemyMovementController.cs - Управління рухом
```csharp
public interface IMovementStrategy
{
    void Move(List<Enemy> enemies, ref bool movingRight, double canvasWidth);
}

public class EasyMovementStrategy : IMovementStrategy
{
    public void Move(List<Enemy> enemies, ref bool movingRight, double canvasWidth)
    {
        bool shouldMoveDown = false;
        
        foreach (var enemy in enemies.Where(e => e.IsAlive))
        {
            double nextX = enemy.X + (movingRight ? GameConfig.Speeds.EnemySpeed : -GameConfig.Speeds.EnemySpeed);
            if (nextX < 0 || nextX + GameConfig.Sizes.EnemyWidth > canvasWidth)
            {
                shouldMoveDown = true;
                movingRight = !movingRight;
                break;
            }
        }
        
        foreach (var enemy in enemies.Where(e => e.IsAlive))
        {
            if (shouldMoveDown)
                enemy.Move(0, GameConfig.Sizes.EnemyMoveDown);
            else
                enemy.Move(movingRight ? GameConfig.Speeds.EnemySpeed : -GameConfig.Speeds.EnemySpeed, 0);
        }
    }
}

public class HardMovementStrategy : IMovementStrategy
{
    public void Move(List<Enemy> enemies, ref bool movingRight, double canvasWidth)
    {
        bool shouldMoveDown = enemies
            .Where(e => e.IsAlive)
            .Any(e => {
                double nextX = enemy.X + (movingRight ? GameConfig.Speeds.EnemySpeedHard : -GameConfig.Speeds.EnemySpeedHard);
                return nextX < -GameConfig.Sizes.EnemyWidth || nextX > canvasWidth;
            });
        
        foreach (var enemy in enemies.Where(e => e.IsAlive))
        {
            if (shouldMoveDown && enemy.X > canvasWidth)
            {
                enemy.Move(0, GameConfig.Sizes.EnemyMoveDown);
                enemy.SetPosition(-GameConfig.Sizes.EnemyWidth, enemy.Y);
            }
            else if (shouldMoveDown && enemy.X < -GameConfig.Sizes.EnemyWidth)
            {
                enemy.Move(0, GameConfig.Sizes.EnemyMoveDown);
                enemy.SetPosition(canvasWidth, enemy.Y);
            }
            else
            {
                double dx = movingRight ? GameConfig.Speeds.EnemySpeedHard : -GameConfig.Speeds.EnemySpeedHard;
                enemy.Move(dx, 0);
            }
        }
        
        if (shouldMoveDown)
            movingRight = !movingRight;
    }
}

public class EnemyMovementController
{
    private IMovementStrategy strategy;
    private DispatcherTimer movementTimer;
    private bool movingRight = true;
    
    public EnemyMovementController(bool isHardMode)
    {
        strategy = isHardMode ? new HardMovementStrategy() : new EasyMovementStrategy();
        
        movementTimer = new DispatcherTimer();
        movementTimer.Interval = TimeSpan.FromMilliseconds(GameConfig.Timing.EnemyMoveMilliseconds);
    }
    
    public void StartMovement(EnemyGroup enemyGroup, Canvas canvas)
    {
        movementTimer.Tick += (s, e) => 
        {
            strategy.Move(enemyGroup.Enemies.ToList(), ref movingRight, canvas.ActualWidth);
        };
        movementTimer.Start();
    }
    
    public void Stop()
    {
        movementTimer?.Stop();
    }
}
```

#### 3. EnemyShootingSystem.cs - Система стрільби
```csharp
public class EnemyShootingSystem
{
    private readonly Canvas canvas;
    private readonly Random random = new Random();
    private DispatcherTimer shootingTimer;
    
    public EnemyShootingSystem(Canvas canvas)
    {
        this.canvas = canvas ?? throw new ArgumentNullException(nameof(canvas));
    }
    
    public void StartShooting(EnemyGroup enemyGroup, Player player, GameState gameState)
    {
        shootingTimer = new DispatcherTimer();
        shootingTimer.Interval = TimeSpan.FromMilliseconds(GameConfig.Timing.EnemyMoveMilliseconds);
        
        shootingTimer.Tick += (s, e) =>
        {
            if (gameState.CurrentState != GameStateEnum.Playing)
                return;
                
            var shooters = enemyGroup.GetBottomEnemiesInColumns();
            
            if (shooters.Count == 0)
            {
                gameState.SetVictory();
                return;
            }
            
            Enemy shooter = shooters[random.Next(shooters.Count)];
            if (shooter.IsAlive)
            {
                ShootBullet(shooter, player);
            }
        };
        
        shootingTimer.Start();
    }
    
    private void ShootBullet(Enemy enemy, Player player)
    {
        Bullet bullet = new Bullet(
            enemy.X + GameConfig.Sizes.EnemyWidth / 2, 
            enemy.Y + GameConfig.Sizes.EnemyHeight, 
            isEnemyBullet: true
        );
        
        bullet.Speed = GameConfig.Speeds.EnemyBulletSpeed;
        canvas.Children.Add(bullet.HitBox);
        
        // Делегувати обробку руху кулі BulletManager
    }
    
    public void Stop()
    {
        shootingTimer?.Stop();
    }
}
```

#### 4. BarrierManager.cs - Управління бар'єрами
```csharp
public class BarrierManager
{
    private readonly List<Barrier> barriers = new List<Barrier>();
    private readonly Canvas canvas;
    
    public IReadOnlyList<Barrier> Barriers => barriers.AsReadOnly();
    
    public BarrierManager(Canvas canvas)
    {
        this.canvas = canvas ?? throw new ArgumentNullException(nameof(canvas));
    }
    
    public void CreateBarriers(params (double x, double y)[] positions)
    {
        foreach (var (x, y) in positions)
        {
            Barrier barrier = new Barrier(x, y);
            barrier.AddToCanvas(canvas);
            barriers.Add(barrier);
        }
    }
    
    public void CreateDefaultBarriers(double canvasHeight)
    {
        CreateBarriers(
            (GameConfig.Layout.Barrier1X, canvasHeight - GameConfig.Layout.BarrierOffsetY),
            (GameConfig.Layout.Barrier2X, canvasHeight - GameConfig.Layout.BarrierOffsetY)
        );
    }
    
    public bool CheckCollision(Rectangle hitBox)
    {
        return barriers.Any(b => b.IsCollidingWith(hitBox));
    }
}
```

#### 5. CollisionDetector.cs - Детекція колізій
```csharp
public class CollisionDetector
{
    public bool CheckBulletEnemyCollision(Bullet bullet, Enemy enemy)
    {
        if (!enemy.IsAlive) return false;
        
        Rect bulletRect = new Rect(
            Canvas.GetLeft(bullet.HitBox), 
            Canvas.GetTop(bullet.HitBox), 
            bullet.HitBox.Width, 
            bullet.HitBox.Height
        );
        
        Rect enemyRect = new Rect(
            Canvas.GetLeft(enemy.HitBox), 
            Canvas.GetTop(enemy.HitBox), 
            enemy.HitBox.Width, 
            enemy.HitBox.Height
        );
        
        return bulletRect.IntersectsWith(enemyRect);
    }
    
    public bool CheckBulletPlayerCollision(Bullet bullet, Player player)
    {
        Rect bulletRect = new Rect(
            Canvas.GetLeft(bullet.HitBox), 
            Canvas.GetTop(bullet.HitBox), 
            bullet.HitBox.Width, 
            bullet.HitBox.Height
        );
        
        Rect playerRect = new Rect(
            Canvas.GetLeft(player.Image), 
            Canvas.GetTop(player.Image), 
            player.Image.Width, 
            player.Image.Height
        );
        
        return bulletRect.IntersectsWith(playerRect);
    }
}
```

---

## Issue #2: Усунення дублювання

### Було (GameMode1.xaml.cs):
```csharp
private void UpdateHeartsUI()
{
    if (player.HP >= 0 && player.HP < ListHPImage.Count)
    {
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
    
    // Той самий код повторюється для HP == -99
    if (player.HP == -99)
    {
        // ... ідентичний код ...
    }
}
```

### Стало:

#### GameMode1.xaml.cs (оновлений):
```csharp
private void UpdateHeartsUI()
{
    if (player.HP >= 0 && player.HP < ListHPImage.Count)
    {
        ListHPImage[player.HP].Visibility = Visibility.Hidden;
    }
    
    if (gameState.CurrentState == GameStateEnum.PlayerDead || 
        gameState.CurrentState == GameStateEnum.Victory)
    {
        HandleGameOver();
    }
}

private void HandleGameOver()
{
    scoreManager.SaveScore(GetCurrentScore(), settingsManager.GetPlayerName());
    ShowEndWindow();
    StopAndCloseGame();
}

private string GetCurrentScore()
{
    var scoreElement = MyCanvas.Children
        .OfType<FrameworkElement>()
        .FirstOrDefault(e => e.Tag != null && e.Tag.ToString() == "ScoreText");
    
    return (scoreElement as TextBlock)?.Text ?? "0";
}

private void ShowEndWindow()
{
    this.Hide();
    EndWindow endWindow = new EndWindow(gameState.CurrentState == GameStateEnum.Victory);
    endWindow.ShowDialog();
    this.Show();
}

private void StopAndCloseGame()
{
    timer.Stop();
    this.Close();
}
```

#### ScoreManager.cs (новий клас):
```csharp
public class ScoreManager
{
    private readonly string recordsFilePath;
    
    public ScoreManager(string recordsFilePath)
    {
        this.recordsFilePath = recordsFilePath ?? throw new ArgumentNullException(nameof(recordsFilePath));
    }
    
    public void SaveScore(string score, string playerName)
    {
        try
        {
            string record = $"{playerName} {score}\n";
            File.AppendAllText(recordsFilePath, record);
        }
        catch (Exception ex)
        {
            // Логування помилки
            Debug.WriteLine($"Error saving score: {ex.Message}");
        }
    }
    
    public List<(string name, int score)> GetTopScores(int count = 10)
    {
        try
        {
            if (!File.Exists(recordsFilePath))
                return new List<(string, int)>();
            
            return File.ReadAllLines(recordsFilePath)
                .Select(line => line.Split(' '))
                .Where(parts => parts.Length == 2 && int.TryParse(parts[1], out _))
                .Select(parts => (parts[0], int.Parse(parts[1])))
                .OrderByDescending(record => record.Item2)
                .Take(count)
                .ToList();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error reading scores: {ex.Message}");
            return new List<(string, int)>();
        }
    }
}
```

#### GameState.cs (новий клас):
```csharp
public enum GameStateEnum
{
    Playing,
    Paused,
    PlayerDead,
    Victory,
    GameOver
}

public class GameState
{
    private GameStateEnum currentState = GameStateEnum.Playing;
    
    public GameStateEnum CurrentState
    {
        get => currentState;
        private set
        {
            if (currentState != value)
            {
                var oldState = currentState;
                currentState = value;
                OnStateChanged?.Invoke(oldState, value);
            }
        }
    }
    
    public event Action<GameStateEnum, GameStateEnum> OnStateChanged;
    
    public void SetPlaying() => CurrentState = GameStateEnum.Playing;
    public void SetPaused() => CurrentState = GameStateEnum.Paused;
    public void SetPlayerDead() => CurrentState = GameStateEnum.PlayerDead;
    public void SetVictory() => CurrentState = GameStateEnum.Victory;
    public void SetGameOver() => CurrentState = GameStateEnum.GameOver;
    
    public bool IsGameActive => CurrentState == GameStateEnum.Playing;
    public bool IsGameOver => CurrentState == GameStateEnum.PlayerDead || 
                             CurrentState == GameStateEnum.Victory || 
                             CurrentState == GameStateEnum.GameOver;
}
```

---

## Issue #3: Константи та інкапсуляція

### Було:

#### Магічні числа всюди:
```csharp
// Player.cs
Width = 50,
Height = 50,
Speed = 8;

// GroupEnemy.cs
double startX = 50;
double startY = 50;
double spacingX = 45;
double spacingY = 35;
player.HP = -99;  // ???

// Інше
bullet.Speed = Hard ? 12 : 12;  // Безглуздо!
```

#### Погана інкапсуляція:
```csharp
// Player.cs
public bool FlyLeft = false, FlyRight = false;
public bool ButtonShoot = false;
public List<Bullet> bullets = new List<Bullet>();
public double lastShotTime { get; set; } = 0;

// Entity.cs
public Rectangle HitBox;  // Публічне поле!
```

### Стало:

#### GameConfig.cs (новий клас):
```csharp
public static class GameConfig
{
    public static class Sizes
    {
        public const double PlayerWidth = 50;
        public const double PlayerHeight = 50;
        public const double EnemyWidth = 28;
        public const double EnemyHeight = 28;
        public const double EnemyMoveDown = 48; // Height + 20
        public const double BulletWidth = 3;
        public const double BulletHeight = 12;
        public const double BulletCenterOffset = 1.5;
        public const double HeartSize = 30;
    }
    
    public static class Speeds
    {
        public const double PlayerSpeed = 8;
        public const double EnemySpeed = 20;
        public const double EnemySpeedHard = 40;
        public const double PlayerBulletSpeed = 10;
        public const double EnemyBulletSpeed = 12;
    }
    
    public static class Layout
    {
        public const double EnemyStartX = 50;
        public const double EnemyStartY = 50;
        public const double EnemySpacingX = 45;
        public const double EnemySpacingY = 35;
        
        public const double Barrier1X = 200;
        public const double Barrier2X = 500;
        public const double BarrierOffsetY = 150;
        
        public const double PlayerBottomOffset = 40;
    }
    
    public static class Timing
    {
        public const double PlayerCooldownSeconds = 0.5;
        public const int GameTickMilliseconds = 16; // ~60 FPS
        public const int EnemyMoveMilliseconds = 500;
        public const int BulletUpdateMilliseconds = 20;
    }
    
    public static class Scoring
    {
        public const int EnemyBaseScore = 10;
    }
    
    public static class Game
    {
        public const int PlayerMaxHP = 3;
        public const int DefaultEnemyRows = 4;
        public const int DefaultEnemyColumns = 7;
    }
}
```

#### Player.cs (оновлений з правильною інкапсуляцією):
```csharp
public class Player : Entity
{
    // Приватні поля
    private bool isMovingLeft = false;
    private bool isMovingRight = false;
    private bool isShooting = false;
    private readonly List<Bullet> bullets = new List<Bullet>();
    private double lastShotTime = 0;
    
    // Властивості з контролем доступу
    public int MaxHP { get; } = GameConfig.Game.PlayerMaxHP;
    public int HP { get; private set; } = GameConfig.Game.PlayerMaxHP;
    public double CooldownSeconds { get; } = GameConfig.Timing.PlayerCooldownSeconds;
    public double Speed { get; } = GameConfig.Speeds.PlayerSpeed;
    
    // Read-only доступ до куль
    public IReadOnlyList<Bullet> Bullets => bullets.AsReadOnly();
    
    // Властивості для читання стану
    public bool IsMovingLeft => isMovingLeft;
    public bool IsMovingRight => isMovingRight;
    public bool IsShooting => isShooting;
    public bool IsAlive => HP > 0;
    
    public Player()
    {
        Image = new Image
        {
            Width = GameConfig.Sizes.PlayerWidth,
            Height = GameConfig.Sizes.PlayerHeight,
            Source = LoadPlayerImage()
        };

        HitBox = new Rectangle
        {
            Width = Image.Width,
            Height = Image.Height,
            Stroke = Brushes.Cyan, 
            StrokeThickness = 1,
            Fill = Brushes.Transparent
        };
    }
    
    // Методи для управління рухом
    public void StartMovingLeft() => isMovingLeft = true;
    public void StopMovingLeft() => isMovingLeft = false;
    public void StartMovingRight() => isMovingRight = true;
    public void StopMovingRight() => isMovingRight = false;
    public void StartShooting() => isShooting = true;
    public void StopShooting() => isShooting = false;
    
    // Метод для отримання пошкодження
    public void TakeDamage(int damage = 1)
    {
        HP = Math.Max(0, HP - damage);
    }
    
    // Метод для створення кулі (тепер повертає кулю замість додавання в список)
    public Bullet TryShoot()
    {
        if (!isShooting) return null;
        
        double currentTime = Environment.TickCount / 1000.0;
        if (currentTime - lastShotTime < CooldownSeconds)
            return null;
        
        lastShotTime = currentTime;
        
        double bulletX = Canvas.GetLeft(Image) + Image.Width / 2 - GameConfig.Sizes.BulletCenterOffset;
        double bulletY = Canvas.GetTop(Image);
        
        return new Bullet(bulletX, bulletY, isEnemyBullet: false);
    }
    
    // Приватний метод для завантаження зображення
    private BitmapImage LoadPlayerImage()
    {
        try
        {
            string settingsPath = Path.Combine(Directory.GetCurrentDirectory(), "Settings.txt");
            string imagePath = File.ReadAllLines(settingsPath)[1].Trim();
            return new BitmapImage(new Uri(imagePath, UriKind.Absolute));
        }
        catch
        {
            // Повернути зображення за замовчуванням
            return new BitmapImage(new Uri("Images/player_default.png", UriKind.Relative));
        }
    }
}
```

#### Entity.cs (оновлений):
```csharp
public abstract class Entity
{
    // Властивості замість полів
    public Image Image { get; protected set; }
    public Rectangle HitBox { get; protected set; }
    public double X { get; set; }
    public double Y { get; set; }
    public bool IsAlive { get; set; } = true;
    
    // Protected константи для доступу в нащадках
    protected virtual double DefaultWidth => GameConfig.Sizes.EnemyWidth;
    protected virtual double DefaultHeight => GameConfig.Sizes.EnemyHeight;
    
    // Метод для оновлення позиції HitBox
    protected void UpdateHitBoxPosition()
    {
        if (HitBox != null)
        {
            Canvas.SetLeft(HitBox, X);
            Canvas.SetTop(HitBox, Y);
        }
    }
}
```

---

## 📝 Підсумок змін

### Issue #1: Розділення відповідальностей
- ✅ Створено 6 нових класів замість 1 великого
- ✅ Кожен клас має чітку відповідальність
- ✅ Використано паттерн Strategy для руху
- ✅ Покращена тестовність

### Issue #2: Усунення дублювання
- ✅ Видалено дублювання коду
- ✅ Створено методи для кожної відповідальності
- ✅ Введено enum для станів гри
- ✅ Винесено роботу з файлами в ScoreManager

### Issue #3: Константи та інкапсуляція
- ✅ Всі магічні числа в GameConfig
- ✅ Приватні поля з контрольованим доступом
- ✅ Використання IReadOnlyList для колекцій
- ✅ Властивості замість публічних полів
- ✅ Enum замість магічного -99

### Загальний результат:
- 📈 Покращена архітектура
- 📈 Зрозуміліший код
- 📈 Легша підтримка
- 📈 Краща тестовність
- 📈 Дотримання SOLID принципів
