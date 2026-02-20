# Issue #2: Дублювання коду (DRY Violation) в обробці закінчення гри

## 🔴 Тип: Code Smell / Code Duplication

## 📝 Опис проблеми

В методі `UpdateHeartsUI` класу `GameMode1` присутнє значне дублювання коду. Майже ідентична логіка обробки закінчення гри повторюється двічі для різних умов (`HP == 0` та `HP == -99`).

## 🔍 Детальний аналіз

### Локація дублювання

**Файл:** [GameMode1.xaml.cs, lines 60-112](https://github.com/YOUR_USERNAME/Space_Invaders/blob/main/Space%20Invaders/GameMode1.xaml.cs#L60-L112)

### Перший блок (HP == 0):
```csharp
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
```

### Другий блок (HP == -99):
```csharp
if (player.HP == -99)
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
```

### Аналіз дублювання:

**Ідентичні операції в обох блоках:**
1. ✅ Пошук TextBlock з тегом "ScoreText"
2. ✅ Читання імені гравця з Settings.txt
3. ✅ Збереження результату в TableRecords.txt
4. ✅ Приховування поточного вікна
5. ✅ Відкриття EndWindow
6. ✅ Повторне відображення вікна
7. ✅ Зупинка таймера
8. ✅ Закриття вікна

**Різниця:** Тільки умова перевірки HP (`== 0` vs `== -99`)

## 💥 Наслідки проблеми

1. **Порушення DRY принципу** - "Don't Repeat Yourself"
2. **Подвійна підтримка** - будь-які зміни треба робити в двох місцях
3. **Ризик помилок** - легко забути оновити один з блоків
4. **Погана читабельність** - код виглядає надмірно довгим
5. **Важче тестувати** - треба покривати тестами два ідентичні блоки

## 🔎 Додаткові проблеми в цьому коді

### 1. Магічне число -99
**Файл:** [GameMode1.xaml.cs, line 90](https://github.com/YOUR_USERNAME/Space_Invaders/blob/main/Space%20Invaders/GameMode1.xaml.cs#L90)

Незрозуміло, чому використовується саме `-99` для позначення спеціального стану.

### 2. Незрозумілий стан гри
**Файл:** [GroupEnemy.cs, line 85](https://github.com/YOUR_USERNAME/Space_Invaders/blob/main/Space%20Invaders/GroupEnemy.cs#L85)
```csharp
player.HP = -99;
```
Використання HP для сигналізації спеціальної події (всі вороги знищені) - це неправильно.

### 3. Змішування відповідальностей
Метод `UpdateHeartsUI` робить набагато більше, ніж оновлення UI:
- Читає файли
- Записує файли
- Управляє вікнами
- Управляє життєвим циклом гри

## ✅ Рішення

### Варіант 1: Винести в окремий метод

```csharp
private void UpdateHeartsUI()
{
    if (player.HP >= 0 && player.HP < ListHPImage.Count)
    {
        ListHPImage[player.HP].Visibility = Visibility.Hidden;
    }
    
    if (player.HP <= 0 || player.HP == -99)
    {
        HandleGameOver();
    }
}

private void HandleGameOver()
{
    SaveScore();
    ShowEndWindow();
    StopAndClose();
}

private void SaveScore()
{
    var scoreElement = MyCanvas.Children
        .OfType<FrameworkElement>()
        .FirstOrDefault(e => e.Tag != null && e.Tag.ToString() == "ScoreText");

    if (scoreElement is TextBlock scoreText)
    {
        string filePath = Path.Combine(Directory.GetCurrentDirectory(), "TableRecords.txt");
        string name = File.ReadAllLines(
            Path.Combine(Directory.GetCurrentDirectory(), "Settings.txt")
        )[0].Trim();
        
        File.AppendAllText(filePath, $"{name} {scoreText.Text}\n");
    }
}

private void ShowEndWindow()
{
    this.Hide();
    EndWindow endWindow = new EndWindow(true);
    endWindow.ShowDialog();
    this.Show();
}

private void StopAndClose()
{
    timer.Stop();
    this.Close();
}
```

### Варіант 2: Створити enum для стану гри (краще)

```csharp
public enum GameState
{
    Playing,
    PlayerDead,
    AllEnemiesDefeated,
    GameOver
}

private void UpdateHeartsUI()
{
    if (player.HP >= 0 && player.HP < ListHPImage.Count)
    {
        ListHPImage[player.HP].Visibility = Visibility.Hidden;
    }
    
    var gameState = DetermineGameState();
    if (gameState == GameState.PlayerDead || gameState == GameState.AllEnemiesDefeated)
    {
        HandleGameOver();
    }
}

private GameState DetermineGameState()
{
    if (player.HP == 0) return GameState.PlayerDead;
    if (player.HP < 0) return GameState.AllEnemiesDefeated; // Замість -99
    return GameState.Playing;
}
```

### Варіант 3: Створити окремий клас GameStateManager (найкраще)

```csharp
public class GameStateManager
{
    private readonly Canvas canvas;
    private readonly DispatcherTimer timer;
    private readonly Window window;
    
    public void HandleGameOver()
    {
        SaveScore();
        ShowEndWindow();
        StopAndClose();
    }
    
    // ... інші методи
}
```

## 🎯 План виправлення

1. ✅ Створити окремі методи для кожної відповідальності
2. ✅ Об'єднати дублюючі блоки в один
3. ✅ Замінити магічне число -99 на enum або константу
4. ✅ Розділити логіку оновлення UI та обробки закінчення гри
5. ✅ Винести роботу з файлами в окремий клас (ScoreManager або FileService)

## 📊 Очікувані переваги

- ✅ Код без дублювання
- ✅ Легше підтримувати
- ✅ Краща читабельність
- ✅ Чіткіше розділення відповідальностей
- ✅ Простіше тестувати
- ✅ Менше ймовірність помилок при змінах

## 🔗 Пов'язані Issues

- Issue #1 - Порушення SRP (цей код також порушує принцип єдиної відповідальності)
- Issue #3 - Magic Numbers (використання -99)

## 🏷️ Мітки
- `code-smell`
- `duplication`
- `DRY-violation`
- `refactoring`
- `medium-priority`
