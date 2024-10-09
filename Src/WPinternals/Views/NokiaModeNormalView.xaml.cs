// NokiaModeNormalView usercontrol

using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace WPinternals
{
    /// <summary>
    /// Interaction logic for NokiaModeNormalView.xaml
    /// </summary>
    public partial class NokiaModeNormalView : UserControl
    {
        public NokiaModeNormalView()
        {
            InitializeComponent();
        }

        private void HandleHyperlinkClick(object sender, RoutedEventArgs args)
        {
            Hyperlink link = args.Source as Hyperlink;
            if (link != null)
            {
                (this.DataContext as NokiaModeNormalViewModel).RebootTo(link.NavigateUri.ToString());
            }
        }

        private void Document_Loaded(object sender, RoutedEventArgs e)
        {
            (sender as FlowDocument).AddHandler(Hyperlink.ClickEvent, 
                new RoutedEventHandler(HandleHyperlinkClick));
        }
    }
}
