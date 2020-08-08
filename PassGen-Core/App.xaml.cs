using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PassGenCore
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string StartingList { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            if (e.Args.Length == 1)
            {
                StartingList = e.Args[0];
            }

            if (e.Args.Length == 2)
            {
                PassGenCore.MainWindow.GetPass(e.Args[0], e.Args[1]);
                Shutdown();
            }

            base.OnStartup(e);
        }
    }
}
