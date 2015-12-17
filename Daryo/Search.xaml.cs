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

using System.Text.RegularExpressions;
using System.Net;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Popups;
using Windows.Phone.UI.Input;

namespace Daryo
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class Search : Page
    {
        public Search()
        {
            this.InitializeComponent();
        }

        DispatcherTimer timer = new DispatcherTimer();

        protected override void OnNavigatedTo(NavigationEventArgs e)        
        {
            HardwareButtons.BackPressed += HardwareButtons_BackPressed;
            timer.Interval = new TimeSpan(0,0,0,0,500);
            timer.Tick += timer_Tick;
            timer.Start();
            currentPage = 1;
            Back.Visibility = Windows.UI.Xaml.Visibility.Collapsed;            
        }

        void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
            e.Handled = true;
        }

        void timer_Tick(object sender, object e)
        {
            timer.Stop();
            SearchBox.Focus(Windows.UI.Xaml.FocusState.Keyboard);
        }



        List<News> newsList = new List<News>();
        string html;
        int lastPage;
        int currentPage;
        public async void GetData(string page)
        {

            try
            {
                List.Children.Clear();
                newsList.Clear();
                URLs.Clear();
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(page);
                System.Net.HttpWebRequest.DefaultWebProxy = null;
                request.Proxy = null;
                WebResponse x = await request.GetResponseAsync();
                HttpWebResponse response = x as HttpWebResponse;
                StreamReader reader = new StreamReader(response.GetResponseStream());
                html = await reader.ReadToEndAsync();
                MatchCollection headers = Regex.Matches(html, "<h2 class=\"cat_article_title\".*?</h2>", RegexOptions.Singleline);
                if (headers.Count < 1)
                {
                    MessageDialog msg = new MessageDialog("Hech narsa topilmadi");
                    msg.Title = "Topilmadi";
                    await msg.ShowAsync();
                    return;
                }
                foreach (Match item in headers)
                {
                    News news = new News();
                    news.title = Regex.Replace(item.Value, "<.*?>", "", RegexOptions.Singleline);
                    news.URL = Regex.Match(item.Value, "href=\".*?\">").Value.Replace("href=", "").Replace("\"", "").Replace(">", "");
                    newsList.Add(news);
                }
                MatchCollection dates = Regex.Matches(html, "<span class=\"meta_date\">.*?</span>", RegexOptions.Singleline);
                for (int i = 0; i < dates.Count; i++)
                {
                    newsList[i].date = Regex.Replace(dates[i].Value, "<.*?>", "", RegexOptions.Singleline);
                }

                MatchCollection cats = Regex.Matches(html, "<span class=\"meta_cat\">.*?</span>", RegexOptions.Singleline);
                for (int i = 0; i < cats.Count; i++)
                {
                    newsList[i].cat = Regex.Replace(cats[i].Value, "<.*?>", "", RegexOptions.Singleline);
                }

                MatchCollection images = Regex.Matches(html, "<div class=\"cat_img\">.*?</div>", RegexOptions.Singleline);
                for (int i = 0; i < images.Count; i++)
                {
                    newsList[i].img = Regex.Match(images[i].Value, "src=\".*?\"", RegexOptions.Singleline).Value.Replace("src=", "").Replace("\"", "");
                }

                MatchCollection descs = Regex.Matches(html, "<div class=\"cat_article_content\">.*?</div>", RegexOptions.Singleline);
                for (int i = 0; i < descs.Count; i++)
                {
                    string text = descs[i].Value;
                    text = Regex.Replace(text, "<a.*?</a>", "", RegexOptions.Singleline); text = Regex.Replace(text, "<.*?>", "", RegexOptions.Singleline); text = System.Net.WebUtility.HtmlDecode(text);
                    text = text.Replace("  ", "").Replace("\n", "");
                    newsList[i].desc = text;
                    string URL = Regex.Match(descs[i].Value, "href=\".*?\"", RegexOptions.Singleline).Value.Replace("href=", "").Replace("\"", "");
                    newsList[i].URL = URL;
                    URLs.Add(newsList[i].URL);
                }

                int q = 0;
                foreach (var item in newsList)
                {
                    StackPanel main = new StackPanel(); main.Name = q.ToString(); main.Margin = new Thickness(0, 15, 0, 0); main.Orientation = Orientation.Vertical;
                    TextBlock title = new TextBlock(); title.Text = WebUtility.HtmlDecode(item.title); title.FontSize = 18; title.TextWrapping = TextWrapping.Wrap; title.Name = q.ToString(); title.Tapped += title_Tapped;
                    main.Children.Add(title);
                    StackPanel dateNcat = new StackPanel() { Orientation = Orientation.Horizontal };
                    TextBlock date = new TextBlock() { FontSize = 15, Text = WebUtility.HtmlDecode(item.date), Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 9, 74, 178)) };
                    TextBlock cat = new TextBlock() { FontSize = 15, Text = WebUtility.HtmlDecode(item.cat), Foreground = new SolidColorBrush(Windows.UI.Colors.Gray), Margin = new Thickness(10, 0, 0, 0) };
                    dateNcat.Children.Add(date); dateNcat.Children.Add(cat);
                    main.Children.Add(dateNcat);
                    Grid imgNtext = new Grid() { Margin = new Thickness(0, 3, 0, 0) };
                    Image img = new Image() { Source = new BitmapImage(new Uri(item.img)), Height = 120, Width = 130, HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Left, Name = q.ToString()};
                    TextBlock text = new TextBlock()
                    {
                        Text = WebUtility.HtmlDecode(item.desc),
                        Margin = new Thickness(135, 0, 0, 0),
                        HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Left,
                        VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Top,
                        TextWrapping = TextWrapping.Wrap,
                        FontSize = 14,
                        Name = q.ToString()
                    };

                    img.Tapped += img_Tapped;
                    text.Tapped += title_Tapped;

                    imgNtext.Children.Add(text);
                    imgNtext.Children.Add(img);
                    main.Children.Add(imgNtext);
                    List.Children.Add(main);
                    q++;
                }
                string pages = Regex.Match(html, "<div class='pagination'>.*?</div>", RegexOptions.Singleline).Value;
                MatchCollection links = Regex.Matches(pages, "<a.*?</a>", RegexOptions.Singleline);
                int lastLink = links.Count - 1;
                string last = Regex.Replace(links[lastLink].Value, "s=.*","",RegexOptions.Singleline);
                last = Regex.Replace(last, @"\D", "", RegexOptions.Singleline);
                lastPage = int.Parse(last);
                if (lastPage > 1 && currentPage != lastPage)
                {
                    Forward.Visibility = Windows.UI.Xaml.Visibility.Visible;
                }

                Last.Text = lastPage.ToString();
                Current.Text = currentPage.ToString();
                if (currentPage == lastPage)
                {
                    Forward.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                }
                if (currentPage == 1)
                {
                    Back.Visibility = Windows.UI.Xaml.Visibility.Collapsed;                    
                }                
            }
            catch
            {
                MessageDialog msg = new MessageDialog("Internet o'chiq, yo'ki tizim xatosi");
                msg.Title = "Xato";
                msg.ShowAsync();
            }
            Ring.IsActive = false;
        }

        void img_Tapped(object sender, TappedRoutedEventArgs e)
        {
            string URL = URLs[int.Parse((sender as Image).Name)];
            Frame.Navigate(typeof(ReadNews), URL);
        }

        void title_Tapped(object sender, TappedRoutedEventArgs e)
        {
            string URL = URLs[int.Parse((sender as TextBlock).Name)];
            Frame.Navigate(typeof(ReadNews), URL);
        }

        private void SearchBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                query = WebUtility.UrlEncode(SearchBox.Text);
                GetData("http://daryo.uz/page/1/?s=" + query);
                LooseFocus.Focus(Windows.UI.Xaml.FocusState.Programmatic);
            }            
        }
        string query;
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            Forward.Click -= Forward_Click;
            Back.Click -= Back_Click;

            Ring.IsActive = true;
            GetData("http://daryo.uz/page/" + (currentPage - 1) + "/?s=" + query);

            currentPage--;
            Back.Visibility = Windows.UI.Xaml.Visibility.Visible;

            Forward.Click += Forward_Click;
            Back.Click += Back_Click;
        }

        private void Forward_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(SearchBox.Text))
            {
                Forward.Click -= Forward_Click;
                Back.Click -= Back_Click;

                Ring.IsActive = true;
                GetData("http://daryo.uz/page/" + (currentPage + 1) + "/?s=" + query);

                currentPage++;
                Back.Visibility = Windows.UI.Xaml.Visibility.Visible;

                Forward.Click += Forward_Click;
                Back.Click += Back_Click;
            }
        }
        List<string> URLs = new List<string>();
    }
}
