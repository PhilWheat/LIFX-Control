﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace LIFX
{
    public enum NetworkState { UnInitialized, Discovery, Initialized };

    public class LIFXBulb
    {
        public byte[] bulbMac;
        public byte[] bulbGateWay;
        public IPEndPoint bulbEndpoint;
        public UInt16 hue;
        public UInt16 saturation;
        public UInt16 brightness;
        public UInt16 kelvin;
        public UInt16 time_delay;
        public UInt16 power;
        public UInt16 dim;
        public byte power_state;
        public string label;
        public UInt64 tags;
    }
    public class LIFXNetwork
    {
        public NetworkState State = NetworkState.UnInitialized;
        IPEndPoint GateWayEndPoint;
        IPAddress GateWayAddress;
        byte[] GateWayMac;

        public Queue<LIFXPacket> OutPackets = new Queue<LIFXPacket>();
        public Queue<LIFXPacket> InPackets = new Queue<LIFXPacket>();
        LIFXPacketFactory PacketFactory = new LIFXPacketFactory();

        byte[] readBuffer = new byte[200];
        int readBytes = 0;

        public List<LIFXBulb> bulbs = new List<LIFXBulb>();
        char[] charsToTrim = { '\0'};

        public void DiscoverNetwork()
        {
            State = NetworkState.Discovery;
            bulbs.Clear();

            // prep a discovery packet
            LIFX_GetPANGateWay discoveryPacket = (LIFX_GetPANGateWay)PacketFactory.Getpacket(0x02);
            discoveryPacket.protocol = 21504;

            // Determine local subnet
            string localIP = LocalIPAddress();
            var pos = localIP.LastIndexOf('.');
            if (pos >= 0)
                localIP = localIP.Substring(0, pos);
            localIP = localIP + ".255";

            // Set up UDP connection.
            Socket sending_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPAddress send_to_address = IPAddress.Parse(localIP);
            IPEndPoint sending_end_point = new IPEndPoint(send_to_address, 56700);

            byte [] sendData = PacketFactory.PacketToBuffer(discoveryPacket);
            sending_socket.SendTo(sendData, sending_end_point);


            // Now listen for the response
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            UdpClient receivingUdpClient = new UdpClient(56700);
            byte[] receivebytes = receivingUdpClient.Receive(ref RemoteIpEndPoint);
            LIFXPacket ReceivedPacket = PacketFactory.Getpacket(receivebytes);

            GateWayMac = ReceivedPacket.site;
            GateWayAddress = RemoteIpEndPoint.Address;
            InPackets.Enqueue(ReceivedPacket);
            LIFXBulb bulb = new LIFXBulb();
            bulb.bulbMac = ReceivedPacket.target_mac_address;
            bulb.bulbGateWay = ReceivedPacket.site;
            bulb.bulbEndpoint = new IPEndPoint(RemoteIpEndPoint.Address, 56700);
            bulbs.Add(bulb);
            Thread.Sleep(100);
            while (receivingUdpClient.Available > 0)
            {
                // We've got what we need, just clean up any additional packets.
                // This can happen if there is more than one bulb acting as a gateway.
                receivebytes = receivingUdpClient.Receive(ref RemoteIpEndPoint);
                ReceivedPacket = PacketFactory.Getpacket(receivebytes);
                InPackets.Enqueue(ReceivedPacket);
                
                if (!bulbs.Any(p=>p.bulbMac.SequenceEqual(ReceivedPacket.target_mac_address)))
                {
                        bulb = new LIFXBulb();
                        bulb.bulbMac = ReceivedPacket.target_mac_address;
                        bulb.bulbGateWay = ReceivedPacket.site;
                        bulb.bulbEndpoint = new IPEndPoint(RemoteIpEndPoint.Address, 56700);
                        bulbs.Add(bulb);
                }
                Thread.Sleep(100);
            }

            State = NetworkState.Initialized;
            sending_socket.Close();
            receivingUdpClient.Close();
            //InPackets.Clear();
        }

        public void Inventory()
        {
            GateWayEndPoint = new IPEndPoint(GateWayAddress, 56700);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(bulbs[0].bulbEndpoint);

            LIFXPacket SendPacket = PacketFactory.Getpacket(0x65);
            SendPacket.site = GateWayMac;
            

            if (!socket.Connected)
            {
                socket.Close();
                socket.Connect(GateWayEndPoint);
            }

            foreach (LIFXBulb bulb in bulbs)
            {

                //SendPacket.target_mac_address = bulb.bulbMac;
                if (!(bulb.bulbEndpoint == GateWayEndPoint))
                {
                    socket.Close();
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    socket.Connect(bulb.bulbEndpoint);
                    GateWayEndPoint = bulb.bulbEndpoint;
                }

                socket.Send(PacketFactory.PacketToBuffer(SendPacket));

                readBytes = socket.Receive(readBuffer);
                InPackets.Enqueue( PacketFactory.Getpacket(readBuffer));
                LIFX_LightStatus packet = (LIFX_LightStatus)PacketFactory.Getpacket(readBuffer);
                bulb.label = packet.bulb_label.TrimEnd(charsToTrim);
                bulb.hue = packet.hue;
                bulb.saturation = packet.saturation;
                bulb.kelvin = packet.kelvin;
                bulb.brightness = packet.brightness;
                bulb.power = packet.power;
                bulb.dim = packet.dim;
                bulb.tags = packet.tags;

            }
            Thread.Sleep(100);

            while (socket.Available > 0)
            {
                LIFXBulb bulb = new LIFXBulb();
                readBytes = socket.Receive(readBuffer);
                try
                {
                    LIFX_LightStatus packet = (LIFX_LightStatus)PacketFactory.Getpacket(readBuffer);

                    if (!bulbs.Any(p => p.bulbMac.SequenceEqual(packet.target_mac_address)))
                    {
                        bulb = new LIFXBulb();
                        bulb.bulbGateWay = packet.site;
                        bulb.bulbMac = packet.target_mac_address;
                        bulb.bulbEndpoint = GateWayEndPoint;
                        bulb.label = packet.bulb_label.TrimEnd(charsToTrim);
                        bulb.hue = packet.hue;
                        bulb.saturation = packet.saturation;
                        bulb.kelvin = packet.kelvin;
                        bulb.brightness = packet.brightness;
                        bulb.power = packet.power;
                        bulb.dim = packet.dim;
                        bulb.tags = packet.tags;
                        bulbs.Add(bulb);
                    }
                    InPackets.Enqueue(PacketFactory.Getpacket(readBuffer));
                }
                catch (Exception e)
                { }
            }
            socket.Close();

        }

        public void SetAllBulbValues(UInt16 hue, UInt16 saturation, UInt16 brightness, UInt16 kelvin, UInt32 fade)
        {
            LIFX_SetLightColor setpacket = (LIFX_SetLightColor)PacketFactory.Getpacket(0x66);
            setpacket.hue = hue;
            setpacket.saturation = saturation;
            setpacket.brightness = brightness;
            setpacket.kelvin = kelvin;
            setpacket.fadeTime = ((fade) * 225) ^ 2;
            setpacket.site = GateWayMac;
            setpacket.size = 49;
            foreach (LIFXBulb bulb in bulbs)
            {
                setpacket.target_mac_address = bulb.bulbMac;
                SendPacket(bulb, setpacket);
            }

        }

        public void SendPacket(LIFXBulb bulb, LIFXPacket outPacket)
        {
            if (outPacket.protocol == 21504)
            {
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Udp);
                socket.Connect(bulb.bulbEndpoint);
                socket.Send(PacketFactory.PacketToBuffer(outPacket));
                socket.Close();
            }
            else
            {
                // Otherwise default to TCP
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(bulb.bulbEndpoint);
                socket.Send(PacketFactory.PacketToBuffer(outPacket));
                socket.Close();
            }
        }

        public string LocalIPAddress()
        {
            IPHostEntry host;
            string localIP = "";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            return localIP;
        }

    }
}
