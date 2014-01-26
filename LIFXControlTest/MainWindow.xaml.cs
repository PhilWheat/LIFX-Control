﻿using System;
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
                Network.Inventory();
                PacketInfo.Text = PacketInfo.Text + System.Environment.NewLine + Network.InPackets.Count + " Inventory Packets Received";
                Change.IsEnabled = true; Cycle.IsEnabled = true;
                ConnectBtn.Content = "Connected";
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
            Network.SetAllBulbValues(Convert.ToUInt16(HueValue.Text), Convert.ToUInt16(SaturationValue.Text), Convert.ToUInt16(BrightnessValue.Text), Convert.ToUInt16(KelvinValue.Text), Convert.ToUInt32(FadeValue.Text));
        }

        private void Cycle_Click(object sender, RoutedEventArgs e)
        {
            colorcycle = !colorcycle;
            if (colorcycle)
            {
                for (UInt16 i = 0; i < 65500; i += 100)
                {
                    Network.SetAllBulbValues(Convert.ToUInt16(i), Convert.ToUInt16(SaturationValue.Text), Convert.ToUInt16(BrightnessValue.Text), Convert.ToUInt16(KelvinValue.Text), Convert.ToUInt32(FadeValue.Text));
                    CycleValue.Text = i.ToString();

                    Thread.Sleep(200);
                }
            }
        }

        private void Value_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Network.SetAllBulbValues(Convert.ToUInt16(HueValue.Text), Convert.ToUInt16(SaturationValue.Text), Convert.ToUInt16(BrightnessValue.Text), Convert.ToUInt16(KelvinValue.Text), Convert.ToUInt32(FadeValue.Text));
            }
        }

    }
}
