/*
This is an app for Daryo.uz

Author: Aziz Yakubjanov
*/





using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
namespace Daryo
{
    public sealed partial class MainPage : Page
    {

        int currentPage = 1;
        string currentCat;
        int lastPage; //last page of this category
        string pageTitle; //title of category or search
        List<string> URLs = new List<string>(); //links to articles
        List<News> newsList = new List<News>(); //list of articles


        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;

        }


        private static void UpdateTile(List<HtmlNode> headers)
        {
            List<string> newNews = new List<string>();
            for (int i = 0; i < 5; i++)
            {
                newNews.Add(WebUtility.HtmlDecode(headers[i].InnerText));
            }
            // create the instance of Tile Updater, which enables you to change the appearance of the calling app's tile
            var updater = TileUpdateManager.CreateTileUpdaterForApplication();

            // enables the tile to queue up to five notifications
            updater.EnableNotificationQueue(true);

            updater.Clear();

            // get the XML content of one of the predefined tile templates, so that, you can customize it

            for (int i = 0; i < 5; i++)
            {
                XmlDocument tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare150x150Text04);
                tileXml.GetElementsByTagName("text")[0].InnerText = newNews[i];
                // Create a new tile notification. 
                updater.Update(new TileNotification(tileXml));
            }

        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (Frame.BackStackDepth < 1)
            {
                pageTitle = "So'nggi yangiliklar";
                Title.Text = pageTitle;
                currentCat = "yangiliklar";
                Ring.IsActive = true;
                GetData("http://daryo.uz/yangiliklar/");
                currentPage = 1;
                Back.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                Ring.IsActive = false;
            }

