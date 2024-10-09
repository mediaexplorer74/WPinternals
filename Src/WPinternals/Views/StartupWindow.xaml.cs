// StartupWindow window

using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace WPinternals
{
    /// <summary>
    /// Interaction logic for StartupWindow.xaml
    /// </summary>
    public partial class StartupWindow : Window
    {
        public StartupWindow()
        {
            InitializeComponent();
        }

        protected override async void OnSourceInitialized(EventArgs e)
        {
            Visibility = System.Windows.Visibility.Hidden;

            bool NeedLicenseAgrement = false;
            if (Registry.CurrentUser.OpenSubKey("Software\\WPInternals") == null)
                Registry.CurrentUser.OpenSubKey("Software", true).CreateSubKey("WPInternals");

            if ((Registration.IsPrerelease)
                && (Registry.CurrentUser.OpenSubKey("Software\\WPInternals").GetValue("NdaAccepted") == null))
            {
                NeedLicenseAgrement = true;
            }
            else if (Registry.CurrentUser.OpenSubKey("Software\\WPInternals")
                .GetValue("DisclaimerAccepted") == null)
            {
                NeedLicenseAgrement = true;
            }

            if ((!Registration.IsPrerelease || Registration.IsRegistered()) && !NeedLicenseAgrement)
            {
                // USB communication uses Windows Messages and therefore the MainViewModel
                // can only be created after the Main Window was initialized.
                System.Threading.SynchronizationContext UIContext 
                    = System.Threading.SynchronizationContext.Current;

                await CommandLine.ParseCommandLine(UIContext);
            }
            else if (Environment.GetCommandLineArgs().Count() > 1)
            {
                Debug.WriteLine("[i] First time use");
                Debug.WriteLine("[i] Switching to graphic user interface for license and registration");
                //CommandLine.CloseConsole(false);
            }

            new MainWindow().Show();
        }
    }
}
