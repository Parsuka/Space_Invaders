using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;

namespace Space_Invaders
{

    public partial class EndWindow : Window
    {
        bool winer;
        public EndWindow(bool win)
        {
            InitializeComponent();
            winer = win;
            LoadTop10Records();
        }

        private void LoadTop10Records()
        {
            string filePath = Directory.GetCurrentDirectory() + "/TableRecords.txt"; 
            List<Record> records = new List<Record>();

            if (File.Exists(filePath))
            {
                try
                {
                    var lines = File.ReadAllLines(filePath);
                    foreach (var line in lines)
                    {
                        var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length == 2 && int.TryParse(parts[1], out int score))
                        {
                            records.Add(new Record { Name = parts[0], Score = score });
                        }
                    }

                    foreach (var item in records.OrderByDescending(r => r.Score).Take(10).ToList())
                    {
                        string str = item.Name + "  " + item.Score;
                        listBoxTop10.Items.Add(str);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка при читанні файлу: {ex.Message}");
                }
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    
    }
    class Record
    {
        public string Name { get; set; }
        public int Score { get; set; }
    }
}
