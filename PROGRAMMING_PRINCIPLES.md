Ти правий — не було потреби переводити на англійську.

Ось **чиста українська версія для GitHub (Markdown)** без дивних символів, без нестандартних тире, без проблем з кодуванням. Можеш вставляти прямо в README.md.

---

# PROGRAMMING PRINCIPLES - Space Invaders

Проєкт: гра Space Invaders на C# (WPF)
Архітектура: XAML (UI) + Code-behind + Game logic classes

## Структура проєкту

* UI: GameMode1.xaml
* Логіка UI: GameMode1.xaml.cs
* Ігрові сутності: Entity.cs, Player.cs, Enemy.cs, Bullet.cs, Barrier.cs
* Управління ворогами: GroupEnemy.cs
* Ресурси: Settings.txt, TableRecords.txt

---

## 1) Separation of Concerns

Кожен файл відповідає за свою частину логіки:

* GameMode1.xaml - тільки розмітка UI
* GameMode1.xaml.cs - обробка подій та ігровий цикл
* Player, Enemy, Bullet, Barrier - логіка конкретних сутностей
* GroupEnemy - управління групою ворогів

Приклад базового класу:

```csharp
public class Entity
{
    public Image Image { get; set; }
    public Rectangle HitBox;
    public double X { get; set; }
    public double Y { get; set; }
    public bool IsAlive { get; set; } = true;
}
```

---

## 2) DRY (Don't Repeat Yourself)

Частково реалізовано.

Позитив:
Спільні властивості винесені в базовий клас Entity.

Проблема:
Логіка завершення гри дублюється в UpdateHeartsUI():

* при HP == 0
* при HP == -99

Рішення:
Винести в окремий метод:

```csharp
private void EndGame()
{
}
```

---

## 3) KISS (Keep It Simple)

Порушення:

```csharp
bullet.Speed = Hard ? 12 : 12;
```

Тернарний оператор не має сенсу, оскільки значення однакове.

Метод EnemyShoot() виконує занадто багато задач:

* створення кулі
* додавання на Canvas
* створення таймера
* рух кулі
* перевірка колізій
* видалення кулі

Метод потрібно розбити на менші.

---

## 4) Single Responsibility Principle (SRP)

Порушується.

GroupEnemy відповідає за:

* створення ворогів
* створення бар'єрів
* рух
* стрільбу
* перевірку колізій
* рахунок
* хвилі

UpdateHeartsUI():

* оновлює UI
* записує рекорд у файл
* відкриває вікно Game Over
* зупиняє таймер
* закриває вікно

---

## 5) Encapsulation

Частково реалізовано.

Коректний приклад:

```csharp
public int MaxHP { get; } = 3;
public int HP { get; set; } = 3;
```

Проблема:

```csharp
public bool FlyLeft = false;
public bool FlyRight = false;
public bool ButtonShoot = false;
public List<Bullet> bullets = new List<Bullet>();
public Rectangle HitBox;
```

Поля повинні бути private або з властивостями доступу.

---

## 6) Magic Numbers

Проблема:

```csharp
double startX = 50;
double startY = 50;
double spacingX = 45;
double spacingY = 35;
Speed = 8;
Width = 50;
Height = 50;
```

Рішення:

```csharp
private const int PLAYER_SPEED = 8;
private const int PLAYER_SIZE = 50;
```

Або винести в клас GameConstants.

---

## 7) Code Reusability

Позитив:

* Наслідування через Entity
* Використання композиції

```csharp
public List<Bullet> bullets = new List<Bullet>();
public List<Barrier> Barriers { get; set; }
```

Покращення:

* Винести логіку колізій в окремий CollisionService
* Винести роботу з файлами в FileService

---

## 8) Defensive Programming

Частково реалізовано:

* перевірка HP
* перевірка завершення гри

Відсутнє:

* перевірка на null
* обробка винятків при роботі з файлами

---

## 9) Readability

Переваги:

* Зрозумілі назви класів: Player, Enemy, Bullet
* Методи названі дієсловами: CreateEnemies(), MoveEnemies()

Проблема:

```csharp
if (element is TextBlock Socore)
```

Помилка в назві змінної.

---

## 10) Composition Over Inheritance

Реалізовано коректно:

* Player містить List<Bullet>
* Немає глибокої ієрархії наслідування
* Використовується композиція там, де це доречно

---

# Підсумок

Сильні сторони:

* Базовий клас Entity
* Логічна структура сутностей
* Коректне використання наслідування
* Використання композиції

Слабкі сторони:

* Порушення SRP
* Змішування UI та бізнес-логіки
* Дублювання коду
* Магічні числа
* Недостатня інкапсуляція

---

Якщо потрібно, можу зробити ще більш формальну версію для захисту лабораторної.
