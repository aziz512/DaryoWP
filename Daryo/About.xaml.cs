using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel.Email;
using Windows.ApplicationModel.Chat;
using Windows.ApplicationModel.Calls;
using Windows.Phone.UI.Input;
using Windows.ApplicationModel;

namespace Daryo
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class About : Page
    {
        public About()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Вызывается перед отображением этой страницы во фрейме.
        /// </summary>
        /// <param name="e">Данные события, описывающие, каким образом была достигнута эта страница.
        /// Этот параметр обычно используется для настройки страницы.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {            
            Windows.ApplicationModel.PackageVersion version = Package.Current.Id.Version;
            Version.Text = string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);;
            HardwareButtons.BackPressed += HardwareButtons_BackPressed;
        }

        void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
            e.Handled = true;
        }

        private async void OtherApps_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("https://www.windowsphone.com/ru-RU/store/publishers?publisherId=Aziz%2BYakubjanov"));
        }

        private async void email_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Windows.ApplicationModel.Email.EmailMessage msg = new Windows.ApplicationModel.Email.EmailMessage();            
            msg.To.Add(new EmailRecipient("yakubjanov0@hotmail.com", "Aziz Yakubjanov"));
            msg.Body = "";
            msg.Subject = "Daryo ilovasi";
            await EmailManager.ShowComposeNewEmailAsync(msg);
        }

        private async void twitter_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("https://twitter.com/a_yoq"));
        }

        private async void sms_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ChatMessage msg = new ChatMessage();
            msg.Recipients.Add("+998935198015");
            await ChatMessageManager.ShowComposeSmsMessageAsync(msg);
        }

        private void call_Tapped(object sender, TappedRoutedEventArgs e)
        {
            PhoneCallManager.ShowPhoneCallUI("+998935198015", "Aziz Yakubjanov");
        }
    }
}