            var statusbar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView(); //getting status bar to change its color
            statusbar.ForegroundColor = Windows.UI.Color.FromArgb(255, 9, 74, 178);
            await statusbar.ShowAsync();

        }



        public async void GetData(string pageURL) //parse data from given page
        {
            Ring.IsActive = true;

            try
            {
                List.Children.Clear();
                newsList.Clear();
                URLs.Clear();

                var req = new HttpClient(); // making request
                var message = new HttpRequestMessage(HttpMethod.Get, pageURL);
                message.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:40.0) Gecko/20100101 Firefox/40.0");
                var response = await req.SendAsync(message);

                HtmlDocument doc = new HtmlDocument(); //creating an instance of HtmlDocument of HtmlAgilityPack library
                doc.Load(await response.Content.ReadAsStreamAsync());
                var htmlRoot = doc.DocumentNode;



                var headers = htmlRoot.Descendants() //getting titles of articles
                              .Where(x => x.Attributes["class"] != null && x.Attributes["class"].Value == "cat_article_title").ToList();

                if (headers.Count > 1)
                {
                    UpdateTile(headers); //updating tile with passing news titles
                }

                foreach (var item in headers)
                {
                    News news = new News();
                    news.title = WebUtility.HtmlDecode(item.InnerText);
                    news.URL = item.Descendants("a").First().Attributes["href"].Value;
                    newsList.Add(news); //Adding to List<News>
                }



                var dates = htmlRoot.Descendants() //getting posted dates of each article on a page
                            .Where(x => x.Attributes["class"] != null && x.Attributes["class"].Value == "meta_date")
                            .ToList();
                for (int i = 0; i < dates.Count(); i++)
                {
                    newsList[i].date = dates[i].InnerText;
                }



                var cats = htmlRoot.Descendants() //getting category of each article
                           .Where(x => x.Attributes["class"] != null && x.Attributes["class"].Value == "meta_cat")
                           .ToList();
                for (int i = 0; i < cats.Count; i++)
                {
                    newsList[i].cat = cats[i].InnerText;
                }



                var images = htmlRoot.Descendants() //getting image for each article
                             .Where(x => x.Attributes["class"] != null && x.Attributes["class"].Value == "cat_img")
                             .ToList();
                for (int i = 0; i < images.Count; i++)
                {
                    newsList[i].img = images[i].Descendants("img").First().Attributes["src"].Value;
                }



                var descriptions = htmlRoot.Descendants() //getting short description and URL for each article
                                   .Where(x => x.Attributes["class"] != null && x.Attributes["class"].Value == "cat_article_content")
                                   .ToList();
                for (int i = 0; i < descriptions.Count; i++)
                {
                    newsList[i].desc = WebUtility.HtmlDecode(descriptions[i].Descendants("p").First().InnerText.Trim());
                    newsList[i].URL = descriptions[i].Descendants("a").First().Attributes["href"].Value;
                    URLs.Add(newsList[i].URL);
                }




                for (int q = 0; q < newsList.Count; q++) //add articles to XAML page
                {
                    StackPanel main = new StackPanel(); main.Name = q.ToString(); main.Margin = new Thickness(0, 15, 0, 0); main.Orientation = Orientation.Vertical;

                    TextBlock title = new TextBlock(); title.Text = WebUtility.HtmlDecode(newsList[q].title); title.FontSize = 18; title.TextWrapping = TextWrapping.Wrap; title.Name = q.ToString(); title.Tapped += title_Tapped;

                    main.Children.Add(title);

                    StackPanel dateNcat = new StackPanel() { Orientation = Orientation.Horizontal };

                    TextBlock date = new TextBlock() { FontSize = 15, Text = WebUtility.HtmlDecode(newsList[q].date), Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 9, 74, 178)) };

                    TextBlock category = new TextBlock() { FontSize = 15, Text = WebUtility.HtmlDecode(newsList[q].cat), Foreground = new SolidColorBrush(Windows.UI.Colors.Gray), Margin = new Thickness(10, 0, 0, 0) };

                    dateNcat.Children.Add(date); dateNcat.Children.Add(category);
                    main.Children.Add(dateNcat);

                    Grid imgNtext = new Grid() { Margin = new Thickness(0, 3, 0, 0) };

                    Image img = new Image() { Source = new BitmapImage(new Uri(newsList[q].img)), Height = 120, Width = 130, HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Left, Name = q.ToString() };

                    TextBlock text = new TextBlock()
                    {
                        Text = WebUtility.HtmlDecode(newsList[q].desc),
                        Margin = new Thickness(135, 0, 0, 0),
                        HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Left,
                        VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Top,
                        TextWrapping = TextWrapping.Wrap,
                        FontSize = 14,
                        Name = q.ToString()
                    };


                    imgNtext.Children.Add(text);
                    imgNtext.Children.Add(img);
                    main.Children.Add(imgNtext);
                    List.Children.Add(main);



                    img.Tapped += img_Tapped;
                    text.Tapped += title_Tapped;
                }

                lastPage = int.Parse(Regex.Replace( /**/(htmlRoot.Descendants().Where(x => x.Attributes["class"] != null && x.Attributes["class"].Value == "pagination").First().Descendants("a").Where(x => !x.InnerText.Contains("aquo")).Last().InnerText)/**/ , @"\D", ""));


                if (currentPage == lastPage)
                {
                    Forward.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                }
                if (currentPage == 1)
                {
                    Back.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    Title.Text = Regex.Replace(Title.Text, " -.*", "", RegexOptions.Singleline);
                }
                else
                {
                    Title.Text = pageTitle + " - " + currentPage;
                }

            }
            catch (System.Net.WebException)
            {
                MessageDialog msg = new MessageDialog("Internet o'chiq, yo'ki tizim xatosi");
                msg.Title = "Xato";
                msg.ShowAsync();
            }
            Ring.IsActive = false;
        }

        void img_Tapped(object sender, TappedRoutedEventArgs e) //image of article tapped
        {
            string URL = URLs[int.Parse((sender as Image).Name)];
            Frame.Navigate(typeof(ReadNews), URL);
        }

        void title_Tapped(object sender, TappedRoutedEventArgs e) //title of article tapped
        {
            string URL = URLs[int.Parse((sender as TextBlock).Name)];
            Frame.Navigate(typeof(ReadNews), URL);
        }


        private void Back_Click(object sender, RoutedEventArgs e) //one page back
        {
            Forward.Click -= Forward_Click;
            Back.Click -= Back_Click;
            Refresh.Click -= Refresh_Click;

            if (currentPage != 1)
            {
                Ring.IsActive = true;
                if (currentCat == "yangiliklar")
                {
                    GetData("http://daryo.uz/" + currentCat + "/page/" + (currentPage - 1));
                }
                else
                {
                    GetData("http://daryo.uz/category/" + currentCat + "/page/" + (currentPage - 1));
                }
                currentPage--;
                Forward.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }

            Forward.Click += Forward_Click;
            Back.Click += Back_Click;
            Refresh.Click += Refresh_Click;
        }

        private void Forward_Click(object sender, RoutedEventArgs e) //one page forward
        {
            Forward.Click -= Forward_Click;
            Back.Click -= Back_Click;
            Refresh.Click -= Refresh_Click;

            Ring.IsActive = true;
            if (currentCat == "yangiliklar")
            {
                GetData("http://daryo.uz/" + currentCat + "/page/" + (currentPage + 1));
            }
            else
            {
                GetData("http://daryo.uz/category/" + currentCat + "/page/" + (currentPage + 1));
            }
            currentPage++;
            Back.Visibility = Windows.UI.Xaml.Visibility.Visible;

            Forward.Click += Forward_Click;
            Back.Click += Back_Click;
            Refresh.Click += Refresh_Click;
        }

        private void Refresh_Click(object sender, RoutedEventArgs e) //refresh the content
        {
            Forward.Click -= Forward_Click;
            Back.Click -= Back_Click;
            Refresh.Click -= Refresh_Click;

            Ring.IsActive = true;
            if (currentCat == "yangiliklar")
            {
                GetData("http://daryo.uz/" + currentCat);
            }
            else
            {
                GetData("http://daryo.uz/category/" + currentCat);
            }

            currentPage = 1;
            Back.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            Ring.IsActive = false;

            Forward.Click += Forward_Click;
            Back.Click += Back_Click;
            Refresh.Click += Refresh_Click;
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e) //Button in Bottom Menu
        {
            (sender as AppBarButton).Click -= AppBarButton_Click;
            Forward.Click -= Forward_Click;
            Back.Click -= Back_Click;
            Refresh.Click -= Refresh_Click;

            string cat = (sender as AppBarButton).Label;
            pageTitle = cat;
            Title.Text = pageTitle;
            currentPage = 1;
            Ring.IsActive = true;
            switch (cat)
            {
                case "So'nggi yangiliklar":
                    currentCat = "yangiliklar";
                    break;
                case "Mahalliy":
                    currentCat = "mahalliy";
                    break;
                case "Dunyo":
                    currentCat = "dunyo";
                    break;
                case "Texnologiyalar":
                    currentCat = "gadjetlar";
                    break;
                case "Musiqa va Kino":
                    currentCat = "shou-biznes";
                    break;
                case "Avto":
                    currentCat = "avto";
                    break;
                case "Sport":
                    currentCat = "sport";
                    break;
            }


            if (currentCat != "yangiliklar")
            {
                GetData("http://daryo.uz/category/" + currentCat);
            }
            else
            {
                GetData("http://daryo.uz/" + currentCat);
            }
            pageTitle = cat;
            Title.Text = pageTitle;
            currentPage = 1;
            Back.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            Ring.IsActive = false;

            (sender as AppBarButton).Click += AppBarButton_Click;
            Forward.Click += Forward_Click;
            Back.Click += Back_Click;
            Refresh.Click += Refresh_Click;
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Search));
        }

        private void About_Click(object sender, RoutedEventArgs e) //About program
        {
            Frame.Navigate(typeof(About));
        }

        private void OpenByLink_Click(object sender, RoutedEventArgs e) //'open article by link' button tapped
        {
            Frame.Navigate(typeof(Link));
        }

    }
}
