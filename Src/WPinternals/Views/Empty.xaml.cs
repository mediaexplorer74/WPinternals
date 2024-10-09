// Empty usercontrol

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace WPinternals
{
    /// <summary>
    /// Interaction logic for Empty.xaml
    /// </summary>
    public partial class Empty : UserControl
    {
        // Dependency injection is not possible here, because this ViewModel is used in a Style.
        public Empty()
        {
            InitializeComponent();

            InterruptBoot = App.InterruptBoot;
        }

        private void HandleHyperlinkClick(object sender, RoutedEventArgs args)
        {
            Hyperlink link = args.Source as Hyperlink;
            if (link != null)
            {
                if (link.NavigateUri.ToString() == "Getting started")
                    App.NavigateToGettingStarted();
                else if (link.NavigateUri.ToString() == "Unlock boot")
                    App.NavigateToUnlockBoot();
                else if (link.NavigateUri.ToString() == "Interrupt boot")
                    InterruptBoot = true;
                else if (link.NavigateUri.ToString() == "Normal boot")
                    InterruptBoot = false;
            }
        }

        private void Document_Loaded(object sender, RoutedEventArgs e)
        {
            (sender as FlowDocument).AddHandler(Hyperlink.ClickEvent, new RoutedEventHandler(HandleHyperlinkClick));
        }

        public static readonly DependencyProperty InterruptBootProperty =
            DependencyProperty.Register("InterruptBoot", typeof(Boolean), typeof(Empty), 
                new FrameworkPropertyMetadata(InterruptBootChanged));
        public bool InterruptBoot
        {
            get
            {
                return (bool)GetValue(InterruptBootProperty);
            }
            set
            {
                SetValue(InterruptBootProperty, value);
            }
        }

        internal static void InterruptBootChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            App.InterruptBoot = (bool)e.NewValue;

            if ((bool)e.NewValue)
            {
                // Find the phone notifier
                DependencyObject obj = d;

                while (!(obj is MainWindow))
                {
                    try
                    {
                        obj = VisualTreeHelper.GetParent(obj);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("[ex] Empty Usercontrol error: " + ex.Message);
                    }
                }


                PhoneNotifierViewModel PhoneNotifier = ((MainViewModel)(((MainWindow)obj).DataContext)).PhoneNotifier;

                if (PhoneNotifier.CurrentInterface == PhoneInterfaces.Lumia_Bootloader)
                {
                    App.InterruptBoot = false;

                    LogFile.Log("Found Lumia BootMgr and user forced to interrupt the boot process. " +
                        "Force to Flash-mode.");
                    
                    Task.Run(() => SwitchModeViewModel.SwitchTo(PhoneNotifier, PhoneInterfaces.Lumia_Flash));
                }
            }
        }
    }
}
