using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using System.Diagnostics;
using System.IO;
using System.ComponentModel;

namespace LIFX
{
    public enum NetworkState { UnInitialized, Discovery, Initialized };

    [Serializable]
    [XmlRoot("BulbGateway")]
    public class BulbGateway
    {
        public byte[] _endPoint
        {
            get
            {
                if (endPoint != null)
                    return endPoint.Address.GetAddressBytes();
                else
                    return null;
            }
            set { endPoint = new IPEndPoint(new IPAddress(value), 56700); }
        }

        [XmlIgnore]
        public IPEndPoint endPoint;
        [XmlIgnore]
        public Socket gateWaySocket;
        public byte[] gatewayMac;

        public BulbGateway()
        {
            gateWaySocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
    }

    [Serializable]
    [XmlRoot("BulbGateways")]
    public class BulbGateways : BindingList<BulbGateway>
    {
        string filename;
        public string Filename
        {
            get { return filename; }
            set { filename = value; }
        }


        public void Save()
        {
            this.SaveAs(filename);
        }
        public void SaveAs(string filename)
        {
            if (this.Count > 0)
            {
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                XmlSerializer x = new XmlSerializer(typeof(BulbGateways));
                ns.Add("", "");

                StringWriter sw = new StringWriter();
                XmlWriter writer = new XmlWriterNoDeclaration(sw);
                x.Serialize(writer, this, ns);

                StreamWriter fs = File.CreateText(filename);
                fs.Write(sw.ToString());
                fs.Close();
            }
        }
        public static BulbGateways Load(string filename)
        {
            Debug.WriteLine(DateTime.Now.ToString() + ": Entering BulbGateways.Load(string filename)");
            BulbGateways t;
            if (File.Exists(filename))
            {
                try
                {
                    XmlSerializer x = new XmlSerializer(typeof(BulbGateways));
                    using (StreamReader sr = new StreamReader(filename))
                    t = (BulbGateways)x.Deserialize(sr);
                    t.Filename = filename;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(DateTime.Now.ToString() + ": Error deserializing '" + filename + "', bailing!");
                    Debug.WriteLine(DateTime.Now.ToString() + ": " + ex.Message);
                    Debug.WriteLine(DateTime.Now.ToString() + ": " + ex.StackTrace);
                    t = new BulbGateways();
                    t.Filename = filename;
                }
            }
            else
            {
                t = new BulbGateways();
                t.Filename = filename;
            }
            Debug.WriteLine(DateTime.Now.ToString() + ": Finished SerializableBindingList.Load(string filename)");
            return t;
        }
    }

    [Serializable]
    [XmlRoot("Bulbs")]
    public class Bulbs : BindingList<LIFXBulb>
    {
        string filename;
        public string Filename
        {
            get { return filename; }
            set { filename = value; }
        }


        public void Save()
        {
            this.SaveAs(filename);
        }
        public void SaveAs(string filename)
        {
            if (this.Count > 0)
            {
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                XmlSerializer x = new XmlSerializer(typeof(Bulbs));
                ns.Add("", "");

                StringWriter sw = new StringWriter();
                XmlWriter writer = new XmlWriterNoDeclaration(sw);
                x.Serialize(writer, this, ns);

                StreamWriter fs = File.CreateText(filename);
                fs.Write(sw.ToString());
                fs.Close();
            }
        }
        public static Bulbs Load(string filename)
        {
            Debug.WriteLine(DateTime.Now.ToString() + ": Entering Bulbs.Load(string filename)");
            Bulbs t;
            if (File.Exists(filename))
            {
                try
                {
                    XmlSerializer x = new XmlSerializer(typeof(Bulbs));
                    using (StreamReader sr = new StreamReader(filename))
                    t = (Bulbs)x.Deserialize(sr);
                    t.Filename = filename;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(DateTime.Now.ToString() + ": Error deserializing '" + filename + "', bailing!");
                    Debug.WriteLine(DateTime.Now.ToString() + ": " + ex.Message);
                    Debug.WriteLine(DateTime.Now.ToString() + ": " + ex.StackTrace);
                    t = new Bulbs();
                    t.Filename = filename;
                }
            }
            else
            {
                t = new Bulbs();
                t.Filename = filename;
            }
            Debug.WriteLine(DateTime.Now.ToString() + ": Finished SerializableBindingList.Load(string filename)");
            return t;
        }
    }

