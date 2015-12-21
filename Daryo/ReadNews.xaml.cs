/*
This is an app for Daryo.uz

Author: Aziz Yakubjanov
*/



using HtmlAgilityPack;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using Windows.ApplicationModel.DataTransfer;
using Windows.Phone.UI.Input;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace Daryo
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class ReadNews : Page
    {
        public ReadNews()
        {
            this.InitializeComponent();
        }
        string URL;
        string html;
        DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView(); //DataTransferManager for sharing link

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            HardwareButtons.BackPressed += HardwareButtons_BackPressed;
            URL = e.Parameter.ToString();
            GetArticle(URL);
            dataTransferManager.DataRequested += dataTransferManager_DataRequested;
        }
        private void dataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs e)
        {
            DataPackage requestData = e.Request.Data;
            requestData.Properties.Title = Title.Text;
            requestData.Properties.Description = "Tavsiya qilish.";
            requestData.SetText(" - " + URL + "\r\n-Windows Phone uchun ilova-");
        }

        void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
            e.Handled = true;
        }

        async void GetArticle(string articleSource)
        {
            try
            {
                Ring.IsActive = true; //Progress ring is active

                var req = new HttpClient();
                var message = new HttpRequestMessage(HttpMethod.Get, articleSource);
                message.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:40.0) Gecko/20100101 Firefox/40.0");
                message.Headers.Add("Referer", "http://daryo.uz/");
                message.Headers.Add("Host", "daryo.uz");
                var response = await req.SendAsync(message);

                var doc = new HtmlDocument(); //Html Document for HtmlAgilityPack
                doc.Load(await response.Content.ReadAsStreamAsync());
                var rootHTML = doc.DocumentNode;

                string title = rootHTML.Descendants() //getting article title
                               .Where(x => x.Attributes["class"] != null && x.Attributes["class"].Value == "cat_article_title")
                               .First()
                               .InnerText;
                Title.Text = WebUtility.HtmlDecode(title); //applying title to XAML page



                var articleContent = rootHTML.Descendants() //getting article content
                              .Where(x => x.Attributes["id"] != null && x.Attributes["id"].Value == "article_content")
                              .First();



                string date = rootHTML.Descendants()  //article posted date
                               .Where(x => x.Attributes["class"] != null && x.Attributes["class"].Value == "meta_date")
                               .First()
                               .InnerText.Trim();
                Date.Text = WebUtility.HtmlDecode(date);



                string category = rootHTML.Descendants() //getting category of the article
                               .Where(x => x.Attributes["class"] != null && x.Attributes["class"].Value == "meta_cat")
                               .First()
                               .InnerText.Trim();
                Category.Text = WebUtility.HtmlDecode(category);


                var images = rootHTML.Descendants().Where(x => x.Attributes["id"] != null && x.Attributes["id"].Value == "article_content").First().Descendants("img").ToList(); //getting all images from article content
                var paragraphs = rootHTML.Descendants()   //getting all paragraphs
                                 .Where(x => x.Attributes["id"] != null && x.Attributes["id"].Value == "article_content")
                                 .First()
                                 .Descendants("p")
                                 .Where(x => x.Attributes["class"] == null || x.Attributes["class"] != null && x.Attributes["class"].Value != "wp-caption-text")
                                 .ToList();
                paragraphs.RemoveAt(0); //removing social media script
                paragraphs.RemoveAt(paragraphs.Count - 1); //removing social media script



                for (int i = 0; i < paragraphs.Count; i++)   //displaying paragraphs + images for it
                {
                    TextBlock Paragraph = new TextBlock()   //creating TextBlock for each paragraph
                    {
                        FontSize = 16,
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(0, 8, 0, 0),
                        HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Left,
                        Text = "" + WebUtility.HtmlDecode(paragraphs[i].InnerText.Trim())
                    };
                    Content.Children.Add(Paragraph);

                    if (i < images.Count)   //checking if image exists for this paragraph
                    {
                        Image img = new Image()
                        {
                            Source = new BitmapImage(new Uri(images[i].Attributes["src"].Value)),
                            Margin = new Thickness(20, 0, 20, 0),
                            HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center
                        };
                        img.Tapped += img_Tapped;
                        Content.Children.Add(img);
                    }
                }

                if (images.Count > paragraphs.Count)   //in case that there are more images than paragraphs
                {
                    for (int i = images.Count - paragraphs.Count - 1; i < images.Count; i++)
                    {
                        Image img = new Image()
                        {
                            Source = new BitmapImage(new Uri(images[i].Attributes["src"].Value)),
                            Margin = new Thickness(20, 0, 20, 0),
                            HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center
                        };
                        img.Tapped += img_Tapped;
                        Content.Children.Add(img);
                    }
                }
            }
            catch (System.Net.WebException)
            {
                MessageDialog msg = new MessageDialog("Internet o'chiq, yo'ki tizim xatosi"); //connection error
                msg.Title = "Xato";
                msg.ShowAsync();
            }

            BottomBar.Visibility = Windows.UI.Xaml.Visibility.Visible; //Bottom Menu
            Ring.IsActive = false; //Turn off Progress Ring
        }

        async void img_Tapped(object sender, TappedRoutedEventArgs e) //Zoom image when tapped
        {
            Zoomed.Source = (sender as Image).Source;
            Zoomer.Visibility = Windows.UI.Xaml.Visibility.Visible;
            DarkLayer.Visibility = Windows.UI.Xaml.Visibility.Visible;
            await Windows.UI.ViewManagement.StatusBar.GetForCurrentView().HideAsync();
            BottomBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private async void Zoomed_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)   //stop zooming when double tapped
        {
            Zoomer.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            DarkLayer.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            await Windows.UI.ViewManagement.StatusBar.GetForCurrentView().ShowAsync();
            BottomBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        private void ShareButton_Click(object sender, RoutedEventArgs e)
        {
            DataTransferManager.ShowShareUI();
        }

    }
}
