// BusyView usercontrol

using System.Windows.Controls;

namespace WPinternals
{
    /// <summary>
    /// Interaction logic for BusyView.xaml
    /// </summary>
    public partial class BusyView : UserControl
    {
        public BusyView()
        {
            InitializeComponent();

            // Setting these properties in XAML results in an error. Why?
            GifImage.GifSource = @"/aerobusy.gif";
            GifImage.AutoStart = true;
        }
    }
}
