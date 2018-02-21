using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AngleSharp;
using AngleSharp.Parser.Html;
using LoxoneStatisticLoader.Annotations;
using Path = System.IO.Path;

namespace LoxoneStatisticLoader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public Dictionary<string, Statistic> Stats = new Dictionary<string, Statistic>();
        
        public ObservableCollection<Statistic> Statistics = new ObservableCollection<Statistic>();
        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;
        }

        private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Stats.Clear();
            Statistics.Clear();

            MainGrid.IsEnabled = false;

            var address = $"http://{AddressUrl.Text}/stats/";

            String encoded = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(User.Text + ":" + Password.Password));
            var webRequest = WebRequest.Create(address) as HttpWebRequest;
            webRequest.Method = "GET";
            webRequest.ProtocolVersion = HttpVersion.Version11;
            webRequest.Headers.Add("Authorization", "Basic " + encoded);

            using (var response = await webRequest.GetResponseAsync())
            {

                var stream = response.GetResponseStream();
                var streamReader = new StreamReader(stream);
                var html = streamReader.ReadToEnd();


                var htmlParser = new HtmlParser();
                var htmlDoc = htmlParser.Parse(html);

                var lis = htmlDoc.QuerySelectorAll("li");

                foreach (var li in lis)
                {
                    var a = li.QuerySelector("a");

                    var url = a.Attributes["href"].Value;

                    var split = url.Split(new[] {"."}, StringSplitOptions.None);

                    if (split.Length == 3)
                    {
                        var guid = split[0];

                        if (!Stats.ContainsKey(guid))
                        {
                            var st = new Statistic
                            {
                                Name = a.InnerHtml.Remove(a.InnerHtml.Length - 7),
                                Guid = guid
                            };
                            Stats.Add(guid, st);

                            Statistics.Add(st);
                        }
                        Stats[guid].Files.Add(new StatisticFile(Stats[guid])
                        {
                            Name = a.InnerHtml,
                            Url = address + url
                        });

                    }

                }

            }
            var root = new RootStatstic();
            root.Files.AddRange(Statistics.ToList());
            TreeView1.ItemsSource = new List<RootStatstic> {root};
            MainGrid.IsEnabled = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void Download_OnClick(object sender, RoutedEventArgs e)
        {
            MainGrid.IsEnabled = false;
            var curAs = Assembly.GetExecutingAssembly().Location;

            var info = new FileInfo(curAs);
            var dir = Path.Combine(info.DirectoryName, DateTime.Now.ToString("ddmmyy_hhMMss"));

            Directory.CreateDirectory(dir);
            String encoded =
                Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1")
                    .GetBytes(User.Text + ":" + Password.Password));




            foreach (var stat in Statistics.Where(a => a.IsChecked.HasValue && a.IsChecked.Value))
            {
                var statDir = Path.Combine(dir, stat.Name);
                Directory.CreateDirectory(statDir);


                var files = stat.Files.Where(a => a.IsChecked.HasValue && a.IsChecked.Value).ToList();
                foreach (var file in files)
                {
                    Console.WriteLine($"{DateTime.Now} loading file {file.Name} for stat {stat.Name}");

                    var webRequest = WebRequest.Create(file.Url) as HttpWebRequest;
                    webRequest.Method = "GET";
                    webRequest.ProtocolVersion = HttpVersion.Version11;
                    webRequest.Headers.Add("Authorization", "Basic " + encoded);

                    using (var response = await webRequest.GetResponseAsync())
                    {
                        var stream = response.GetResponseStream();
                        var streamReader = new StreamReader(stream);
                        var html = streamReader.ReadToEnd();

                        using (StreamWriter writer = new StreamWriter(Path.Combine(statDir, file.Name)))
                        {
                            writer.Write(html);
                        }
                    }
                }
            }


            MainGrid.IsEnabled = true;

        }

        static async Task InvokeAsync(IEnumerable<Func<Task>> taskFactories, int maxDegreeOfParallelism)
        {
            Queue<Func<Task>> queue = new Queue<Func<Task>>(taskFactories);

            if (queue.Count == 0)
            {
                return;
            }

            List<Task> tasksInFlight = new List<Task>(maxDegreeOfParallelism);

            do
            {
                while (tasksInFlight.Count < maxDegreeOfParallelism && queue.Count != 0)
                {
                    Func<Task> taskFactory = queue.Dequeue();

                    tasksInFlight.Add(taskFactory());
                }

                Task completedTask = await Task.WhenAny(tasksInFlight).ConfigureAwait(false);

                // Propagate exceptions. In-flight tasks will be abandoned if this throws.
                await completedTask.ConfigureAwait(false);

                tasksInFlight.Remove(completedTask);
            }
            while (queue.Count != 0 || tasksInFlight.Count != 0);
        }
    }
}
