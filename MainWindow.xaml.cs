using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;

namespace NMController
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private UdpClient udpClient;
        private IPEndPoint endPoint;
        private Thread udpThread;
        private bool keepListening = true;
        public ObservableCollection<NMDevice> nmDevices = new ObservableCollection<NMDevice>();
        CollectionViewSource view = new CollectionViewSource();

        private UdpClient _udpClient2;
        private Dictionary<string, NMParam> nmParams = new Dictionary<string, NMParam>();
        private List<Window> childWindows = new List<Window>();

        private System.Timers.Timer checkTimer;
        private const int offlineTimeout = 60;

        public MainWindow()
        {
            InitializeComponent();
            InitializeTimer();

            view.Source = nmDevices;
            this.deviceListView.DataContext = view;

            // Start listening to UDP messages
            udpClient = new UdpClient(12345);
            _udpClient2 = new UdpClient(12346);
            endPoint = new IPEndPoint(IPAddress.Any, 0);
            udpThread = new Thread(new ThreadStart(RecvNMDeviceMsg));
            udpThread.Start();

            Task.Run(() =>
            {
                while (keepListening)
                {
                    try
                    {
                        IPEndPoint remoteEndPoint = null;
                        byte[] bytes = _udpClient2.Receive(ref remoteEndPoint);
                        string message = Encoding.UTF8.GetString(bytes);
                        NMParam param = JsonConvert.DeserializeObject<NMParam>(message);
                        if (param != null)
                        {
                            nmParams[param.IP] = param;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error: " + e.Message);
                    }
                }
            });

            this.Closing += MainWindow_Closing;
        }

        private void InitializeTimer()
        {
            checkTimer = new System.Timers.Timer(5000); 
            checkTimer.Elapsed += CheckDeviceTimeout;
            checkTimer.AutoReset = true;
            checkTimer.Enabled = true;
        }

        private void CheckDeviceTimeout(object sender, ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var device in nmDevices)
                {
                    DateTime updateTime;
                    if (DateTime.TryParse(device.UpdateTime, out updateTime))
                    {
                        if ((DateTime.Now - updateTime).TotalSeconds > offlineTimeout)
                        {
                            device.Offline = true;
                        }
                        else
                        {
                            device.Offline = false;
                        }
                    }
                }
            });
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            foreach (var window in childWindows)
            {
                if (window != null)
                {
                    window.Close();
                }
            }
        }

        private void clearAll_Click(object sender, RoutedEventArgs e)
        {
            nmDevices.Clear();
        }

        private void RecvNMDeviceMsg()
        {
            while (keepListening)
            {
                try
                {
                    byte[] bytes = udpClient.Receive(ref endPoint);
                    string message = Encoding.ASCII.GetString(bytes);

                    NMDevice device = JsonConvert.DeserializeObject<NMDevice>(message);
                    if (device != null)
                    {
                        device.UpdateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        bool isContains = false;
                        for (int i = 0; i < nmDevices.Count; i++)
                        {
                            if (nmDevices[i].IP == device.IP)
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    nmDevices[i].HashRate = device.HashRate;
                                    nmDevices[i].BoardType = device.BoardType;
                                    nmDevices[i].Share = device.Share;
                                    nmDevices[i].NetDiff = device.NetDiff;
                                    nmDevices[i].PoolDiff = device.PoolDiff;
                                    nmDevices[i].LastDiff = device.LastDiff;
                                    nmDevices[i].BestDiff = device.BestDiff;
                                    nmDevices[i].Valid = device.Valid;
                                    nmDevices[i].Progress = device.Progress;
                                    nmDevices[i].Temp = device.Temp;
                                    nmDevices[i].RSSI = device.RSSI;
                                    nmDevices[i].FreeHeap = device.FreeHeap;
                                    nmDevices[i].Uptime = device.Uptime;
                                    nmDevices[i].UpdateTime = device.UpdateTime;
                                    nmDevices[i].Version = device.Version;
                                    nmDevices[i].PoolInUse = device.PoolInUse;
                                    nmDevices[i].Offline = false;
                                });
                                isContains = true;
                                break;
                            }
                        }
                        if (!isContains)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                nmDevices.Add(device);
                            });
                        }
                    }
                }
                catch (SocketException e)
                {
                    Console.WriteLine("SocketException: " + e.Message);
                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: " + e.Message);
                }
            }
        }

        // Add this method to handle the Closing event
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            keepListening = false;
            try
            {
                udpClient.Close();
                _udpClient2.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            NMDevice device = deviceListView.SelectedItem as NMDevice;
            if (device != null)
            {
                // Navigate to the IP address...
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("http://" + device.IP) { UseShellExecute = true });
            }
        }

        GridViewColumnHeader _lastHeaderClicked;
        ListSortDirection _lastDirection;
        private void GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            var headerClicked = e.OriginalSource as GridViewColumnHeader;
            ListSortDirection direction;

            if (headerClicked != null)
            {
                if (headerClicked != _lastHeaderClicked)
                {
                    direction = ListSortDirection.Ascending;
                }
                else
                {
                    if (_lastDirection == ListSortDirection.Ascending)
                    {
                        direction = ListSortDirection.Descending;
                    }
                    else
                    {
                        direction = ListSortDirection.Ascending;
                    }
                }

                var columnBinding = headerClicked.Column.DisplayMemberBinding as Binding;
                var sortBy = columnBinding?.Path.Path ?? headerClicked.Column.Header as string;

                if (sortBy == "RSSI(dBm)")
                {
                    sortBy = "RSSI";
                }

                if (sortBy == "BestDiff" || sortBy == "NetDiff" || sortBy == "LastDiff" || sortBy == "HashRate" || sortBy == "Share")
                {
                    SortHash(sortBy, direction);
                }
                else
                {
                    Sort(sortBy, direction);
                }

                _lastHeaderClicked = headerClicked;
                _lastDirection = direction;
            }
        }

        private void SortHash(string sortBy, ListSortDirection direction)
        {
            ListCollectionView dataView = (ListCollectionView)CollectionViewSource.GetDefaultView(deviceListView.ItemsSource);

            if (dataView == null)
            {
                return;
            }

            dataView.SortDescriptions.Clear();
            if (sortBy == "HashRate")
            {
                dataView.CustomSort = new HashRateComparer(direction);
            }
            else if (sortBy == "Share")
            {
                dataView.CustomSort = new ShareComparer(direction);
            }
            else
            {
                dataView.CustomSort = new HashComparer(direction, sortBy);
            }

            dataView.Refresh();
        }
        private void Sort(string sortBy, ListSortDirection direction)
        {
            ICollectionView dataView = CollectionViewSource.GetDefaultView(deviceListView.ItemsSource);

            dataView.SortDescriptions.Clear();
            SortDescription sd = new SortDescription(sortBy, direction);
            dataView.SortDescriptions.Add(sd);

            dataView.Refresh();
        }

        private void OpenConfigWindow(NMParam param)
        {
            ConfigWnd devConfigWnd = new ConfigWnd(param);
            devConfigWnd.Closed += (sender, e) =>
            {
                childWindows.Remove(devConfigWnd);
            };
            devConfigWnd.Show();
            childWindows.Add(devConfigWnd);
        }

        private void configDevice_Click(object sender, RoutedEventArgs e)
        {
            NMDevice device = deviceListView.SelectedItem as NMDevice;
            if (device != null)
            {
                if (nmParams.ContainsKey(device.IP))
                {
                    OpenConfigWindow(nmParams[device.IP]);
                }
                else
                {
                    NMParam param = new NMParam();
                    param.IP = device.IP;
                    param.WiFiSSID = "NMTech-2.4G";
                    param.WiFiPWD = "NMMiner2048";
                    param.PrimaryPool = "stratum+tcp://public-pool.io:21496";
                    param.PrimaryPassword = "x";
                    param.PrimaryAddress = "18dK8EfyepKuS74fs27iuDJWoGUT4rPto1";
                    param.SecondaryPool = "stratum+tcp://pool.tazmining.ch:33333";
                    param.SecondaryPassword = "x";
                    param.SecondaryAddress = "18dK8EfyepKuS74fs27iuDJWoGUT4rPto1";
                    param.Timezone = 8;
                    param.UIRefresh = 2;
                    param.ScreenTimeout = 60;
                    param.Brightness = 100;
                    param.SaveUptime = true;
                    param.LedEnable = true;
                    param.RotateScreen = false;
                    param.BTCPrice = true;
                    param.AutoBrightness = true;
                    OpenConfigWindow(param);
                }
            }
        }

        private void Facebook_Click(object sender, MouseButtonEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://www.facebook.com/groups/650385280668297") { UseShellExecute = true });
        }

        private void Github_Click(object sender, MouseButtonEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://github.com/NMminer1024/NMMiner") { UseShellExecute = true });
        }

        private void Reddit_Click(object sender, MouseButtonEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://www.reddit.com/r/NMTech_Team/") { UseShellExecute = true });
        }

        private void Telegram_Click(object sender, MouseButtonEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://t.me/NMMiner") { UseShellExecute = true });
        }

        private void Mail_Click(object sender, MouseButtonEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://www.facebook.com/groups/650385280668297") { UseShellExecute = true });
        }

        private void Youtube_Click(object sender, MouseButtonEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://www.youtube.com/@NMTech-official") { UseShellExecute = true });
        }

        private void Mail_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        private void Download_Click(object sender, MouseButtonEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://flash.nmminer.com") { UseShellExecute = true });
        }
    }
}
