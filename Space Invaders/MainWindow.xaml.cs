using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace Space_Invaders
{
    public partial class MainWindow : Window
    {

        Player player = new Player();

        private DispatcherTimer timer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();
            this.PreviewKeyDown += MainWindow_PreviewKeyDown;

        }



        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            // Приховати головне меню
            MainMenuPanel.Visibility = Visibility.Collapsed;
            // Показати вибір режиму
            ModeSelectionPanel.Visibility = Visibility.Visible;
        }

        private void EasyModeButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            GameMode1 gameMode1 = new GameMode1();
            gameMode1.ShowDialog();
            this.Show();

        }

        private void HardModeButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            GameMode2 gameMode2 = new GameMode2();
            gameMode2.ShowDialog();
            this.Show();

        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.Owner = this; // Встановлюємо власника
            settingsWindow.ShowDialog();
        }
        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Якщо натиснуто ESC і видима панель вибору режиму
            if (e.Key == Key.Escape && ModeSelectionPanel.Visibility == Visibility.Visible)
            {
                // Повертаємося до головного меню
                ModeSelectionPanel.Visibility = Visibility.Collapsed;
                MainMenuPanel.Visibility = Visibility.Visible;
                e.Handled = true; // Запобігаємо подальшій обробці клавіші
            }
        }
    }
}