# Issue #3: Магічні числа та порушення інкапсуляції

## Тип: Code Smell / Magic Numbers & Poor Encapsulation

## Опис проблеми

Проєкт містить численні "магічні числа" (hard-coded values без пояснення) та порушення інкапсуляції через використання публічних полів замість властивостей. Це робить код важким для розуміння, підтримки та модифікації.

## Детальний аналіз

## Частина 1: Магічні числа (Magic Numbers)

### 1.1 Розміри та позиції ворогів

**Файл:** [GroupEnemy.cs, lines 44-47](https://github.com/YOUR_USERNAME/Space_Invaders/blob/main/Space%20Invaders/GroupEnemy.cs#L44-L47)

```csharp
double startX = 50;
double startY = 50;
double spacingX = 45;
double spacingY = 35;
```

**Проблема:** Незрозуміло, чому використовуються саме ці значення.

### 1.2 Розміри гравця

**Файл:** [Player.cs, lines 29-30](https://github.com/YOUR_USERNAME/Space_Invaders/blob/main/Space%20Invaders/Player.cs#L29-L30)

```csharp
Width = 50,
Height = 50,
```

**Проблема:** Жорстко закодовані розміри без можливості легко їх змінити.

### 1.3 Швидкість та затримки

**Файл:** [Player.cs, lines 23-24, 44](https://github.com/YOUR_USERNAME/Space_Invaders/blob/main/Space%20Invaders/Player.cs#L23-L24)

```csharp
public double CooldownSeconds { get; } = 0.5;
Speed = 8;
```

**Файл:** [GroupEnemy.cs, line 22](https://github.com/YOUR_USERNAME/Space_Invaders/blob/main/Space%20Invaders/GroupEnemy.cs#L22)

```csharp
private double enemySpeed = 20;
```

### 1.4 Позиція створення кулі

**Файл:** [Player.cs, line 69](https://github.com/YOUR_USERNAME/Space_Invaders/blob/main/Space%20Invaders/Player.cs#L69)

```csharp
double XBulletPositions = Canvas.GetLeft(Image) + Image.Width / 2 - 1.5;
```

**Проблема:** Чому саме `1.5`? Що це за значення?

### 1.5 Критичний магічний номер HP

**Файл:** [GroupEnemy.cs, line 85](https://github.com/YOUR_USERNAME/Space_Invaders/blob/main/Space%20Invaders/GroupEnemy.cs#L85)

```csharp
player.HP = -99;
```

**Проблема:** Використання -99 як спеціального сигналу про закінчення гри - дуже поганий підхід.

### 1.6 Позиції бар'єрів

**Файл:** [GroupEnemy.cs, lines 31-32](https://github.com/YOUR_USERNAME/Space_Invaders/blob/main/Space%20Invaders/GroupEnemy.cs#L31-L32)

```csharp
Barrier barrier1 = new Barrier(100, MyCanvas.ActualHeight - 150);
Barrier barrier2 = new Barrier(400, MyCanvas.ActualHeight - 150);
```

### 1.7 Швидкість куль (безглуздий тернарний оператор)

**Файл:** [GroupEnemy.cs, line 100](https://github.com/YOUR_USERNAME/Space_Invaders/blob/main/Space%20Invaders/GroupEnemy.cs#L100)

```csharp
bullet.Speed = Hard ? 12 : 12;
```

**Проблема:** Тернарний оператор, який повертає однакове значення - це не тільки магічне число, але й безглуздий код.

### 1.8 Інтервали таймерів

**Файл:** [GameMode1.xaml.cs, line 31](https://github.com/YOUR_USERNAME/Space_Invaders/blob/main/Space%20Invaders/GameMode1.xaml.cs#L31)

```csharp
timer.Interval = TimeSpan.FromMilliseconds(16);
```

**Файл:** [GroupEnemy.cs, line 74](https://github.com/YOUR_USERNAME/Space_Invaders/blob/main/Space%20Invaders/GroupEnemy.cs#L74)

```csharp
enemyTimer.Interval = TimeSpan.FromMilliseconds(500);
```

---

## Частина 2: Порушення інкапсуляції

### 2.1 Публічні поля в класі Player

**Файл:** [Player.cs, lines 16-17](https://github.com/YOUR_USERNAME/Space_Invaders/blob/main/Space%20Invaders/Player.cs#L16-L17)

```csharp
public bool FlyLeft = false, FlyRight = false;
public bool ButtonShoot = false;
```

**Проблема:** Ці поля повинні бути приватними з контрольованим доступом через методи.

### 2.2 Публічний список куль

**Файл:** [Player.cs, line 20](https://github.com/YOUR_USERNAME/Space_Invaders/blob/main/Space%20Invaders/Player.cs#L20)

```csharp
public List<Bullet> bullets = new List<Bullet>();
```

**Проблема:** Зовнішній код може напряму модифікувати список, обходячи логіку класу.

### 2.3 Публічне поле HitBox в Entity

**Файл:** [Entity.cs, line 13](https://github.com/YOUR_USERNAME/Space_Invaders/blob/main/Space%20Invaders/Entity.cs#L13)

```csharp
public Rectangle HitBox;
```

**Проблема:** Поле повинно бути властивістю або принаймні `readonly`.

### 2.4 Публічне поле lastShotTime

**Файл:** [Player.cs, line 22](https://github.com/YOUR_USERNAME/Space_Invaders/blob/main/Space%20Invaders/Player.cs#L22)

```csharp
public double lastShotTime { get; set; } = 0;
```

**Проблема:** Внутрішня деталь реалізації не повинна бути публічною.

### 2.5 Внутрішні константи в Entity

**Файл:** [Entity.cs, lines 19-20](https://github.com/YOUR_USERNAME/Space_Invaders/blob/main/Space%20Invaders/Entity.cs#L19-L20)

```csharp
internal const double Width = 28;
internal const double Height = 28;
```

**Проблема:** Навіщо `internal`? Повинні бути `protected` або `private` з публічними властивостями при потребі.

---

## Наслідки проблем

### Магічні числа:
1. Незрозумілий код - неясно, що означають числа
2. Важко налаштовувати - треба шукати всі входження
3. Ризик помилок - при зміні можна забути якесь місце
4. Погана масштабованість - складно адаптувати під різні екрани

### Порушення інкапсуляції:
1. Немає контролю над даними
2. Можливість некоректної модифікаціїззовні
3. Важко додавати логіку валідації
4. Порушення принципу сховування інформації

---

## Рішення

### Рішення 1: Створити клас конфігурації для констант

```csharp
public static class GameConfig
{
    // Розміри
    public static class Sizes
    {
        public const double PlayerWidth = 50;
        public const double PlayerHeight = 50;
        public const double EnemyWidth = 28;
        public const double EnemyHeight = 28;
        public const double BulletOffset = 1.5;
    }
    
    // Швидкості
    public static class Speeds
    {
        public const double PlayerSpeed = 8;
        public const double EnemySpeed = 20;
        public const double BulletSpeed = 12;
        public const double BulletSpeedHard = 12; // або інше значення
    }
    
    // Позиції
    public static class Layout
    {
        public const double EnemyStartX = 50;
        public const double EnemyStartY = 50;
        public const double EnemySpacingX = 45;
        public const double EnemySpacingY = 35;
        
        public const double Barrier1X = 100;
        public const double Barrier2X = 400;
        public const double BarrierOffsetY = 150;
    }
    
    // Час та затримки
    public static class Timing
    {
        public const double PlayerCooldownSeconds = 0.5;
        public const int GameTickMilliseconds = 16; // ~60 FPS
        public const int EnemyMoveMilliseconds = 500;
        public const int BulletUpdateMilliseconds = 20;
    }
    
    // Стани гри
    public static class GameStates
    {
        public const int PlayerAlive = 0;
        public const int PlayerDead = 0;
        public const int AllEnemiesDefeated = -1; // Замість -99
    }
}
```

### Використання:

```csharp
// Замість:
Width = 50,
Height = 50,

// Використовувати:
Width = GameConfig.Sizes.PlayerWidth,
Height = GameConfig.Sizes.PlayerHeight,

// Замість:
player.HP = -99;

// Використовувати:
gameState = GameState.AllEnemiesDefeated;
```

### Рішення 2: Виправити інкапсуляцію

```csharp
public class Player : Entity
{
    // Замість публічних полів:
    private bool flyLeft = false;
    private bool flyRight = false;
    private bool buttonShoot = false;
    private readonly List<Bullet> bullets = new List<Bullet>();
    private double lastShotTime = 0;
    
    // Додати контрольовані методи:
    public void StartMovingLeft() => flyLeft = true;
    public void StopMovingLeft() => flyLeft = false;
    public void StartMovingRight() => flyRight = true;
    public void StopMovingRight() => flyRight = false;
    public void StartShooting() => buttonShoot = true;
    public void StopShooting() => buttonShoot = false;
    
    // Або властивості з валідацією:
    public bool IsMovingLeft 
    { 
        get => flyLeft; 
        set => flyLeft = value; 
    }
    
    // Для списку куль - тільки читання:
    public IReadOnlyList<Bullet> Bullets => bullets.AsReadOnly();
    
    // Або метод для отримання копії:
    public List<Bullet> GetBullets() => new List<Bullet>(bullets);
}
```

### Рішення 3: Виправити Entity

```csharp
public class Entity
{
    public Image Image { get; set; }
    public Rectangle HitBox { get; protected set; } // Властивість замість поля
    public double X { get; set; }
    public double Y { get; set; }
    public bool IsAlive { get; set; } = true;

    protected const double Width = 28;  // protected замість internal
    protected const double Height = 28;
    
    // Або краще - використовувати GameConfig:
    protected virtual double EntityWidth => GameConfig.Sizes.EnemyWidth;
    protected virtual double EntityHeight => GameConfig.Sizes.EnemyHeight;
}
```

### Рішення 4: Enum для станів гри

```csharp
public enum GameState
{
    Playing,
    PlayerDead,
    AllEnemiesDefeated,
    Paused,
    Victory
}

public class GameStateManager
{
    private GameState currentState = GameState.Playing;
    
    public GameState CurrentState 
    { 
        get => currentState;
        private set
        {
            if (currentState != value)
            {
                OnStateChanged(currentState, value);
                currentState = value;
            }
        }
    }
    
    public void SetPlayerDead() => CurrentState = GameState.PlayerDead;
    public void SetVictory() => CurrentState = GameState.AllEnemiesDefeated;
    
    private void OnStateChanged(GameState oldState, GameState newState)
    {
        // Логіка зміни стану
    }
}
```

---

## План виправлення

### Етап 1: Константи
1. Створити клас GameConfig
2. Перенести всі магічні числа в константи
3. Замінити всі входження в коді
4. Видалити або замінити безглуздий тернарний оператор

### Етап 2: Інкапсуляція
1. Зробити поля приватними
2. Додати властивості або методи доступу
3. Додати валідацію де потрібно
4. Використати readonly для колекцій

### Етап 3: Стани гри
1. Створити enum GameState
2. Замінити магічне -99 на enum
3. Створити GameStateManager

---

## Очікувані переваги

### Після виправлення магічних чисел:
- Код легко читається і зрозумілий
- Легко змінювати параметри гри
- Всі константи в одному місці
- Можливість створення різних конфігурацій (easy/hard)

### Після виправлення інкапсуляції:
- Контрольований доступ до даних
- Можливість додавання логіки валідації
- Захист від некоректних модифікацій
- Легше тестувати
- Кращий дизайн API класів

