using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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

            var keyLists = Directory.EnumerateFiles(".", "*.keys.json")
                .Select(f => Path.GetFileName(f.Replace(".keys.json", "")));

            KeyListsCombo.ItemsSource = keyLists;
            KeyListsCombo.Text = keyListName = App.StartingList ?? "Default";

            MasterBox.Focus();

            loadList();
        }

        private void KeyListsCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            keyListName = (sender as ComboBox)?.SelectedValue as string;

            loadList();
        }

        private void KeyListsCombo_KeyUp(object sender, KeyEventArgs e)
        {
            keyListName = (sender as ComboBox)?.Text as string;

            loadList();
        }

        private void loadList()
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

            try { generatePass(MasterBox.Password, key); }
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

        private void generatePass(string master, Key key)
        {
            if (!checkMaster(master))
            {
                MessageBox.Show("Master Password check missmatch!", "Error", MessageBoxButton.OK);
                return;
            }

            var bytes = new HMACSHA256(master.ToUTF8()).ComputeHash(key.Label.ToUTF8());

            var pass = key.GenMode switch
            {
                GenMode.Base64 => bytes.ToBase64(),
                GenMode.AlphaNum => bytes.ToBase64().Replace("/", "").Replace("+", "").Replace("=", ""),
                _ => throw new Exception("Unexpected GenMode"),
            };

            if (key.MaxLength != null)
                pass = pass.Substring(0, key.MaxLength.Value);

            Clipboard.SetText(pass);
            MessageBox.Show("Copied to Clipboard!", "Done!");
        }

        private bool checkMaster(string input)
        {
            // Get Salt Bytes or Generate new one
            if (keyList.Master.Salt is null)
            {
                var salt = RandomNumberGenerator.GetBytes(32);
                keyList.Master.Salt = salt.ToBase64();
            }

            // Compute Input Master Hash
            var inputHash = this.computeMasterHash(input);

            // If this is a new Key List, save the Hash and return true
            if (keyList.Master.Hash is null)
            {
                keyList.Master.Hash = inputHash;
                return true;
            }

            if (keyList.Version < KeyList.CurrVersion)
            {
                var legacyHash = keyList.Master.Hash;

                keyList.Version = KeyList.CurrVersion;
                keyList.Master.Hash = this.computeMasterHash(input);

                return inputHash == legacyHash;
            }

            return inputHash == keyList.Master.Hash;
        }

        private string computeMasterHash(string input)
        {
            var inputBytes = input.ToUTF8();
            var salt = keyList.Master.Salt.FromBase64();
            var iters = keyList.Master.IterCount;

            if (keyList.Version is 0)
                return new HMACSHA256(inputBytes).ComputeHash("CHECK".ToUTF8()).ToBase64();

            if (keyList.Version is 1)
            {
                var sha = SHA256.Create();
                return Enumerable.Range(0, iters).Aggregate(new byte[0], (hash, _) =>
                    sha.ComputeHash(concatBuffers(hash, inputBytes, salt))).ToBase64();
            }

            if (keyList.Version is 2)
                return new Rfc2898DeriveBytes(inputBytes, salt, iters,
                    HashAlgorithmName.SHA256).GetBytes(256 / 8).ToBase64();

            throw new Exception("Unexpected Version");
        }

        private byte[] concatBuffers(params byte[][] buffers)
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
