using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.Net;
using System.Net.Sockets;
using System.ComponentModel;
using System.IO;
using System.Diagnostics;

namespace LIFX
{
    #region BulbGateway definitions

    
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

    #endregion

    public class BulbEvent
    { 
        public DateTime EventTime;
        public LIFXPacket EventPacket;
    }

    #region Bulb Object Definition
    public class LIFXBulb
    {
        // Should not change for the life of the bulb object
        public byte[] BulbMac;
        public byte[] BulbGateWay;

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

        // Should only be changed by the object
        public DateTime LastNetworkUpdate;
        public bool HasUpdates;

        // Can be updated - changes trigger packets
        public bool BatchMode;
        private UInt16 _hue;
        public UInt16 Hue
        {
            get { return _hue; }
            set
            {
                _hue = value;
                if (!BatchMode)
                {
                    SendPacket(AppToBulb.SetLightColor);
                }
            }
        }
        private UInt16 _saturation;

        public UInt16 Saturation
        {
            get { return _saturation; }
            set
            {
                _saturation = value;
                if (!BatchMode)
                {
                    SendPacket(AppToBulb.SetLightColor);
                }
            }
        }
        private UInt16 _brightness;

        public UInt16 Brightness
        {
            get { return _brightness; }
            set
            {
                _brightness = value;
                if (!BatchMode)
                {
                    SendPacket(AppToBulb.SetLightColor);
                }
            }
        }
        private UInt16 _kelvin;

        public UInt16 Kelvin
        {
            get { return _kelvin; }
            set
            {
                _kelvin = value;
                if (!BatchMode)
                {
                    SendPacket(AppToBulb.SetLightColor);
                }
            }
        }

        private string _label;
        public string Label
        {
            get { return _label; }
            set
            {
                _label = value;
                if (!BatchMode)
                {
                    SendPacket(AppToBulb.SetBulbLabel);
                }
            }
        }
        // Encapsulation not needed yet, maybe simplify?
        private UInt16 _time_Delay;
        public UInt16 Time_Delay
        {
            get { return _time_Delay; }
            // Time_Delay is a modifier, don't send a packet to change just this value
            set { _time_Delay = value; }
        }

        private ONOFF _power_State;
        public ONOFF Power_State
        {
            get { return _power_State; }
            set
            {
                _power_State = value;
                if (!BatchMode)
                {
                    SendPacket(AppToBulb.SetPowerState);
                }
            }
        }

        public UInt16 Power;
        public Int16 Dim;
        public UInt64 Tags;
        public string TagLabel;
        public List<BulbEvent> PacketEvents = new List<BulbEvent>();

        // General information from the protocol.  Read only from bulbs.
        // Note some items do have a set packet, doesn't seem to make sense.
        // WifiInfo
        public Single Signal;
        public int Tx;
        public int Rx;
        public short Mcu_temperature;
        // WifiFirmwareInfo
        public byte[] Wifi_Firmware_BuildTime;
        public byte[] Wifi_Firmware_InstallTime;
        // WifiState - Has setter, maybe to change Interface type?  Or manually set IP's?  
        public INTERFACE interface_type; // Dupes on AccessPoint packets
        public WIFI_STATUS wifi_status;
        public byte[] ip4_address;
        public byte[] ip6_address;
        // AccessPoint
        public byte[] ssid;      // UTF-8 encoded string
        public byte[] password;  // UTF-8 encoded string
        public SECURITY_PROTOCOL security_protocol;
        public UInt16 strength;
        public UInt16 channel;
        // Light Cycle
        public UInt32 period;
        public float cycles;
        public UInt16 dutyCycles;
        public byte waveform;
        public byte transient;
        // Time
        public UInt64 time;  // Dups with Info Diagnostic
        //Diagnostics
        public RESET_SWITCH_POSITION resetSwitchPosition;
        public float mesh_signal;
        public int mesh_tx;
        public int mesh_rx;
        public short mesh_mcu_temperature;
        public LIFX_TIMESTAMP fwBuild;
        public LIFX_TIMESTAMP fwInstall;
        public UInt32 fwVersion;
        public UInt32 bulb_vendor;
        public UInt32 bulb_product;
        public UInt32 bulb_version;
        public UInt64 uptime;
        public UInt64 downtime;
        public UInt32 voltage;
        public byte testmode;

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
            catch (Exception e)
            {
                //TODO Error logging here
                string Error = "true";
            }
        }
        public void SendBuffer(byte[] buffer)
        {
            try
            {
                BulbSocket.Send(buffer);
            }
            catch (Exception)
            {
                //TODO Error logging here
                string Error = "true";
            }
        }
        public void SetLabel(string Label)
        {
            this.Label = Label;
            SendPacket(AppToBulb.SetBulbLabel);
        }
        public void SetColor(UInt16 Hue)
        {
            this.Hue = Hue;
            SendPacket(AppToBulb.SetLightColor);

        }
        public void SendPacket(AppToBulb packetType)
        {
            LIFXPacket packet = LIFXPacketFactory.Getpacket((UInt16)packetType, this);
            if (!BulbSocket.Connected)
            {
                if (BulbSocket.RemoteEndPoint!=null)
                    BulbSocket.Connect(BulbSocket.RemoteEndPoint);
            }
            BulbSocket.Send(LIFXPacketFactory.PacketToBuffer(packet));
            // TODO Review if we need to get a confirming packet for the command.
            //if ((packetType == AppToBulb.SetBulbLabel) ||
            //packet = LIFXPacketFactory.Getpacket((UInt16)AppToBulb.GetLightState);
            //BulbSocket.Send(LIFXPacketFactory.PacketToBuffer(packet));
        }
        public void DelaySetColorValues(UInt16 hue, UInt16 saturation, UInt16 brightness, UInt16 kelvin, UInt32 fade, DateTime toDo)
        {
            LIFX_SetLightColor setpacket = (LIFX_SetLightColor)LIFXPacketFactory.Getpacket(0x66);
            setpacket.hue = hue;
            setpacket.saturation = saturation;
            setpacket.brightness = brightness;
            setpacket.kelvin = kelvin;
            setpacket.fadeTime = ((fade) * 225) ^ 2;
            setpacket.size = 49;
            BulbEvent be = new BulbEvent();
            be.EventPacket = setpacket;
            be.EventTime = toDo;
            PacketEvents.Add(be);
            PacketEvents.Sort((x, y) => DateTime.Compare(x.EventTime, y.EventTime));
        }
        public void DelaySetColorValues(UInt16 hue, UInt16 saturation, UInt16 brightness, UInt16 kelvin, UInt32 fade, TimeSpan packetDelay)
        {
            LIFX_SetLightColor setpacket = (LIFX_SetLightColor)LIFXPacketFactory.Getpacket(0x66);
            setpacket.hue = hue;
            setpacket.saturation = saturation;
            setpacket.brightness = brightness;
            setpacket.kelvin = kelvin;
            setpacket.fadeTime = ((fade) * 225) ^ 2;
            setpacket.size = 49;
            BulbEvent be = new BulbEvent();
            be.EventPacket = setpacket;
            be.EventTime = DateTime.Now + packetDelay;
            PacketEvents.Add(be);
            PacketEvents.Sort((x, y) => DateTime.Compare(x.EventTime, y.EventTime));
        }
    }
    #endregion
}
