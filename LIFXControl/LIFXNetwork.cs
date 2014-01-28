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

        public override string ToString()
        {
            return label + "   :   " + BitConverter.ToString(bulbGateWay) + "   :   " + BitConverter.ToString(bulbMac) + "   :   " + bulbEndpoint.Address.ToString();
        }
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
            byte[] receivebytes;
            LIFXPacket ReceivedPacket;

            LIFXBulb bulb;
            Thread.Sleep(1000);
            while (receivingUdpClient.Available > 0)
            {
                receivebytes = receivingUdpClient.Receive(ref RemoteIpEndPoint);
                ReceivedPacket = PacketFactory.Getpacket(receivebytes);
                InPackets.Enqueue(ReceivedPacket);
                
                if (!bulbs.Any(p=>p.bulbMac.SequenceEqual(ReceivedPacket.target_mac_address)))
                {
                        GateWayMac = ReceivedPacket.site;
                        GateWayAddress = RemoteIpEndPoint.Address;
                        bulb = new LIFXBulb();
                        bulb.bulbMac = ReceivedPacket.target_mac_address;
                        bulb.bulbGateWay = ReceivedPacket.site;
                        bulb.bulbEndpoint = new IPEndPoint(RemoteIpEndPoint.Address, 56700);
                        bulbs.Add(bulb);
                }
                Thread.Sleep(100);
            }

            if (bulbs.Count > 0)
            {
                State = NetworkState.Initialized;
            }
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

            //foreach (LIFXBulb bulb in bulbs)
            int bulbCount = bulbs.Count;
            for (int i = 0; i < bulbCount; i++)
                {
                    LIFXBulb bulb = bulbs[i];
                    //SendPacket.target_mac_address = bulb.bulbMac;
                    if (!(bulb.bulbEndpoint == GateWayEndPoint))
                    {
                        socket.Close();
                        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        socket.Connect(bulb.bulbEndpoint);
                        GateWayEndPoint = bulb.bulbEndpoint;
                    }

                    socket.Send(PacketFactory.PacketToBuffer(SendPacket));

                    while (socket.Available > 0)
                    {
                        readBuffer = new byte[socket.Available];
                        readBytes = socket.Receive(readBuffer);
                        while (readBuffer.Length > 0)
                        {
                            InPackets.Enqueue(PacketFactory.Getpacket(readBuffer));
                            LIFXPacket basePacket = PacketFactory.Getpacket(readBuffer);
                            if (basePacket is LIFX_LightStatus)
                            {
                                AddBulb((LIFX_LightStatus)basePacket);
                            }
                            if (basePacket.size < readBuffer.Length)
                            {
                                int remainingBuffer = readBuffer.Length - basePacket.size;
                                byte[] newBuffer = new byte[remainingBuffer];
                                Array.Copy(readBuffer, basePacket.size, newBuffer, 0, remainingBuffer);
                                readBuffer = newBuffer;
                            }
                            else
                            {
                                readBuffer = new byte[0];
                            }
                        }
                    }
                }
            Thread.Sleep(100);

            while (socket.Available > 0)
            {
                LIFXBulb bulb = new LIFXBulb();
                LIFXPacket packet = null;
                readBuffer = new byte[socket.Available];
                readBytes = socket.Receive(readBuffer);
                while (readBuffer.Length > 0)
                {
                    try
                    {
                        packet = PacketFactory.Getpacket(readBuffer);

                        if (!bulbs.Any(p => p.bulbMac.SequenceEqual(packet.target_mac_address)))
                        {
                            AddBulb((LIFX_LightStatus)packet);
                        }
                        else
                        {
                            int bulbMatch = bulbs.FindIndex(p => p.bulbMac.SequenceEqual(packet.target_mac_address));
                            UpdateBulb((LIFX_LightStatus)packet, bulbs[bulbMatch]);
                        }
                        InPackets.Enqueue(PacketFactory.Getpacket(readBuffer));

                    }
                    catch (Exception e)
                    { }
                    if (packet.size < readBuffer.Length)
                    {
                        int remainingBuffer = readBuffer.Length - packet.size;
                        byte[] newBuffer = new byte[remainingBuffer];
                        Array.Copy(readBuffer, packet.size, newBuffer, 0, remainingBuffer);
                        readBuffer = newBuffer;
                    }
                    else
                    {
                        readBuffer = new byte[0];
                    }
                }
            }
            socket.Close();
        }

        public void SetAllBulbValues(UInt16 hue, UInt16 saturation, UInt16 brightness, UInt16 kelvin, UInt32 fade, UInt16 delay)
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
                Thread.Sleep(100);
            }
        }
        public void SetBulbValues(UInt16 hue, UInt16 saturation, UInt16 brightness, UInt16 kelvin, UInt32 fade, LIFXBulb bulb)
        {
            LIFX_SetLightColor setpacket = (LIFX_SetLightColor)PacketFactory.Getpacket(0x66);
            setpacket.hue = hue;
            setpacket.saturation = saturation;
            setpacket.brightness = brightness;
            setpacket.kelvin = kelvin;
            setpacket.fadeTime = ((fade) * 225) ^ 2;
            setpacket.site = GateWayMac;
            setpacket.size = 49;
            setpacket.target_mac_address = bulb.bulbMac;
            SendPacket(bulb, setpacket);
        }

        public void AddBulb(LIFX_LightStatus stat)
        {
            LIFXBulb bulb = new LIFXBulb();
            bulb.bulbGateWay = stat.site;
            bulb.bulbMac = stat.target_mac_address;
            bulb.bulbEndpoint = GateWayEndPoint;
            bulb.label = stat.bulb_label.TrimEnd(charsToTrim);
            bulb.hue = stat.hue;
            bulb.saturation = stat.saturation;
            bulb.kelvin = stat.kelvin;
            bulb.brightness = stat.brightness;
            bulb.power = stat.power;
            bulb.dim = stat.dim;
            bulb.tags = stat.tags;
            bulbs.Add(bulb);
        }

        public void UpdateBulb(LIFX_LightStatus packet, LIFXBulb bulb)
        {
            bulb.bulbMac = packet.target_mac_address;
            bulb.label = packet.bulb_label.TrimEnd(charsToTrim);
            bulb.hue = packet.hue;
            bulb.saturation = packet.saturation;
            bulb.kelvin = packet.kelvin;
            bulb.brightness = packet.brightness;
            bulb.power = packet.power;
            bulb.dim = packet.dim;
            bulb.tags = packet.tags;
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
