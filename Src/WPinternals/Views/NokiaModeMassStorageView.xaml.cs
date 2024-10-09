// NokiaModeMassStorageView usercontrol

using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace WPinternals
{
    /// <summary>
    /// Interaction logic for NokiaModeMassStorageView.xaml
    /// </summary>
    public partial class NokiaModeMassStorageView : UserControl
    {
        public NokiaModeMassStorageView()
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
