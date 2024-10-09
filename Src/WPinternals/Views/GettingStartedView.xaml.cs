// GettingStartedView usercontrol

using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace WPinternals
{
    /// <summary>
    /// Interaction logic for GettingStartedView.xaml
    /// </summary>
    public partial class GettingStartedView : UserControl
    {
        public GettingStartedView()
        {
            InitializeComponent();
        }

        private void HandleHyperlinkClick(object sender, RoutedEventArgs args)
        {
            Hyperlink link = args.Source as Hyperlink;
            if (link != null)
            {
                switch (link.NavigateUri.ToString())
                {
                    case "Disclaimer":
                        (this.DataContext as GettingStartedViewModel).ShowDisclaimer();
                        break;
                    case "UnlockBoot":
                        (this.DataContext as GettingStartedViewModel).SwitchToUnlockBoot();
                        break;
                    case "UnlockRoot":
                        (this.DataContext as GettingStartedViewModel).SwitchToUnlockRoot();
                        break;
                    case "Backup":
                        (this.DataContext as GettingStartedViewModel).SwitchToBackup();
                        break;
                    case "Flash":
                        (this.DataContext as GettingStartedViewModel).SwitchToFlashRom();
                        break;
                    case "Dump":
                        (this.DataContext as GettingStartedViewModel).SwitchToDumpRom();
                        break;
                    case "Download":
                        (this.DataContext as GettingStartedViewModel).SwitchToDownload();
                        break;
                    default:
                        Process.Start(link.NavigateUri.AbsoluteUri);
                        break;
                }
            }
        }

        private void Document_Loaded(object sender, RoutedEventArgs e)
        {
            (sender as FlowDocument).AddHandler(Hyperlink.ClickEvent, 
                new RoutedEventHandler(HandleHyperlinkClick));
        }

        private void TextBlock_Loaded(object sender, RoutedEventArgs e)
        {
            (sender as TextBlock).AddHandler(Hyperlink.ClickEvent, 
                new RoutedEventHandler(HandleHyperlinkClick));
        }

        private void BulletDecorator_Loaded(object sender, RoutedEventArgs e)
        {
            (sender as System.Windows.Controls.Primitives.BulletDecorator)
                .AddHandler(Hyperlink.ClickEvent, new RoutedEventHandler(HandleHyperlinkClick));
        }
    }
}
