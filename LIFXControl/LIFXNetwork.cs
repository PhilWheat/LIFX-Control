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

    public class LIFXNetwork
    {
        public NetworkState State = NetworkState.UnInitialized;
        public BulbGateways tcpGateways;

        public Queue<LIFXPacket> OutPackets = new Queue<LIFXPacket>();
        public Queue<LIFXPacket> InPackets = new Queue<LIFXPacket>();
        
        public Bulbs bulbs = new Bulbs();

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
        }
        /// <summary>
        /// This overload bypasses the inventory and bulb polling.
        /// It is here for momentary events such as command line tools, the bulb collection should be de-serialized before calling this method.
        /// </summary>
        /// <param name="IPAddress"></param>
        /// <param name="Port"></param>
        /// <param name="MAC"></param>
        public void Start (String IPAddress, String Port, byte[] MAC)
        {
            LIFXBulb bulb = new LIFXBulb();
            bulb.BulbMac = MAC;
            bulb.BulbGateWay = MAC;
            IPEndPoint gatewayEndPoint = new IPEndPoint(new IPAddress(System.Text.Encoding.Default.GetBytes(IPAddress)), Convert.ToInt32(Port));
            bulb.BulbEndpoint = gatewayEndPoint;
            bulbs.Add(bulb);
            foreach (LIFXBulb bulbiterate in bulbs)
            { 
                bulb.BulbEndpoint = gatewayEndPoint;
            }
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
            foreach (LIFXBulb bulb in bulbs)
            {
                if (bulb.PacketEvents.Count > 0)
                {
                    if (bulb.PacketEvents.First().EventTime < DateTime.Now)
                    {
                        LIFXPacket packet = bulb.PacketEvents.First().EventPacket;
                        bulb.PacketEvents.Remove(bulb.PacketEvents.First());
                        packet.site = bulb.BulbGateWay;
                        packet.target_mac_address = bulb.BulbMac;
                        bulb.SendPacket(packet);
                    }
                }
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
            bulb.BatchMode = true;
            bulb.BulbGateWay = stat.site;
            bulb.BulbMac = stat.target_mac_address;
            bulb.Label = stat.bulb_label.TrimEnd(charsToTrim);
            bulb.Hue = stat.hue;
            bulb.Saturation = stat.saturation;
            bulb.Kelvin = stat.kelvin;
            bulb.Brightness = stat.brightness;
            bulb.Power = stat.power;
            bulb.Dim = (Int16)stat.dim;
            bulb.Tags = stat.tags;
            bulb.BulbSocket = gwSocket;
            bulb.LastNetworkUpdate = DateTime.Now;
            bulb.BatchMode = false;
            bulbs.Add(bulb);
        }

        /// <summary>
        /// Update an existing bulb from a LightStatus packet.  Need to genericise this some more for more packet types.
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="bulb"></param>
 
        public void UpdateBulb(LIFX_LightStatus packet, LIFXBulb bulb)
        {
            bulb.BatchMode = true;
            bulb.BulbMac = packet.target_mac_address;
            bulb.Label = packet.bulb_label.TrimEnd(charsToTrim);
            bulb.Hue = packet.hue;
            bulb.Saturation = packet.saturation;
            bulb.Kelvin = packet.kelvin;
            bulb.Brightness = packet.brightness;
            bulb.Power = packet.power;
            bulb.Dim = (Int16)packet.dim;
            bulb.Tags = packet.tags;
            bulb.LastNetworkUpdate = DateTime.Now;
            bulb.BatchMode = false;
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
                reEntrant = true;
                LIFXBulb bulb = new LIFXBulb();
                LIFXPacket packet = null;
                byte[] readBuffer;
                int readBytes;

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
                                switch (packet.packet_type)
                                { 
                                    case ((ushort) BulbToApp.LightStatus):
                                        AddBulb((LIFX_LightStatus)packet, gateway.gateWaySocket);
                                    break;
                                    default:
                                    //TODO need to do some logging
                                    string hmm = "this really shouldn't happen";
                                    break;
                                }
                            }
                            else
                            {
                                //if (packet is LIFX_LightStatus)
                                //{
                                    // Have to find out which one matches.
                                    foreach (LIFXBulb b in bulbs)
                                    {
                                        if (b.BulbMac.SequenceEqual(packet.target_mac_address))
                                        { 
                                            // Now process packet type
                                            // First set batch mode to true to not try to set the incoming value on the bulb again
                                            bulb.BatchMode = true;
                                            switch (packet.packet_type)
                                            {
                                                case ((ushort)BulbToApp.LightStatus):
                                                    UpdateBulb((LIFX_LightStatus)packet, b);
                                                    break;
                                                case ((ushort)BulbToApp.PowerState):
                                                    LIFX_PowerState ps = (LIFX_PowerState)packet;
                                                    b.Power_State = ps.OnOff;
                                                    break;
                                                case ((ushort)BulbToApp.WifiInfo):
                                                    LIFX_WifiInfo wf = (LIFX_WifiInfo)packet;
                                                    b.Signal = wf.Signal;
                                                    b.Tx = wf.Tx;
                                                    b.Rx = wf.Rx;
                                                    b.Mcu_temperature = wf.Mcu_temperature;
                                                    break;
                                                case ((ushort)BulbToApp.WifiFirmwareState):
                                                    LIFX_WifiFirmwareState wffs = (LIFX_WifiFirmwareState)packet;
                                                    b.Wifi_Firmware_BuildTime = wffs.buildTimestamp;
                                                    b.Wifi_Firmware_InstallTime = wffs.installTimestamp;
                                                    break;
                                                case ((ushort)BulbToApp.WifiState):
                                                    LIFX_WifiState wfs = (LIFX_WifiState)packet;
                                                    bulb.wifi_status = wfs.wifi_status;
                                                    bulb.interface_type = wfs.interface_type;
                                                    bulb.ip4_address = wfs.ip4_address;
                                                    bulb.ip6_address = wfs.ip6_address;
                                                    break;
                                                case ((ushort)BulbToApp.AccessPoint):
                                                    LIFX_AccessPoint acp1 = (LIFX_AccessPoint)packet;
                                                    bulb.interface_type = acp1.interface_type;
                                                    bulb.ssid = acp1.ssid;
                                                    bulb.security_protocol = acp1.security_protocol;
                                                    bulb.strength = acp1.strength;
                                                    bulb.channel = acp1.channel;
                                                    break;
                                                case ((ushort)BulbToApp.BulbLabel):
                                                    LIFX_BulbLabel bl1 = (LIFX_BulbLabel)packet;
                                                    bulb.Label = System.Text.Encoding.Default.GetString(bl1.label);
                                                    break;
                                                case ((ushort)BulbToApp.Tags):
                                                    LIFX_Tags tg1 = (LIFX_Tags)packet;
                                                    break;
                                                case ((ushort)BulbToApp.TagLabels):
                                                    LIFX_TagLabels tl1 = (LIFX_TagLabels)packet;
                                                    bulb.TagLabel = System.Text.Encoding.Default.GetString(tl1.label);
                                                    break;
                                                case ((ushort)BulbToApp.TimeState):
                                                    LIFX_TimeState ts1 = (LIFX_TimeState)packet;
                                                    bulb.time = ts1.time;
                                                    break;
                                                case ((ushort)BulbToApp.ResetSwitchState):
                                                    LIFX_ResetSwtichState rss1 = (LIFX_ResetSwtichState)packet;
                                                    bulb.resetSwitchPosition = rss1.resetSwitchPosition;
                                                    break;
                                                case ((ushort)BulbToApp.MeshInfo):
                                                    LIFX_MeshInfo mi1 = (LIFX_MeshInfo)packet;
                                                    bulb.mesh_signal = mi1.signal;
                                                    bulb.mesh_rx = mi1.rx;
                                                    bulb.mesh_tx = mi1.tx;
                                                    bulb.mesh_mcu_temperature = mi1.mcu_temperature;
                                                    break;
                                                case ((ushort)BulbToApp.MeshFirmwareState):
                                                    LIFX_MeshFirmwareState mfs1 = (LIFX_MeshFirmwareState)packet;
                                                    bulb.fwBuild = mfs1.fwBuild;
                                                    bulb.fwInstall = mfs1.fwInstall;
                                                    bulb.fwVersion = mfs1.fwVersion;
                                                    break;
                                                case ((ushort)BulbToApp.VersionState):
                                                    LIFX_VersionState vst1 = (LIFX_VersionState)packet;
                                                    bulb.bulb_vendor = vst1.bulb_vendor;
                                                    bulb.bulb_version = vst1.bulb_version;
                                                    bulb.bulb_product = vst1.bulb_product;
                                                    break;
                                                case ((ushort)BulbToApp.Info):
                                                    LIFX_Info inf1 = (LIFX_Info)packet;
                                                    bulb.time =inf1.time;
                                                    bulb.uptime = inf1.uptime;
                                                    bulb.downtime = inf1.downtime;
                                                    break;
                                                case ((ushort)BulbToApp.MCURailVoltage):
                                                    LIFX_MCURailVoltage mcr1 = (LIFX_MCURailVoltage)packet;
                                                    bulb.voltage = mcr1.voltage;
                                                    break;
                                                default:
                                                    //TODO need to do some logging
                                                    string hmm = "Unknown or Invalid packet type";
                                                    break;
                                            }
                                            // Done our updates, turn off batch mode
                                            bulb.BatchMode = false;
                                        }
                                    }
                                //}
                            }
                            InPackets.Enqueue(LIFXPacketFactory.Getpacket(readBuffer));
                        }
                        catch (Exception e)
                        { string etext = e.Message; }
                        if (packet.size <= readBuffer.Length)
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
