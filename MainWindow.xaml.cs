using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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

        public MainWindow()
        {
            InitializeComponent();
            view.Source = nmDevices;
            this.deviceListView.DataContext = view;

            // Start listening to UDP messages
            udpClient = new UdpClient(12345);
            endPoint = new IPEndPoint(IPAddress.Any, 0);
            udpThread = new Thread(new ThreadStart(RecvNMDeviceMsg));
            udpThread.Start();

            this.Closing += MainWindow_Closing;
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
    }
}
