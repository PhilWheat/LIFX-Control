using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using LIFX;
using System.Threading;

namespace LIFXControlTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        LIFXNetwork Network = new LIFXNetwork();
        bool colorcycle = false;
        public MainWindow()
        {
            InitializeComponent();


            if (Network.State != NetworkState.Initialized)
            {
                Change.IsEnabled = false;
                Cycle.IsEnabled = false;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                Network.DiscoverNetwork();
                PacketInfo.Text = Network.InPackets.Count + " Discovery Packets Received";
                Network.InPackets.Clear();
                if (Network.State == NetworkState.Initialized)
                {
                    Network.Inventory();
                    Thread.Sleep(100);
                    Network.Inventory();
                    Change.IsEnabled = true; Cycle.IsEnabled = true;
                    ConnectBtn.Content = "Connected";
                }
                PacketInfo.Text = PacketInfo.Text + System.Environment.NewLine + Network.InPackets.Count + " Inventory Packets Received";
                Status.Text = "Number of Bulbs: " + Network.bulbs.Count();
                string bulbList = "";
                foreach (LIFXBulb bulb in Network.bulbs)
                {
                    bulbList += bulb.label + "   :   " + BitConverter.ToString(bulb.bulbGateWay) + "   :   " + BitConverter.ToString(bulb.bulbMac) + "   :   " + bulb.bulbEndpoint.Address.ToString() + System.Environment.NewLine;
                }
                GeneralInfo.Text = bulbList;
            }
            else
            {
                Status.Text = "Not connected to a network";
            }


        }

        private void Change_Click(object sender, RoutedEventArgs e)
        {
            Network.SetAllBulbValues(Convert.ToUInt16(HueValue.Text), Convert.ToUInt16(SaturationValue.Text), Convert.ToUInt16(BrightnessValue.Text), Convert.ToUInt16(KelvinValue.Text), Convert.ToUInt32(FadeValue.Text), Convert.ToUInt16(PacketDelay.Text));
        }

        private void Cycle_Click(object sender, RoutedEventArgs e)
        {
            int step = Convert.ToInt32(CycleStep.Text);
            int cycleDelay = 200 - (Network.bulbs.Count * Convert.ToInt32(PacketDelay.Text));
            if (cycleDelay < 0)
            { cycleDelay = 0; }
            //colorcycle = !colorcycle;
            //if (colorcycle)
            //{
                for (int i = 0; i < 65500; i += step)
                {
                    // Note, overriding fade value here - just to smooth out the color transitions.
                    Network.SetAllBulbValues(Convert.ToUInt16(i), Convert.ToUInt16(SaturationValue.Text), Convert.ToUInt16(BrightnessValue.Text), Convert.ToUInt16(KelvinValue.Text), 100, Convert.ToUInt16(PacketDelay.Text));
                    CycleValue.Text = i.ToString();

                    Thread.Sleep(cycleDelay);
                }
            //}
        }

        private void Value_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Network.SetAllBulbValues(Convert.ToUInt16(HueValue.Text), Convert.ToUInt16(SaturationValue.Text), Convert.ToUInt16(BrightnessValue.Text), Convert.ToUInt16(KelvinValue.Text), Convert.ToUInt32(FadeValue.Text), Convert.ToUInt16(PacketDelay.Text));
            }
        }

    }
}
