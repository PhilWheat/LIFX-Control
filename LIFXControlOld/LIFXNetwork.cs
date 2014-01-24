using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace LIFX
{
    enum NetworkState { UnInitialized, Discovery, Initialized };

    class LIFXBulb
    {
        public byte[] bulbMac;
        public IPEndPoint bulbEndpoint;
    }
    class LIFXNetwork
    {
        public NetworkState State = NetworkState.UnInitialized;
        IPEndPoint GateWayEndPoint;
        IPAddress GateWayAddress;
        byte[] GateWayMac;

        Queue<LIFXPacket> OutPackets = new Queue<LIFXPacket>();
        Queue<LIFXPacket> InPackets = new Queue<LIFXPacket>();
        LIFXPacketFactory PacketFactory = new LIFXPacketFactory();

        byte[] readBuffer = new byte[200];
        int readBytes = 0;

        public List<LIFXBulb> bulbs = new List<LIFXBulb>();


        public void DiscoverNetwork()
        {
            State = NetworkState.Discovery;

            // prep a discovery packet
            LIFX_GetPANGateWay discoveryPacket = (LIFX_GetPANGateWay)PacketFactory.Getpacket(0x02);
            discoveryPacket.protocol = 21504;

            // Set up UDP connection.
            Socket sending_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPAddress send_to_address = IPAddress.Parse("192.168.1.255");
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
            bulb.bulbEndpoint = new IPEndPoint(RemoteIpEndPoint.Address, 56700);
            bulbs.Add(bulb);
            Thread.Sleep(100);
            while (receivingUdpClient.Available > 0)
            {
                // We've got what we need, just clean up any additional packets.
                receivebytes = receivingUdpClient.Receive(ref RemoteIpEndPoint);
                ReceivedPacket = PacketFactory.Getpacket(receivebytes);
                InPackets.Enqueue(ReceivedPacket);

                
                if (!bulbs.Any(p=>p.bulbMac.SequenceEqual(ReceivedPacket.target_mac_address)))
                {
                    bulb = new LIFXBulb();
                    bulb.bulbMac = ReceivedPacket.target_mac_address;
                    bulb.bulbEndpoint = new IPEndPoint(RemoteIpEndPoint.Address, 56700);
                    bulbs.Add(bulb); 
                }
                Thread.Sleep(100);
            }


            State = NetworkState.Initialized;

            sending_socket.Close();
            InPackets.Clear();


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

                SendPacket.target_mac_address = bulb.bulbMac;
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

            }

            while (socket.Available > 0)
            {
                readBytes = socket.Receive(readBuffer);
                InPackets.Enqueue(PacketFactory.Getpacket(readBuffer));
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

    }
}
