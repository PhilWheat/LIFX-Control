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
        DispatcherTimer dispatchTimer;
        public MainWindow()
        {
            InitializeComponent();
            //BulbSetup();
            Network.Start();
            Thread.Sleep(1000);  // Wait 1 second to get at least first bulb
            dispatchTimer = new System.Windows.Threading.DispatcherTimer();
            dispatchTimer.Tick += new EventHandler(Timed_Refresh);
            // Update once a second
            dispatchTimer.Interval = new TimeSpan(0, 0, 0, 1);
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
                    foreach (LIFXBulb bulb in Network.bulbs)
                    {
                        if (!bulbListBox.Items.Contains(bulb))
                        {
                            // Default to selected
                            bulb.UXSelected = true;
                            bulbListBox.Items.Add(bulb);
                            bulbListBox.SelectedItems.Add(bulb);
                        }
                    }
                    bulbListBox.Items.Refresh();
                }
            }
            if (Network.ColorCycleBulbs)
            {
                CycleValue.Text = "";
                foreach (LIFXBulb bulb in bulbListBox.SelectedItems)
                {
                    CycleValue.Text += bulb.Hue + "    ";
                }
            }
        }

        private void Change_Click(object sender, RoutedEventArgs e)
        {
            foreach (LIFXBulb bulb in bulbListBox.SelectedItems)
            {
                Network.SetBulbValues(Convert.ToUInt16(HueValue.Text), Convert.ToUInt16(SaturationValue.Text), Convert.ToUInt16(BrightnessValue.Text), Convert.ToUInt16(KelvinValue.Text), Convert.ToUInt32(FadeValue.Text), bulb);
                //Thread.Sleep(Convert.ToUInt16(PacketDelay.Text));
            }
        }

        private void Cycle_Click(object sender, RoutedEventArgs e)
        {
            Network.ColorCycleBulbs = !Network.ColorCycleBulbs;
            if (Network.ColorCycleBulbs)
            { 
                Cycle.Content = "Stop Cycle";
                Network.ColorCycleStep = Convert.ToUInt16(CycleStep.Text);
            }
            else
            { Cycle.Content = "Color Cycle"; }
        }

        private void Value_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (sender is TextBox)
                {
                    TextBox senderBox = sender as TextBox;
                    switch (senderBox.Name)
                    {
                        case "HueValue":
                            foreach (LIFXBulb bulb in Network.bulbs)
                            {
                                if (bulb.UXSelected)
                                { bulb.Hue = Convert.ToUInt16(HueValue.Text); }
                            }
                            break;
                        case "SaturationValue":
                            foreach (LIFXBulb bulb in bulbListBox.SelectedItems)
                            {
                                if (bulb.UXSelected)
                                { bulb.Saturation = Convert.ToUInt16(SaturationValue.Text); }
                            }
                            break;
                        case "BrightnessValue":
                            foreach (LIFXBulb bulb in bulbListBox.SelectedItems)
                            { 
                                if (bulb.UXSelected)
                                {bulb.Brightness = Convert.ToUInt16(BrightnessValue.Text); }
                            }

                            break;
                        case "KelvinValue":
                            foreach (LIFXBulb bulb in bulbListBox.SelectedItems)
                            { 
                                if (bulb.UXSelected)
                                {bulb.Kelvin = Convert.ToUInt16(KelvinValue.Text); }
                            }
                            break;
                        case "FadeValue":
                            foreach (LIFXBulb bulb in bulbListBox.SelectedItems)
                            { 
                                if (bulb.UXSelected)
                                {bulb.Time_Delay = Convert.ToUInt16(FadeValue.Text); }
                            }
                            break;
                        case "BulbLabelText":
                            foreach (LIFXBulb bulb in bulbListBox.SelectedItems)
                            { 
                                if (bulb.UXSelected)
                                {bulb.Label = BulbLabelText.Text; }
                            }
                            break;
                    }
                    //foreach (LIFXBulb bulb in bulbListBox.SelectedItems)
                    //{
                    //    Network.SetBulbValues(Convert.ToUInt16(HueValue.Text), Convert.ToUInt16(SaturationValue.Text), Convert.ToUInt16(BrightnessValue.Text), Convert.ToUInt16(KelvinValue.Text), Convert.ToUInt32(FadeValue.Text), bulb);
                    //    Thread.Sleep(Convert.ToUInt16(PacketDelay.Text));
                    //}
                }
            }
        }

        private void bulbListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (LIFXBulb bulb in e.AddedItems)
            {
                bulb.UXSelected = true;
            }
            foreach (LIFXBulb bulb in e.RemovedItems)
            {
                bulb.UXSelected = false;
            }
            // Just for the test - fill in the first current selected bulb's label
            if (bulbListBox.SelectedItems.Count > 0)
            {
                LIFXBulb b = (LIFXBulb)bulbListBox.SelectedItems[0];
                BulbLabelText.Text = b.Label;
            }

        }

        private void RenameTest_Click(object sender, RoutedEventArgs e)
        {
            //LIFX_SetBulbLabel packet = (LIFX_SetBulbLabel) LIFXPacketFactory.Getpacket(0x18);
            //packet.label = System.Text.Encoding.UTF8.GetBytes("Bulb A5");
            //packet.target_mac_address = Network.bulbs[0].BulbMac;
            //packet.site = Network.bulbs[0].BulbGateWay;
            //Network.bulbs[0].SendPacket((LIFXPacket)packet);
            //Network.Inventory();
            foreach (LIFXBulb bulb in bulbListBox.SelectedItems)
            {
                bulb.SetLabel(BulbLabelText.Text);
            }

        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (LIFXBulb bulb in bulbListBox.SelectedItems)
            {
                bulb.BatchMode = true;
                bulb.SendPacket(AppToBulb.GetPowerState);
                bulb.SendPacket(AppToBulb.GetWifiInfo);
                bulb.SendPacket(AppToBulb.GetWifiFirmwareState);
                bulb.SendPacket(AppToBulb.GetWifiState);
                bulb.SendPacket(AppToBulb.GetTags);
                bulb.SendPacket(AppToBulb.GetTagLabels); // Should have picked up Tags with the previous step, maybe delay?
                bulb.SendPacket(AppToBulb.GetAccessPoints);
                bulb.SendPacket(AppToBulb.GetTime);
                bulb.SendPacket(AppToBulb.GetResetSwitch);
                bulb.SendPacket(AppToBulb.GetMeshInfo);
                bulb.SendPacket(AppToBulb.GetMeshFirmware);
                bulb.SendPacket(AppToBulb.GetVersion);
                bulb.SendPacket(AppToBulb.GetInfo);
                bulb.SendPacket(AppToBulb.GetMCURailVoltage);
                bulb.DelaySetColorValues(24000, 65000, 10000, 0, 0, new TimeSpan(0, 0, 10));
            }
        }

    }
}
