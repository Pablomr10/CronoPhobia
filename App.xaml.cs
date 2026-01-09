using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Cronophobia
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            DispatcherUnhandledException += (s, ex) =>
            {
                MessageBox.Show(
                    ex.Exception.ToString(),
                    "Error crítico",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                ex.Handled = true;
            };

            base.OnStartup(e);
        }
    }
}
