using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Path = System.IO.Path;

namespace PassGenCore;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private static readonly string keyListDir = Path.GetDirectoryName(
        System.Reflection.Assembly.GetExecutingAssembly().Location);

    private string keyListName;
    private string keyListPath => Path.Combine(keyListDir, $"{keyListName}.keys.json");
    private KeyList keyList;
    private Engine engine;

    public MainWindow()
    {
        InitializeComponent();

        JsonConvert.DefaultSettings = () => new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            Converters = new List<JsonConverter> { new StringEnumConverter() },
        };

        var keyLists = Directory.EnumerateFiles(keyListDir, "*.keys.json")
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
            foreach (var line in lines.Skip(2))
                keyList.Keys.Add(new Key { Label = line });
        }
        else
            keyList = new KeyList();

        KeysCombo.ItemsSource = keyList.Keys;

        engine = new Engine { KeyList = keyList };
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
                KeysCombo.SelectedValue = key;
            }

        try
        {
            var pass = engine.GeneratePass(key, MasterBox.Password);
            Clipboard.SetText(pass);
            MessageBox.Show("Copied to Clipboard!", "Done!");
        }
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
}
