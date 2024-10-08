﻿// Copyright (c) 2018, Rene Lergner - wpinternals.net - @Heathcliff74xda
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace WPinternals
{
    /// <summary>
    /// Interaction logic for DumpRomView.xaml
    /// </summary>
    public partial class DumpRomView : UserControl
    {
        public DumpRomView()
        {
            InitializeComponent();
        }

        private void HandleHyperlinkClick(object sender, RoutedEventArgs args)
        {
            Hyperlink link = args.Source as Hyperlink;
            if (link != null)
            {
                if (link.NavigateUri.ToString() == "UnlockBoot")
                    (this.DataContext as DumpRomTargetSelectionViewModel).SwitchToUnlockBoot();
                else if (link.NavigateUri.ToString() == "UnlockRoot")
                    (this.DataContext as DumpRomTargetSelectionViewModel).SwitchToUnlockRoot();
                else if (link.NavigateUri.ToString() == "FlashRom")
                    (this.DataContext as DumpRomTargetSelectionViewModel).SwitchToFlashRom();
            }
        }

        private void Document_Loaded(object sender, RoutedEventArgs e)
        {
            (sender as FlowDocument).AddHandler(Hyperlink.ClickEvent, new RoutedEventHandler(HandleHyperlinkClick));
        }

        private void FilePicker_PathChanged(object sender, PathChangedEventArgs e)
        {
            ((DumpRomTargetSelectionViewModel)DataContext).EvaluateViewState();
        }

        private void CompressEFIESP_Changed(object sender, RoutedEventArgs e)
        {
            EFIESPPicker.DefaultFileName = "EFIESP.bin" + ((CompressEFIESP.IsChecked == true) ? ".pz" : "");
        }

        private void CompressMainOS_Changed(object sender, RoutedEventArgs e)
        {
            MainOSPicker.DefaultFileName = "MainOS.bin" + ((CompressMainOS.IsChecked == true) ? ".pz" : "");
        }

        private void CompressData_Changed(object sender, RoutedEventArgs e)
        {
            DataPicker.DefaultFileName = "Data.bin" + ((CompressData.IsChecked == true) ? ".pz" : "");
        }
    }
}