    [Serializable]
    [XmlRoot("Bulb")]
    public class LIFXBulb
    {
        // Should not change for the life of the bulb object
        public byte[] BulbMac;
        public byte[] BulbGateWay;
<<<<<<< HEAD
        public IPEndPoint BulbEndpoint;
        public Socket BulbSocket;

=======
        public byte[] bulbEndpoint
        {
            get
            {
                if (BulbEndpoint != null)
                    return BulbEndpoint.Address.GetAddressBytes();
                else
                    return null;
            }
            set { BulbEndpoint = new IPEndPoint(new IPAddress(value), 56700); }
        }
        [XmlIgnore]
        public IPEndPoint BulbEndpoint;
        
        [XmlIgnore]
        public Socket BulbSocket;

        public LIFXBulb()
        {
            BulbSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

>>>>>>> Command Line App + Serialization
        // Should only be changed by the object
        public DateTime LastNetworkUpdate;
        public bool HasUpdates;
        public bool BatchMode;

        // Can be updated 
        private UInt16 _Hue;
        public UInt16 Hue
        {
            get { return _Hue; }
            set { _Hue = value; }
        }
        public UInt16 Saturation;
        public UInt16 Brightness;
        public UInt16 Kelvin;
        public UInt16 Time_Delay;
        public UInt16 Power;
        public UInt16 Dim;

        public byte Power_State;
        public string Label;
        public UInt64 Tags;

        // Used by controller to identify bulbs for bulk operations
        public bool UXSelected;

        public override string ToString()
        {
            string stringText = Label + "   :   " + BitConverter.ToString(BulbGateWay) + "   :   " + BitConverter.ToString(BulbMac) + "   :   ";
            if (BulbEndpoint != null)
            {
                stringText += BulbEndpoint.Address.ToString();
            }
            return stringText;
        }
        public void SendPacket(LIFXPacket packet)
        {
            try
            {
                BulbSocket.Send(LIFXPacketFactory.PacketToBuffer(packet));
            }
<<<<<<< HEAD
            catch (Exception e)
=======
            catch (Exception)
>>>>>>> Command Line App + Serialization
            { 
            }
        }
        public void SendBuffer(byte[] buffer)
        {
            try
            {
            BulbSocket.Send(buffer);
            }
<<<<<<< HEAD
            catch (Exception e)
=======
            catch (Exception)
>>>>>>> Command Line App + Serialization
            { 
            }
        }

    }

    public class LIFXNetwork
    {
        public NetworkState State = NetworkState.UnInitialized;
        public BulbGateways tcpGateways;

        public Queue<LIFXPacket> OutPackets = new Queue<LIFXPacket>();
        public Queue<LIFXPacket> InPackets = new Queue<LIFXPacket>();

<<<<<<< HEAD
        public List<LIFXBulb> bulbs = new List<LIFXBulb>();
=======
        public Bulbs bulbs = new Bulbs();
>>>>>>> Command Line App + Serialization
        char[] charsToTrim = { '\0'};
        private bool reEntrant = false;
        private Timer _readTimer;
        private Timer _PollTimer;

        public bool ColorCycleBulbs = false;
        public UInt16 ColorCycleStep = 400;
        // Test to see if really need this
        public UInt16 PacketDelay = 50;
        DateTime pingTimer;

        /// <summary>
        /// Set up the object.
        /// </summary>
        public LIFXNetwork()
        {
        }
<<<<<<< HEAD

