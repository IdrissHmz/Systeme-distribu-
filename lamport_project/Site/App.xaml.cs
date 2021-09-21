using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ExclusionMutuelle
{
    /// <summary>
    /// Logique d'interaction pour App.xaml
    /// </summary>
    public partial class App : Application
    {
        public string[] args;
        protected override void OnStartup(StartupEventArgs e)
        {
            args = e.Args;
            ExclusionMutuelle.MainWindow.config = this;
        }
    }
}
