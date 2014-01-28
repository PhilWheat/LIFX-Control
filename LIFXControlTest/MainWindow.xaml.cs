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
using System.Windows.Threading;
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
        DispatcherTimer dispatchTimer;
        public MainWindow()
        {
            InitializeComponent();

            //_refreshTimer = new Timer(Timed_Refresh);
            //_refreshTimer.Change(0, 5000);
            dispatchTimer = new System.Windows.Threading.DispatcherTimer();
            dispatchTimer.Tick += new EventHandler(Timed_Refresh);
            dispatchTimer.Interval = new TimeSpan(0, 0, 1);
            dispatchTimer.Start();

            if (Network.State != NetworkState.Initialized)
            {
                Change.IsEnabled = false;
                Cycle.IsEnabled = false;
            }
        }


        public void Timed_Refresh(object sender, EventArgs e)
        {
            if (Network != null)
            {
                if (Network.bulbs != null)
                {
                    Status.Text = "Number of Bulbs: " + Network.bulbs.Count();
                    bulbListBox.Items.Clear();
                    foreach (LIFXBulb bulb in Network.bulbs)
                    {
                        bulbListBox.Items.Add(bulb);
                    }
                    bulbListBox.SelectAll();
                }
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
                    Thread.Sleep(1000);
                    Network.Inventory();
                    Change.IsEnabled = true; Cycle.IsEnabled = true;
                    ConnectBtn.Content = "Connected";
                }
                PacketInfo.Text = PacketInfo.Text + System.Environment.NewLine + Network.InPackets.Count + " Inventory Packets Received";
                Status.Text = "Number of Bulbs: " + Network.bulbs.Count();
                bulbListBox.Items.Clear();
                foreach (LIFXBulb bulb in Network.bulbs)
                {
                    bulbListBox.Items.Add(bulb);
                }
                bulbListBox.SelectAll();
            }
            else
            {
                Status.Text = "Not connected to a network";
            }
        }

        private void Change_Click(object sender, RoutedEventArgs e)
        {
            foreach (LIFXBulb bulb in bulbListBox.SelectedItems)
            {
                Network.SetBulbValues(Convert.ToUInt16(HueValue.Text), Convert.ToUInt16(SaturationValue.Text), Convert.ToUInt16(BrightnessValue.Text), Convert.ToUInt16(KelvinValue.Text), Convert.ToUInt32(FadeValue.Text), bulb);
                Thread.Sleep(Convert.ToUInt16(PacketDelay.Text));
            }
        }

        private void Cycle_Click(object sender, RoutedEventArgs e)
        {
            int step = Convert.ToInt32(CycleStep.Text);
            int cycleDelay = 200 - (Network.bulbs.Count * Convert.ToInt32(PacketDelay.Text));
            if (cycleDelay < 0)
            { cycleDelay = 0; }
            for (int i = 0; i < 65500; i += step)
            {
                // Note, overriding fade value here - just to smooth out the color transitions.
                foreach (LIFXBulb bulb in bulbListBox.SelectedItems)
                {
                    Network.SetBulbValues(Convert.ToUInt16(i), Convert.ToUInt16(SaturationValue.Text), Convert.ToUInt16(BrightnessValue.Text), Convert.ToUInt16(KelvinValue.Text), 100, bulb);
                    Thread.Sleep(Convert.ToUInt16(PacketDelay.Text));
                }
                CycleValue.Text = i.ToString();

                Thread.Sleep(cycleDelay);
            }
        }

        private void Value_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                foreach (LIFXBulb bulb in bulbListBox.SelectedItems)
                {
                    Network.SetBulbValues(Convert.ToUInt16(HueValue.Text), Convert.ToUInt16(SaturationValue.Text), Convert.ToUInt16(BrightnessValue.Text), Convert.ToUInt16(KelvinValue.Text), Convert.ToUInt32(FadeValue.Text), bulb);
                    Thread.Sleep(Convert.ToUInt16(PacketDelay.Text));
                }
            }
        }

    }
}
