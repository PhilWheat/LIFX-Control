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

    public class BulbGateway
    {
        public IPEndPoint endPoint;
        public Socket gateWaySocket;
        public byte[] gatewayMac;
    }

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
        public Socket bulbSocket;
        public DateTime lastNetworkUpdate;

        public override string ToString()
        {
            string stringText = label + "   :   " + BitConverter.ToString(bulbGateWay) + "   :   " + BitConverter.ToString(bulbMac) + "   :   ";
            if (bulbEndpoint != null)
            {
                stringText += bulbEndpoint.Address.ToString();
            }
            return stringText;
        }
    }

    public class LIFXNetwork
    {
        public NetworkState State = NetworkState.UnInitialized;
        private List<BulbGateway> tcpGateways;

        public Queue<LIFXPacket> OutPackets = new Queue<LIFXPacket>();
        public Queue<LIFXPacket> InPackets = new Queue<LIFXPacket>();
        LIFXPacketFactory PacketFactory = new LIFXPacketFactory();

        byte[] readBuffer = new byte[200];
        int readBytes = 0;

        public List<LIFXBulb> bulbs = new List<LIFXBulb>();
        char[] charsToTrim = { '\0'};
        private bool reEntrant = false;
        private Timer _readPump;

        /// <summary>
        /// Find the Gateway bulbs on the network.
        /// </summary>
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

            // Send the announce UDP Packet
            byte [] sendData = PacketFactory.PacketToBuffer(discoveryPacket);
            sending_socket.SendTo(sendData, sending_end_point);

            // Now set up the Listen port
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            UdpClient receivingUdpClient = new UdpClient(56700);

            // Prep the loop variables
            byte[] receivebytes;
            LIFXPacket ReceivedPacket;
            LIFXBulb bulb;

            // Pause for a second to allow for slow bulb responses
            Thread.Sleep(1000);

            // Now loop through received packets
            while (receivingUdpClient.Available > 0)
            {
                // Get the outstanding bytes
                receivebytes = receivingUdpClient.Receive(ref RemoteIpEndPoint);
                // Convert them to a raw packet
                ReceivedPacket = PacketFactory.Getpacket(receivebytes);
                // Store the packet for later analysis if desired
                InPackets.Enqueue(ReceivedPacket);
                
                // See if this packe thas a bulb we haven't seen before.  If Note, add it to the list
                // Note we don't have other bulb information yet, that will come with the inventory
                // This loop is here because there can be multiple gateway bulbs on a network.
                if (!bulbs.Any(p=>p.bulbMac.SequenceEqual(ReceivedPacket.target_mac_address)))
                {
                        //GateWayMac = ReceivedPacket.site;
                        //GateWayAddress = RemoteIpEndPoint.Address;
                        bulb = new LIFXBulb();
                        bulb.bulbMac = ReceivedPacket.target_mac_address;
                        bulb.bulbGateWay = ReceivedPacket.site;
                        bulb.bulbEndpoint = new IPEndPoint(RemoteIpEndPoint.Address, 56700);
                        bulbs.Add(bulb);
                }
                // Timing for the read, this really shouldn't be here.
                Thread.Sleep(100);
            }

            // Set the network state to Init if we have at least one gateway bulb
            // If not, no commands make sense
            if (bulbs.Count > 0)
            {
                State = NetworkState.Initialized;
            }
            // Clean up the ports.
            sending_socket.Close();
            receivingUdpClient.Close();

            //Now extract and populate the Gateways
            tcpGateways = new List<BulbGateway>();

            foreach (LIFXBulb bulbEnum in bulbs)
            {
                // get the IP address for the gateway bulb
                IPEndPoint endPoint = bulbEnum.bulbEndpoint;
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(bulbs[0].bulbEndpoint);
                BulbGateway gw1 = new BulbGateway();
                gw1.gateWaySocket = socket;
                gw1.gatewayMac = bulbEnum.bulbGateWay;
                gw1.endPoint = endPoint;
                tcpGateways.Add(gw1);

                bulbEnum.bulbSocket = socket;

            }
            _readPump = new Timer(PacketPump);
            _readPump.Change(0, 500);

        }
        /// <summary>
        /// Query the gateway bulbs for additional bulbs on their mesh networks
        /// </summary>
        public void Inventory()
        {
            // Now prep an inventory packet
            LIFXPacket SendPacket = PacketFactory.Getpacket(0x65);

            foreach (BulbGateway gwSocket in tcpGateways)
            {
                if (gwSocket.gateWaySocket.Connected)
                {
                    SendPacket.site = gwSocket.gatewayMac;
                    gwSocket.gateWaySocket.Send(PacketFactory.PacketToBuffer(SendPacket));
                }
                else
                {
                    string breakpt = "break here, something happened";
                }
            }
        }

        public void SetAllBulbValues(UInt16 hue, UInt16 saturation, UInt16 brightness, UInt16 kelvin, UInt32 fade, UInt16 delay)
        {
            LIFX_SetLightColor setpacket = (LIFX_SetLightColor)PacketFactory.Getpacket(0x66);
            setpacket.hue = hue;
            setpacket.saturation = saturation;
            setpacket.brightness = brightness;
            setpacket.kelvin = kelvin;
            setpacket.fadeTime = ((fade) * 225) ^ 2;

            setpacket.size = 49;
            foreach (LIFXBulb bulb in bulbs)
            {
                setpacket.site = bulb.bulbGateWay;
                setpacket.target_mac_address = bulb.bulbMac;
                bulb.bulbSocket.Send(PacketFactory.PacketToBuffer(setpacket));
                Thread.Sleep(50);
                LIFXPacket sendPacket = PacketFactory.Getpacket(0x65);
                sendPacket.site = bulb.bulbGateWay;
                sendPacket.target_mac_address = bulb.bulbMac;
                bulb.bulbSocket.Send(PacketFactory.PacketToBuffer(sendPacket));


            }
        }
        //  This will need to be totall reworked for Getter/Setter stuff.
        public void SetBulbValues(UInt16 hue, UInt16 saturation, UInt16 brightness, UInt16 kelvin, UInt32 fade, LIFXBulb bulb)
        {
            LIFX_SetLightColor setpacket = (LIFX_SetLightColor)PacketFactory.Getpacket(0x66);
            setpacket.hue = hue;
            setpacket.saturation = saturation;
            setpacket.brightness = brightness;
            setpacket.kelvin = kelvin;
            setpacket.fadeTime = ((fade) * 225) ^ 2;
            setpacket.site = bulb.bulbGateWay;
            setpacket.size = 49;
            setpacket.target_mac_address = bulb.bulbMac;
            bulb.bulbSocket.Send(PacketFactory.PacketToBuffer(setpacket));
            LIFXPacket sendPacket = PacketFactory.Getpacket(0x65);
            sendPacket.site = bulb.bulbGateWay;
            sendPacket.target_mac_address = bulb.bulbMac;
            bulb.bulbSocket.Send(PacketFactory.PacketToBuffer(sendPacket));
        }

        public void AddBulb(LIFX_LightStatus stat, Socket gwSocket)
        {
            LIFXBulb bulb = new LIFXBulb();
            bulb.bulbGateWay = stat.site;
            bulb.bulbMac = stat.target_mac_address;
            //bulb.bulbEndpoint = stat;
            bulb.label = stat.bulb_label.TrimEnd(charsToTrim);
            bulb.hue = stat.hue;
            bulb.saturation = stat.saturation;
            bulb.kelvin = stat.kelvin;
            bulb.brightness = stat.brightness;
            bulb.power = stat.power;
            bulb.dim = stat.dim;
            bulb.tags = stat.tags;
            bulb.bulbSocket = gwSocket;
            bulb.lastNetworkUpdate = DateTime.Now;
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
            bulb.lastNetworkUpdate = DateTime.Now;
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

        public void PacketPump(Object stateInfo)
        {
            if (!reEntrant)
            {
                LIFXBulb bulb = new LIFXBulb();
                LIFXPacket packet = null;
                byte[] readBuffer;
                int readBytes;
                reEntrant = true;
                foreach (BulbGateway gateway in tcpGateways)
                {
                    readBuffer = new byte[gateway.gateWaySocket.Available];
                    readBytes = gateway.gateWaySocket.Receive(readBuffer);
                    while (readBuffer.Length > 0)
                    {
                        try
                        {
                            packet = PacketFactory.Getpacket(readBuffer);

                            if (!bulbs.Any(p => p.bulbMac.SequenceEqual(packet.target_mac_address)))
                            {
                                if (packet is LIFX_LightStatus)
                                {
                                    AddBulb((LIFX_LightStatus)packet, gateway.gateWaySocket);
                                }
                                else
                                { 
                                    
                                }
                            }
                            else
                            {
                                if (packet is LIFX_LightStatus)
                                {
                                    int bulbMatch = bulbs.FindIndex(p => p.bulbMac.SequenceEqual(packet.target_mac_address));
                                    UpdateBulb((LIFX_LightStatus)packet, bulbs[bulbMatch]);
                                }
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
                reEntrant = false;
            }
        }
    }
}
