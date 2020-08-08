using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace PassGen
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string keyList;
        private string keyListPath => $"{keyList}.keys";
        private List<string> keys;

        public MainWindow()
        {
            InitializeComponent();

            var keyLists = Directory.EnumerateFiles(".", "*.keys")
                .Select(f => Path.GetFileNameWithoutExtension(f)).ToList();

            KeyListsCombo.ItemsSource = keyLists;
            KeyListsCombo.Text = App.StartingList ?? "Default";

            MasterBox.Focus();
        }

        private void KeyListsCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            keyList = (sender as ComboBox)?.SelectedValue as string;

            loadList();
        }

        private void KeyListsCombo_KeyUp(object sender, KeyEventArgs e)
        {
            keyList = (sender as ComboBox)?.Text as string;

            loadList();
        }

        private void loadList()
        {
            keys = File.Exists(keyListPath) ?
                File.ReadAllLines(keyListPath).ToList() : new List<string>();

            KeysCombo.ItemsSource = keys.ElementAtOrDefault(1) == "" ?
                keys.Skip(2) : keys;
        }

        private void GenBtn_Click(object sender, RoutedEventArgs e)
        {
            var check = GetPass(MasterBox.Password, "CHECK", false);

            if (keys.Count < 2 || keys[1] != "")
                keys.InsertRange(0, new[] { check, "" });
            else if (keys[0] == "")
                keys[0] = check;
            else if (check != keys[0])
                if (MessageBox.Show("Master Password check missmatch!\r\nGenerate anyways?",
                    "Error", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                    return;

            var key = KeysCombo.Text;

            if (!keys.Contains(key))
                keys.Add(key);

            File.WriteAllLines(keyListPath, keys);

            GetPass(MasterBox.Password, key);
        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public static string GetPass(string master, string key, bool clipboard = true)
        {
            var utf8 = Encoding.UTF8;
            var hmac = new HMACSHA256(utf8.GetBytes(master));
            var bytes = hmac.ComputeHash(utf8.GetBytes(key));
            var pass = Convert.ToBase64String(bytes);

            if (clipboard)
            {
                Clipboard.SetText(pass);
                MessageBox.Show("Copied to Clipboard!", "Done!");
            }

            return pass;
        }
    }
}
