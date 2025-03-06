using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NMController
{
    /// <summary>
    /// ConfigWnd.xaml 的交互逻辑
    /// </summary>
    public partial class ConfigWnd : Window
    {

        public ConfigWnd(NMParam param)
        {
            InitializeComponent();
            UpdateWndParam(param);
        }

        private void UpdateWndParam(NMParam param)
        {
            deviceIpTB.Text = param.IP;
            WifiSsidTB.Text = param.WiFiSSID;
            wifiPasswordTB.Text = param.WiFiPWD;
            PrimaryPoolTB.Text = param.PrimaryPool;
            PrimaryPoolPasswordTB.Text = param.PrimaryPassword;
            PrimaryAddrTB.Text = param.PrimaryAddress;
            SecondaryPoolTB.Text = param.SecondaryPool;
            SecondaryPoolPasswordTB.Text = param.SecondaryPassword;
            SecondaryAddrTB.Text = param.SecondaryAddress;
            TimeZoneTB.Text = param.Timezone.ToString();
            RefreshIntervalTB.Text = param.UIRefresh.ToString();
            ScreenTimeOutTB.Text = param.ScreenTimeout.ToString();
            BrightnessTB.Text = param.Brightness.ToString();
            SaveUptimeCB.IsChecked = param.SaveUptime;
            LedEnableCB.IsChecked = param.LedEnable;
            RotateScreenCB.IsChecked = param.RotateScreen;
            PriceUpdateCB.IsChecked = param.BTCPrice;
            AutoBrightnessCB.IsChecked = param.AutoBrightness;
        }

        private async Task BroadcastNMParamAsync(NMParam param, IPAddress address)
        {
            try
            {
                string json = JsonConvert.SerializeObject(param);
                byte[] data = System.Text.Encoding.UTF8.GetBytes(json);

                using (UdpClient udpClient = new UdpClient(new IPEndPoint(IPAddress.Any, 0)))
                {
                    udpClient.EnableBroadcast = true;
                    udpClient.Client.SendTimeout = 100;

                    IPEndPoint endPoint = new IPEndPoint(address, 12347);

                    for (int i = 0; i < 10; i++)
                    {
                        try
                        {
                            await udpClient.SendAsync(data, data.Length, endPoint);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Send error: {ex.Message}");
                        }

                        await Task.Delay(100);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        IPAddress GetBroadcastAddress(IPAddress address, IPAddress subnetMask)
        {
            byte[] ipAdressBytes = address.GetAddressBytes();
            byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

            byte[] broadcastAddressBytes = new byte[ipAdressBytes.Length];
            for (int i = 0; i < broadcastAddressBytes.Length; i++)
            {
                broadcastAddressBytes[i] = (byte)(ipAdressBytes[i] | (subnetMaskBytes[i] ^ 255));
            }

            return new IPAddress(broadcastAddressBytes);
        }

        private async Task configNMParam(NMParam param)
        {
            param.WiFiSSID = WifiSsidTB.Text;
            param.WiFiPWD = wifiPasswordTB.Text;
            param.PrimaryPool = PrimaryPoolTB.Text;
            param.PrimaryPassword = PrimaryPoolPasswordTB.Text;
            param.PrimaryAddress = PrimaryAddrTB.Text;
            param.SecondaryPool = SecondaryPoolTB.Text;
            param.SecondaryPassword = SecondaryPoolPasswordTB.Text;
            param.SecondaryAddress = SecondaryAddrTB.Text;
            param.SaveUptime = SaveUptimeCB.IsChecked ?? false;
            param.LedEnable = LedEnableCB.IsChecked ?? false;
            param.RotateScreen = RotateScreenCB.IsChecked ?? false;
            param.BTCPrice = PriceUpdateCB.IsChecked ?? false;
            param.AutoBrightness = AutoBrightnessCB.IsChecked ?? false;

            if (param.WiFiSSID == "" || param.WiFiPWD == "")
            {
                MessageBox.Show("WiFi Parameter (SSID or PWD) can't be empty.");
                return;
            }

            if (int.TryParse(TimeZoneTB.Text, out int timezone))
            {
                if (timezone >= -12 && timezone <= 12)
                {
                    param.Timezone = timezone;
                }
                else
                {
                    TimeZoneTB.Text = "8";
                    MessageBox.Show("TimeZone valid: [-12, 12].");
                    return;
                }
            }
            else
            {
                TimeZoneTB.Text = "8";
                MessageBox.Show("TimeZone input invalid!");
                return;
            }

            if (int.TryParse(RefreshIntervalTB.Text, out int refresh))
            {
                param.UIRefresh = refresh;
            }
            else
            {
                RefreshIntervalTB.Text = "2";
                MessageBox.Show("RefreshInterval input invalid!");
                return;
            }

            if (int.TryParse(ScreenTimeOutTB.Text, out int timeout))
            {
                param.ScreenTimeout = timeout;
            }
            else
            {
                ScreenTimeOutTB.Text = "60";
                MessageBox.Show("ScreenTimeOut input invalid!");
                return;
            }

            if (int.TryParse(BrightnessTB.Text, out int brightness))
            {
                if (brightness >= 0 && brightness <= 100)
                {
                    param.Brightness = brightness;
                }
                else
                {
                    BrightnessTB.Text = "100";
                    MessageBox.Show("Brightness valid: [0-100].");
                    return;
                }
            }
            else
            {
                BrightnessTB.Text = "100";
                MessageBox.Show("Brightness input invalid!");
                return;
            }

            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface ni in interfaces)
            {
                if (ni.OperationalStatus == OperationalStatus.Up &&
                    ni.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                    ni.Supports(NetworkInterfaceComponent.IPv4))
                {
                    IPInterfaceProperties ipProperties = ni.GetIPProperties();

                    foreach (UnicastIPAddressInformation ip in ipProperties.UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            IPAddress broadcastAddress = GetBroadcastAddress(ip.Address, ip.IPv4Mask);
                            await BroadcastNMParamAsync(param, broadcastAddress);
                        }
                    }
                }
            }

            if (param.IP == "0.0.0.0")
            {
                MessageBox.Show("Parameters Send to ALL!");
            }
            else
            {
                MessageBox.Show("Parameters Send to " + param.IP + "!");
            }
        }

        private async void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            NMParam param = new NMParam();
            param.IP = deviceIpTB.Text;
            await configNMParam(param);
            this.Close();
        }

        private async void saveAllBtn_Click(object sender, RoutedEventArgs e)
        {
            NMParam param = new NMParam();
            param.IP = "0.0.0.0";
            await configNMParam(param);
            this.Close();
        }

        private void TimezoneTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = new Regex("[^0-9-]+").IsMatch(e.Text);
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = new Regex("[^0-9]+").IsMatch(e.Text);
        }

        private void TextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            string clipboard = e.DataObject.GetData(typeof(string)) as string;
            foreach (var item in clipboard)
            {
                if (new Regex("[^0-9]+").IsMatch(item.ToString()))
                {
                    e.CancelCommand();
                    break;
                }
            }
        }
    }
}
