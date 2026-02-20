# Issue #1: Порушення Single Responsibility Principle в класі GroupEnemy

## Тип: Code Smell / Violation of SRP

## Опис проблеми

Клас `GroupEnemy` порушує принцип єдиної відповідальності (Single Responsibility Principle), виконуючи надто багато різнорідних функцій. Це ускладнює підтримку, тестування та розширення коду.

## Детальний аналіз

### Множинні відповідальності класу GroupEnemy:

1. **Управління ворогами** - створення, зберігання та видалення
2. **Управління бар'єрами** - створення та розташування
3. **Логіка руху** - різні алгоритми переміщення (Easy/Hard)
4. **Система стрільби** - створення куль та їх обробка
5. **Детекція колізій** - перевірка зіткнень куль з ворогами
6. **UI логіка** - відображення рахунку
7. **Управління хвилями** - прогресія складності

### Локації коду з проблемами:

#### 1. Створення бар'єрів (не відноситься до групи ворогів)
**Файл:** [GroupEnemy.cs, lines 28-40](https://github.com/YOUR_USERNAME/Space_Invaders/blob/main/Space%20Invaders/GroupEnemy.cs#L28-L40)
```csharp
public void CreateBarriers()
{
    Barrier barrier1 = new Barrier(100, MyCanvas.ActualHeight - 150);
    Barrier barrier2 = new Barrier(400, MyCanvas.ActualHeight - 150);
    
    barrier1.AddToCanvas(MyCanvas);
    barrier2.AddToCanvas(MyCanvas);
    
    Barriers.Add(barrier1);
    Barriers.Add(barrier2);
}
```

#### 2. Складна логіка стрільби з обробкою колізій
**Файл:** [GroupEnemy.cs, lines 97-141](https://github.com/YOUR_USERNAME/Space_Invaders/blob/main/Space%20Invaders/GroupEnemy.cs#L97-L141)
```csharp
private void EnemyShoot(Enemy enemy, Player player, List<Barrier> barriers)
{
    Bullet bullet = new Bullet(enemy.X + 15, enemy.Y + 40, true);
    bullet.Speed = Hard ? 12 : 12;
    MyCanvas.Children.Add(bullet.HitBox);

    DispatcherTimer bulletTimer = new DispatcherTimer();
    bulletTimer.Interval = TimeSpan.FromMilliseconds(20);
    bulletTimer.Tick += (s, e) =>
    {
        // ... логіка руху
        // ... перевірка виходу за межі
        // ... перевірка колізії з бар'єрами
        // ... перевірка колізії з гравцем
        // ... нанесення шкоди
    };
    bulletTimer.Start();
}
```

#### 3. Дві різні логіки руху в одному класі
**Файл:** [GroupEnemy.cs, lines 150-176](https://github.com/YOUR_USERNAME/Space_Invaders/blob/main/Space%20Invaders/GroupEnemy.cs#L150-L176) та [lines 177-210](https://github.com/YOUR_USERNAME/Space_Invaders/blob/main/Space%20Invaders/GroupEnemy.cs#L177-L210)

## Наслідки проблеми

1. **Складність тестування** - важко написати юніт-тести для класу з множинною відповідальністю
2. **Важка підтримка** - будь-яка зміна в одній частині може вплинути на інші
3. **Низька когезія** - різнорідні методи в одному класі
4. **Порушення Open/Closed Principle** - складно розширювати функціональність без модифікації класу
5. **Складність розуміння** - клас занадто великий і робить занадто багато

## Рішення

### Розділити клас на кілька окремих класів з чіткою відповідальністю:

1. **EnemyGroup** - тільки управління групою ворогів
   - Створення ворогів
   - Зберігання списку
   - Базові операції з групою

2. **EnemyMovementController** - управління рухом
   - Різні стратегії руху (Easy/Hard)
   - Патерн Strategy для вибору алгоритму

3. **EnemyShootingSystem** - система стрільби
   - Логіка вибору стрільців
   - Створення куль

4. **BulletManager** - управління кулями
   - Рух куль
   - Обробка життєвого циклу

5. **CollisionDetector** - детекція колізій
   - Перевірка зіткнень
   - Окрема відповідальність

6. **BarrierManager** - управління бар'єрами
   - Створення
   - Розташування
   - Обробка пошкоджень

7. **ScoreManager** - управління рахунком
   - Відображення
   - Збереження
   - Обчислення

## План виправлення

1. Створити нові класи для кожної відповідальності
2. Перенести відповідний код з GroupEnemy
3. Оновити залежності та взаємодію між класами
4. Видалити або сильно спростити GroupEnemy (можливо залишити як координатор)
5. Покрити новий код тестами