        public void Start()
        {
            DiscoverNetwork();
            Inventory();
            _PollTimer = new Timer(NetworkPoll);
            _PollTimer.Change(0, 1000);
            pingTimer = DateTime.Now.AddMinutes(1);

=======
        public void Setup()
        {
            _PollTimer = new Timer(NetworkPoll);
            _PollTimer.Change(0, 1000);
            pingTimer = DateTime.Now.AddMinutes(1);
        }
        public void Start()
        {
            DiscoverNetwork();
            Inventory();
            Setup();
>>>>>>> Command Line App + Serialization
        }

        private void NetworkPoll(object state)
        {
            if (ColorCycleBulbs)
            {
                // Note, overriding fade value here - just to smooth out the color transitions.
                // Updates set for every second, so should be 1000ms for color transition.
                foreach (LIFXBulb bulb in bulbs)
                {
                    if (bulb.UXSelected)
                    {
                        bulb.Hue += ColorCycleStep;
                        bulb.Time_Delay = 1200;
                        LIFX_SetLightColor setPacket = (LIFX_SetLightColor)LIFXPacketFactory.Getpacket(0x66, bulb);
                        bulb.SendPacket(setPacket);
                    }
                }
            }
            if (DateTime.Now > pingTimer)
            {
                Inventory();
                pingTimer = DateTime.Now.AddMinutes(1);
            }
        }

        public void Stop()
        { 

        }

        public void NetworkHeartbeat(Object stateInfo)
        { }

        /// <summary>
        /// Find the Gateway bulbs on the network.
        /// </summary>
        private void DiscoverNetwork()
        {
            State = NetworkState.Discovery;

            // prep a discovery packet
            LIFX_GetPANGateWay discoveryPacket = (LIFX_GetPANGateWay)LIFXPacketFactory.Getpacket(0x02);
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
            byte[] sendData = LIFXPacketFactory.PacketToBuffer(discoveryPacket);
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
                ReceivedPacket = LIFXPacketFactory.Getpacket(receivebytes);
                // Store the packet for later analysis if desired
                InPackets.Enqueue(ReceivedPacket);
                
                // See if this packe thas a bulb we haven't seen before.  If Note, add it to the list
                // Note we don't have other bulb information yet, that will come with the inventory
                // This loop is here because there can be multiple gateway bulbs on a network.
                if (!bulbs.Any(p=>p.BulbMac.SequenceEqual(ReceivedPacket.target_mac_address)))
                {
                        //GateWayMac = ReceivedPacket.site;
                        //GateWayAddress = RemoteIpEndPoint.Address;
                        bulb = new LIFXBulb();
                        bulb.BulbMac = ReceivedPacket.target_mac_address;
                        bulb.BulbGateWay = ReceivedPacket.site;
                        bulb.BulbEndpoint = new IPEndPoint(RemoteIpEndPoint.Address, 56700);
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
            tcpGateways = new BulbGateways();

            foreach (LIFXBulb bulbEnum in bulbs)
            {
                // get the IP address for the gateway bulb
                IPEndPoint endPoint = bulbEnum.BulbEndpoint;
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(bulbs[0].BulbEndpoint);
                BulbGateway gw1 = new BulbGateway();
                gw1.gateWaySocket = socket;
                gw1.gatewayMac = bulbEnum.BulbGateWay;
                gw1.endPoint = endPoint;
                tcpGateways.Add(gw1);

                bulbEnum.BulbSocket = socket;
<<<<<<< HEAD

=======
                
>>>>>>> Command Line App + Serialization
            }
            _readTimer = new Timer(PacketPump);
            _readTimer.Change(0, 500);

        }
        /// <summary>
        /// Query the gateway bulbs for additional bulbs on their mesh networks
        /// The responses will be handled on the PacketPump receiver.
        /// </summary>
        public void Inventory()
        {
            // Now prep an inventory packet
            LIFXPacket SendPacket = LIFXPacketFactory.Getpacket(0x65);

            foreach (BulbGateway gwSocket in tcpGateways)
            {
                if (gwSocket.gateWaySocket.Connected)
                {
                    SendPacket.site = gwSocket.gatewayMac;
                    gwSocket.gateWaySocket.Send(LIFXPacketFactory.PacketToBuffer(SendPacket));
                }
                else
                {
                    Debug.Print("break here, something happened");
                }
            }
        }
        /// <summary>
        /// Set values for all bulbs on the network.  
        /// </summary>
        /// <param name="hue"></param>
        /// <param name="saturation"></param>
        /// <param name="brightness"></param>
        /// <param name="kelvin"></param>
        /// <param name="fade"></param>
        /// <param name="delay"></param>
        public void SetAllBulbValues(UInt16 hue, UInt16 saturation, UInt16 brightness, UInt16 kelvin, UInt32 fade, UInt16 delay)
        {
            LIFX_SetLightColor setpacket = (LIFX_SetLightColor)LIFXPacketFactory.Getpacket(0x66);
            setpacket.hue = hue;
            setpacket.saturation = saturation;
            setpacket.brightness = brightness;
            setpacket.kelvin = kelvin;
            setpacket.fadeTime = ((fade) * 225) ^ 2;

            setpacket.size = 49;
            foreach (LIFXBulb bulb in bulbs)
            {
                setpacket.site = bulb.BulbGateWay;
                setpacket.target_mac_address = bulb.BulbMac;
                bulb.BulbSocket.Send(LIFXPacketFactory.PacketToBuffer(setpacket));
                Thread.Sleep(50);
                LIFXPacket sendPacket = LIFXPacketFactory.Getpacket(0x65);
                sendPacket.site = bulb.BulbGateWay;
                sendPacket.target_mac_address = bulb.BulbMac;
                bulb.BulbSocket.Send(LIFXPacketFactory.PacketToBuffer(sendPacket));


            }
        }
        //  This will need to be totally reworked for Getter/Setter stuff.
        public void SetBulbValues(UInt16 hue, UInt16 saturation, UInt16 brightness, UInt16 kelvin, UInt32 fade, LIFXBulb bulb)
        {
            LIFX_SetLightColor setpacket = (LIFX_SetLightColor)LIFXPacketFactory.Getpacket(0x66);
            setpacket.hue = hue;
            setpacket.saturation = saturation;
            setpacket.brightness = brightness;
            setpacket.kelvin = kelvin;
            setpacket.fadeTime = ((fade) * 225) ^ 2;
            setpacket.site = bulb.BulbGateWay;
            setpacket.size = 49;
            setpacket.target_mac_address = bulb.BulbMac;
            bulb.BulbSocket.Send(LIFXPacketFactory.PacketToBuffer(setpacket));
            LIFXPacket sendPacket = LIFXPacketFactory.Getpacket(0x65);
            sendPacket.site = bulb.BulbGateWay;
            sendPacket.target_mac_address = bulb.BulbMac;
            bulb.BulbSocket.Send(LIFXPacketFactory.PacketToBuffer(sendPacket));
        }
        /// <summary>
        /// Add a bulb to the Network collection.
        /// </summary>
        /// <param name="stat"></param>
        /// <param name="gwSocket"></param>
        public void AddBulb(LIFX_LightStatus stat, Socket gwSocket)
        {
            LIFXBulb bulb = new LIFXBulb();
            bulb.BulbGateWay = stat.site;
            bulb.BulbMac = stat.target_mac_address;
            bulb.Label = stat.bulb_label.TrimEnd(charsToTrim);
            bulb.Hue = stat.hue;
            bulb.Saturation = stat.saturation;
            bulb.Kelvin = stat.kelvin;
            bulb.Brightness = stat.brightness;
            bulb.Power = stat.power;
            bulb.Dim = stat.dim;
            bulb.Tags = stat.tags;
            bulb.BulbSocket = gwSocket;
            bulb.LastNetworkUpdate = DateTime.Now;
            bulbs.Add(bulb);
        }

        /// <summary>
        /// Update an existing bulb from a LightStatus packet.  Need to genericise this some more for more packet types.
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="bulb"></param>
 
        public void UpdateBulb(LIFX_LightStatus packet, LIFXBulb bulb)
        {
            bulb.BulbMac = packet.target_mac_address;
            bulb.Label = packet.bulb_label.TrimEnd(charsToTrim);
            bulb.Hue = packet.hue;
            bulb.Saturation = packet.saturation;
            bulb.Kelvin = packet.kelvin;
            bulb.Brightness = packet.brightness;
            bulb.Power = packet.power;
            bulb.Dim = packet.dim;
            bulb.Tags = packet.tags;
            bulb.LastNetworkUpdate = DateTime.Now;
        }

        /// <summary>
        /// Here to find out the local subnet and broadcast on it.  
        /// Don't want to broadcast on 255.255.255.255 because some routers eat those types of broadcasts
        /// Limit our queries to the most local subnet (x.x.x.255) to be a good network citizen.
        /// </summary>
        /// <returns></returns>
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
        /// <summary>
        /// This is where we pick up all the incoming packts from the gateway(s)
        /// We use the MAC addresses to tell us which bulb the information is for.
        /// Packets are processed until the buffer is completely read - additional work should be done to handle the possiblity of a packet being split in the buffer read.
        /// </summary>
        /// <param name="stateInfo"></param>
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
                            packet = LIFXPacketFactory.Getpacket(readBuffer);

                            if (!bulbs.Any(p => p.BulbMac.SequenceEqual(packet.target_mac_address)))
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
<<<<<<< HEAD
                                    int bulbMatch = bulbs.FindIndex(p => p.BulbMac.SequenceEqual(packet.target_mac_address));
                                    UpdateBulb((LIFX_LightStatus)packet, bulbs[bulbMatch]);
=======
                                    foreach (LIFXBulb b in bulbs)
                                    {
                                        if (b.BulbMac.SequenceEqual(packet.target_mac_address))
                                            UpdateBulb((LIFX_LightStatus)packet, b);
                                    }
>>>>>>> Command Line App + Serialization
                                }
                            }
                            InPackets.Enqueue(LIFXPacketFactory.Getpacket(readBuffer));
                        }
                        catch (Exception)
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
