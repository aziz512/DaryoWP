/*
This is an app for Daryo.uz

Author: Aziz Yakubjanov
*/

using System;
using Windows.Phone.UI.Input;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
namespace Daryo
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class Link : Page
    {
        public Link()
        {
            this.InitializeComponent();
        }

        DispatcherTimer timer = new DispatcherTimer();
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            HardwareButtons.BackPressed += HardwareButtons_BackPressed;
            timer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            timer.Tick += timer_Tick;
            timer.Start();

        }

        void timer_Tick(object sender, object e)
        {
            timer.Stop();
            Field.Focus(Windows.UI.Xaml.FocusState.Keyboard);
        }


        void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
            e.Handled = true;
        }

        private void Field_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            string link = Field.Text;
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                if (!String.IsNullOrWhiteSpace(Field.Text))
                {
                    if (link.Contains("daryo.uz") && link.Contains("http://"))
                    {
                        link = link.Replace(" ", "");
                        Frame.Navigate(typeof(ReadNews), link);
                    }
                    else if (link.Contains("daryo.uz") && !link.Contains("http://"))
                    {
                        link = ("http://" + link).Replace(" ", "");
                        Frame.Navigate(typeof(ReadNews), link);
                        return;
                    }
                    else
                    {
                        MessageDialog msg = new MessageDialog("To'g'ri manzilni yozing. Misol: http://daryo.uz/2015/07/29/shimoliy-koreyada-syorfingchilar-uchun-oromgoh-tashkil-etiladi/");
                        msg.ShowAsync();
                    }
                }
            }
        }
    }
}
