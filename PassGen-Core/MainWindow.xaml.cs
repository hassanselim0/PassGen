using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Path = System.IO.Path;

namespace PassGenCore
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string keyListName;
        private string keyListPath => $"{keyListName}.keys.json";
        private KeyList keyList;

        public MainWindow()
        {
            InitializeComponent();

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                Converters = new List<JsonConverter> { new StringEnumConverter() },
            };

            var keyLists = Directory.EnumerateFiles(".", "*.keys")
                .Select(f => Path.GetFileNameWithoutExtension(f)).ToList();

            KeyListsCombo.ItemsSource = keyLists;
            KeyListsCombo.Text = keyListName = App.StartingList ?? "Default";

            MasterBox.Focus();
        }

        private void KeyListsCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            keyListName = (sender as ComboBox)?.SelectedValue as string;

            LoadList();
        }

        private void KeyListsCombo_KeyUp(object sender, KeyEventArgs e)
        {
            keyListName = (sender as ComboBox)?.Text as string;

            LoadList();
        }

        private void LoadList()
        {
            var legacyPath = keyListPath.Replace(".keys.json", ".keys");
            if (File.Exists(keyListPath))
                keyList = JsonConvert.DeserializeObject<KeyList>(File.ReadAllText(keyListPath));
            else if (File.Exists(legacyPath))
            {
                // Migrate from text-based Key Files
                var lines = File.ReadAllLines(legacyPath).ToList();
                keyList = new KeyList();
                keyList.Version = 0;
                keyList.Master.Hash = lines[0];
                keyList.Keys.AddRange(lines.Skip(2).Select(l => new Key { Label = l }));
            }
            else
                keyList = new KeyList();

            KeysCombo.ItemsSource = keyList.Keys;
        }

        private void GenBtn_Click(object sender, RoutedEventArgs e)
        {
            var key = KeysCombo.SelectedValue as Key;
            if (key is null)
                if (KeysCombo.Text == "")
                {
                    MessageBox.Show("Key is Empty!");
                    return;
                }
                else
                {
                    // Add new Key to Key List
                    key = new Key { Label = KeysCombo.Text };
                    keyList.Keys.Add(key);
                }

            try { GeneratePass(MasterBox.Password, key); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Error!"); }
        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Closing(object sender, EventArgs e)
        {
            File.WriteAllText(keyListPath, JsonConvert.SerializeObject(keyList));
        }

        private void GeneratePass(string master, Key key)
        {
            if (!CheckMaster(master))
            {
                var answer = MessageBox.Show(
                    "Master Password check missmatch!\r\nGenerate anyways?",
                    "Error", MessageBoxButton.YesNo);
                if (answer != MessageBoxResult.Yes)
                    return;
            }

            var bytes = new HMACSHA256(master.ToUTF8()).ComputeHash(key.Label.ToUTF8());

            var pass = key.GenMode switch
            {
                GenMode.Base64 => bytes.ToBase64(),
                GenMode.AlphaNum => bytes.ToBase64().Replace("/", "").Replace("+", "").Replace("=", ""),
                _ => throw new Exception("Unexepected GenMode"),
            };

            if (key.MaxLength != null)
                pass = pass.Substring(0, key.MaxLength.Value);

            Clipboard.SetText(pass);
            MessageBox.Show("Copied to Clipboard!", "Done!");
        }

        private bool CheckMaster(string master)
        {
            var masterBytes = master.ToUTF8();

            // Get Salt Bytes or Generate new one
            byte[] salt;
            if (keyList.Master.Salt is null)
            {
                salt = new byte[32];
                new RNGCryptoServiceProvider().GetBytes(salt);
                keyList.Master.Salt = salt.ToBase64();
            }
            else
                salt = keyList.Master.Salt.FromBase64();

            // Compute Input Master Hash
            var sha = SHA256.Create();
            var hash = new byte[0];
            for (var i = 0; i < keyList.Master.IterCount; i++)
                hash = sha.ComputeHash(ConcatBuffers(hash, masterBytes, salt));

            var hashB64 = hash.ToBase64();

            // Backward Compatibility for text-based Key Files
            if (keyList.Version == 0)
            {
                var legacyHash = keyList.Master.Hash;

                // Migrate Master Key
                keyList.Version = 1;
                keyList.Master.Hash = hashB64;

                var hashedInput = new HMACSHA256(masterBytes).ComputeHash("CHECK".ToUTF8()).ToBase64();
                return hashedInput == legacyHash;
            }

            // If this is a new Key List, save the Hash and return true
            if (keyList.Master.Hash is null)
            {
                keyList.Master.Hash = hashB64;
                return true;
            }

            return hashB64 == keyList.Master.Hash;
        }

        private byte[] ConcatBuffers(params byte[][] buffers)
        {
            var result = new byte[buffers.Sum(b => b.Length)];
            var offset = 0;
            foreach (var buffer in buffers)
            {
                Buffer.BlockCopy(buffer, 0, result, offset, buffer.Length);
                offset += buffer.Length;
            }

            return result;
        }
    }
}
