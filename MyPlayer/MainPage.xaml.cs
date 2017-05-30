using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MyPlayer
{


    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

      
      
  
        public MainPage()
        {
            this.InitializeComponent();
            frame.Navigate(typeof(musicPlayer));


        }

        public void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            MySplitView.IsPaneOpen = !MySplitView.IsPaneOpen;
        }

        private void PrzyciskMenu2_Click(object sender, RoutedEventArgs e)
        {
            frame.Navigate(typeof(videoPlayer));
        }

        private void PrzyciskMenu1_Click(object sender, RoutedEventArgs e)
        {
            frame.Navigate(typeof(musicPlayer));
        }
    }

  
}
