using Space_Invaders.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Space_Invaders
{
    /// <summary>
    /// Логика взаимодействия для SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private List<string> imagePaths = new List<string>();
        private int currentImageIndex = -1;

        public SettingsWindow()
        {
            InitializeComponent();
            LoadImagesFromFolder();
            DataContext = this;

            
            this.PreviewKeyDown += (sender, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    this.Close();
                    Owner?.Show(); 
                    e.Handled = true;
                }
            };
        }

        private void LoadImagesFromFolder()
        {
          
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            int binIndex = basePath.IndexOf($"{System.IO.Path.DirectorySeparatorChar}bin{System.IO.Path.DirectorySeparatorChar}");

            string fullPath = System.IO.Path.Combine(basePath.Substring(0, binIndex), $"Images/SkinsPlayer/");
            string[] files = Directory.GetFiles(fullPath);
            List<string> imageExtensions = new List<string> { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff" };
            imagePaths = files
                .Where(file => imageExtensions.Contains(System.IO.Path.GetExtension(file).ToLower()))
                .ToList();

            if (imagePaths.Count > 0)
            {
                currentImageIndex = 0;
                LoadCurrentImage();
            }
            else
            {
                currentImageIndex = -1;
                DisplayImage.Source = null;
                System.Windows.MessageBox.Show("У папці не знайдено зображень");
            }

            try
            {
                TextBoxName.Text = File.ReadAllLines(Directory.GetCurrentDirectory() + "/Settings.txt")[0].Trim();
            }
            catch
            {
            }
        }

        private void LoadCurrentImage()
        {
            try
            {
                var bitmap = new BitmapImage(new Uri(imagePaths[currentImageIndex]));
                DisplayImage.Source = bitmap;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Помилка завантаження зображення: {ex.Message}");
            }
        }

        private void LeftButton_Click(object sender, RoutedEventArgs e)
        {
            if (imagePaths.Count == 0) return;

            currentImageIndex--;
            if (currentImageIndex < 0) currentImageIndex = imagePaths.Count - 1;

            LoadCurrentImage();
        }

        private void RightButton_Click(object sender, RoutedEventArgs e)
        {
            if (imagePaths.Count == 0) return;

            currentImageIndex++;
            if (currentImageIndex >= imagePaths.Count) currentImageIndex = 0;

            LoadCurrentImage();

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string filePath = Directory.GetCurrentDirectory() + "/Settings.txt"; 

            File.WriteAllText(filePath, string.Empty);
            File.AppendAllText(filePath, TextBoxName.Text + "\n");
            File.AppendAllText(filePath, imagePaths[currentImageIndex] + Environment.NewLine);

        }
    }
}
