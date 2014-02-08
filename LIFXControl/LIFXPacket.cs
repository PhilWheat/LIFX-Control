using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LIFX
{
#region Enums
    public enum AppToBulb : ushort
    { 
        // Network
        GetPanGateway = 0x02,
        //Power
        GetPowerState = 0x14,
        SetPowerState = 0x15,
        //Wireless
        GetWifiInfo = 0x10,
        GetWifiFirmwareState = 0x12,
        GetWifiState = 0x12d,
        SetWifiState = 0x12e,
        GetAccessPoints = 0x130,
        SetAccessPoint = 0x131,
        //Labels and Tags
        GetBulbLabel = 0x17,
        SetBulbLabel = 0x18,
        GetTags = 0x1a,
        SetTags = 0x1b,
        GetTagLabels = 0x1d,
        SetTaglabels = 0x1e,
        //Brightness and Colors
        GetLightState = 0x65,
        SetLightColor = 0x66,
        SetWaveform = 0x67,
        SetDimAbs = 0x68,
        SetDimRel = 0x69,
        //Time
        GetTime = 0x04,
        SetTime = 0x05,
        //Diagnostic
        GetResetSwitch = 0x07,
        GetDummyLoad = 0x09,
        SetDummyLoad = 0x0a,
        GetMeshInfo = 0x0c,
        GetMeshFirmware = 0x0e,
        GetVersion = 0x20,
        GetInfo = 0x22,
        GetMCURailVoltage = 0x24,
        Reboot = 0x26,
        SetFactoryTestMode = 0x27,
        DisableFactoryTestMode = 0x28
    }
    public enum BulbToApp : ushort
    {
        //Network
        PanGateway = 0x03,
        //Power
        PowerState = 0x16,
        //Wireless
        WifiInfo = 0x11,
        WifiFirmwareState = 0x13,
        WifiState = 0x12f,
        AccessPoint = 0x132,
        //Labels and Tags
        BulbLabel = 0x19,
        Tags = 0x1c,
        TagLabels = 0x1f,
        //Brightness and Colors
        LightStatus = 0x6b,
        //Time
        TimeState = 0x06,
        //Diagnostic
        ResetSwitchState = 0x08,
        DummyLoad = 0x0b,
        MeshInfo = 0x0d,
        MeshFirmwareState = 0x0e,
        VersionState = 0x21,
        Info = 0x23,
        MCURailVoltage = 0x25
    }
    public enum INTERFACE : byte
    {
        SOFT_AP = 1, // i.e. act as an access point
        STATION = 2  // i.e. connect to an existing access point
    }
    public enum WIFI_STATUS : byte
    {
        CONNECTING = 0,
        CONNECTED = 1,
        FAILED = 2,
        OFF = 3
    }
    public enum SECURITY_PROTOCOL : byte
    {
       OPEN           = 1,
       WEP_PSK        = 2, // Not officially supported
       WPA_TKIP_PSK   = 3,
       WPA_AES_PSK    = 4,
       WPA2_AES_PSK   = 5,
       WPA2_TKIP_PSK  = 6,
       WPA2_MIXED_PSK = 7
    }
    public enum SERVICE : byte
    {
      UDP = 1,
      TCP = 2
    }
    public enum RESET_SWITCH_POSITION : byte
    {
      UP = 0,
      DOWN = 1
    }
    public enum ONOFF : byte
    {
      OFF = 0,
      ON  = 1
    }
    public struct LIFX_TIMESTAMP
    {
        public byte second;
        public byte minute;
        public byte hour;
        public byte day;
        public byte[] month;
        public byte year;
    }
#endregion
    public static class LIFXPacketFactory
    {
        public static LIFXPacket Getpacket()
        {
            LIFXPacket packet = new LIFXPacket();
            return packet;
        }
        public static LIFXPacket Getpacket (UInt16 packetType)
        {
            LIFXPacket packet = null; ;
            switch (packetType)
            { 
                //TODO update to Enum for readability.
                case 0x02:
                    packet = new LIFX_GetPANGateWay();
                break;
                case 0x03:
                    packet = new LIFX_PANGateWay();
                break;
                case 0x14:
                    packet = new LIFX_GetPowerState();
                break;
                case 0x15:
                    packet = new LIFX_SetPowerState();
                break;
                case 0x16:
                    packet = new LIFX_PowerState();
                break;
                case 0x10:
                    packet = new LIFX_GetWifiInfo();
                break;
                case 0x11:
                    packet = new LIFX_WifiInfo();
                break;
                case 0x12:
                    packet = new LIFX_GetWifiFirmwareState();
                break;
                case 0x13:
                    packet = new LIFX_WifiFirmwareState();
                break;
                case 0x12d:
                    packet = new LIFX_GetWifiState();
                break;
                case 0x12e:
                    packet = new LIFX_SetWifiState();
                break;
                case 0x12f:
                    packet = new LIFX_WifiState();
                break;
                case 0x130:
                    packet = new LIFX_GetAccessPoints();
                break;
                case 0x131:
                    packet = new LIFX_SetAccessPoint();
                break;
                case 0x132:
                    packet = new LIFX_AccessPoint();
                break;
                case 0x17:
                    packet = new LIFX_GetBulbLabel();
                break;
                case 0x18:
                    packet = new LIFX_SetBulbLabel();
                    packet.size = 36 + 32;  // packet + payload
                break;
                case 0x19:
                    packet = new LIFX_BulbLabel();
                break;
                case 0x1a:
                    packet = new LIFX_GetTags();
                break;
                case 0x1b:
                    packet = new LIFX_SetTags();
                break;
                case 0x1c:
                    packet = new LIFX_Tags();
                break;
                case 0x1d:
                    packet = new LIFX_GetTagLabels();
                break;
                case 0x1e:
                    packet = new LIFX_SetTagLabels();
                break;
                case 0x1f:
                    packet = new LIFX_TagLabels();
                break;
                case 0x65:
                    packet = new LIFX_GetLightState();
                break;
                case (UInt16)AppToBulb.SetLightColor:
                    packet = new LIFX_SetLightColor();
                break;
                case 0x67:
                    packet = new LIFX_SetWaveForm();
                break;
                case 0x68:
                    packet = new LIFX_SetDimAbsolute();
                break;
                case 0x69:
                    packet = new LIFX_SetDimRelative();
                break;
                case 0x6b:
                    packet = new LIFX_LightStatus();
                break;
                case 0x04:
                    packet = new LIFX_GetTime();
                break;
                case 0x05:
                    packet = new LIFX_SetTime();
                break;
                case 0x06:
                    packet = new LIFX_TimeState();
                break;
                case 0x07:
                    packet = new LIFX_GetResetSwitch();
                break;
                case 0x08:
                    packet = new LIFX_ResetSwtichState();
                break;
                case 0x09:
                    packet = new LIFX_GetDummyLoad();
                break;
                case 0x0a:
                    packet = new LIFX_SetDummyLoad();
                break;
                case 0x0b:
                    packet = new LIFX_DummyLoad();
                break;
                case 0x0c:
                    packet = new LIFX_GetMeshInfo();
                break;
                case 0x0d:
                    packet = new LIFX_MeshInfo();
                break;
                case 0x0e:
                    packet = new LIFX_GetMeshFirmware();
                break;
                case 0x0f:
                    packet = new LIFX_MeshFirmwareState();
                break;
                case 0x20:
                    packet = new LIFX_GetVersion();
                break;
                case 0x21:
                    packet = new LIFX_VersionState();
                break;
                case 0x22:
                    packet = new LIFX_GetInfo();
                break;
                case 0x23:
                    packet = new LIFX_Info();
                break;
                case 0x24:
                    packet = new LIFX_GetMCURailVoltage();
                break;
                case 0x25:
                    packet = new LIFX_MCURailVoltage();
                break;
                case 0x26:
                    packet = new LIFX_Reboot();
                break;
                case 0x27:
                    packet = new LIFX_SetFactoryTestMode();
                break;
                case 0x28:
                    packet = new LIFX_DisableFactoryTestMode();
                break;
                default:
                    return null;
            }
            packet.packet_type = packetType;
            if (packet.protocol == 0)
            { packet.protocol = 13312;}
            if (packet.size == 0)
            { packet.size = 36; }
            return packet;
        }

        public static LIFXPacket Getpacket(UInt16 packetType, LIFXBulb bulb)
        {
            LIFXPacket packet;
            switch (packetType)
            {
                case (UInt16)AppToBulb.SetLightColor:
                    packet = new LIFX_SetLightColor(bulb);
                    break;
                case (UInt16)AppToBulb.SetBulbLabel:
                    packet = new LIFX_SetBulbLabel(bulb);
                    break;
                case (UInt16)AppToBulb.SetPowerState:
                    packet = new LIFX_SetPowerState(bulb);
                    break;
                case (UInt16)AppToBulb.SetWifiState:
                    packet = new LIFX_SetWifiState(bulb);
                    break;
                case (UInt16)AppToBulb.SetTags:
                    packet = new LIFX_SetTags(bulb);
                    break;
                case (UInt16)AppToBulb.GetTagLabels:
                    packet = new LIFX_GetTagLabels(bulb);
                    break;
                case (UInt16)AppToBulb.SetTaglabels:
                    packet = new LIFX_SetTagLabels(bulb);
                    break;
                case (UInt16)AppToBulb.SetWaveform:
                    packet = new LIFX_SetWaveForm(bulb);
                    break;
                case (UInt16)AppToBulb.SetDimAbs:
                    packet = new LIFX_SetDimAbsolute(bulb);
                    break;
                case (UInt16)AppToBulb.SetDimRel:
                    packet = new LIFX_SetDimRelative(bulb);
                    break;
                case (UInt16)AppToBulb.SetTime:
                    packet = new LIFX_SetTime(bulb);
                    break;
                case (UInt16)AppToBulb.SetFactoryTestMode:
                    packet = new LIFX_SetFactoryTestMode(bulb);
                    break;
                default:
                    packet = Getpacket(packetType);
                    break;
            }
            packet.site = bulb.BulbGateWay;
            packet.target_mac_address = bulb.BulbMac;
            packet.packet_type = packetType;
            if (packet.protocol == 0)
            { packet.protocol = 13312; }
            if (packet.size == 0)
            { packet.size = 36; }
            return packet;

        }
        public static LIFXPacket Getpacket(byte[] packetBuffer)
        {
            UInt16 packetType = BitConverter.ToUInt16(packetBuffer, 32);
            LIFXPacket newPacket = Getpacket(packetType);
            newPacket.size = BitConverter.ToUInt16(packetBuffer, 0);
            newPacket.protocol = BitConverter.ToUInt16(packetBuffer, 2);
            newPacket.reserved1 = BitConverter.ToUInt16(packetBuffer, 4);
            Array.Copy(packetBuffer, 8, newPacket.target_mac_address, 0, 6);
            newPacket.reserved2 = BitConverter.ToUInt16(packetBuffer, 14);
            Array.Copy(packetBuffer, 16, newPacket.site, 0, 6);
            newPacket.reserved3 = BitConverter.ToUInt16(packetBuffer, 22);
            newPacket.timestamp = BitConverter.ToUInt16(packetBuffer, 24);
            newPacket.packet_type = BitConverter.ToUInt16(packetBuffer, 32);
            newPacket.reserved4 = BitConverter.ToUInt16(packetBuffer, 34);

            byte[] payloadBuffer = new byte[newPacket.size - 36];
            Array.Copy (packetBuffer, 36, payloadBuffer, 0, newPacket.size - 36);
            newPacket.SetPayload(payloadBuffer);

            return newPacket;
        }
        public static byte[] PacketToBuffer(LIFXPacket encodePacket)
        {
            byte[] buffer = new byte[encodePacket.size];
            Array.Copy(BitConverter.GetBytes(encodePacket.size), 0, buffer, 0, 2);
            Array.Copy(BitConverter.GetBytes(encodePacket.protocol), 0, buffer, 2, 2);
            Array.Copy(BitConverter.GetBytes(encodePacket.reserved1), 0, buffer, 4, 4);
            Array.Copy(encodePacket.target_mac_address, 0, buffer, 8, 6);
            Array.Copy(BitConverter.GetBytes(encodePacket.reserved2), 0, buffer, 14, 2);
            Array.Copy(encodePacket.site, 0, buffer, 16, 6);
            Array.Copy(BitConverter.GetBytes(encodePacket.reserved3), 0, buffer, 22, 2);
            Array.Copy(BitConverter.GetBytes(encodePacket.timestamp), 0, buffer, 24, 8);
            Array.Copy(BitConverter.GetBytes(encodePacket.packet_type), 0, buffer, 32, 2);
            Array.Copy(BitConverter.GetBytes(encodePacket.reserved4), 0, buffer, 34, 2);
            byte[] payload = encodePacket.GetPayloadBuffer();
            Array.Copy(payload, 0, buffer, 36, payload.Length);
            return buffer;
        }
    }
    [Serializable]
    public class LIFXPacket
    {
        public LIFXPacket()
        {
            target_mac_address = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            site = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            size = 36; 
        }

        public ushort size;                // LE
        public ushort protocol;
        public uint reserved1 = 0;         // Always 0x0000
        public byte[] target_mac_address;  //[6]
        public ushort reserved2 = 0;       // Always 0x00
        public byte[] site;                // MAC address of gateway PAN controller bulb [6]
        public ushort reserved3 = 0;       // Always 0x00
        public ulong timestamp;
        public ushort packet_type;         // LE
        public ushort reserved4 = 0;           // Always 0x0000

        public virtual byte[] GetPayloadBuffer()
        { 
            byte[] emptyPayload = new byte[0];
            return emptyPayload;
        }
        public virtual void SetPayload(byte[] payloadBuffer)
        {
        }
    }

    /// <summary>
    /// Network Management Packets
    /// </summary>
    public class LIFX_GetPANGateWay :LIFXPacket
    {
        // Packet type 0x02 - app to bulb
        // No Payload
        // Expect one or more 0x03 - LIFX_PANGateWay packets in response
        public LIFX_GetPANGateWay()
        {
        }
    }
    public class LIFX_PANGateWay : LIFXPacket
    {
        // Packet type 0x03 - bulb to app
        public SERVICE service;
        public UInt32 port;
        public LIFX_PANGateWay()
        {
            service = SERVICE.TCP;
            port = 0;
            size = 36;
            protocol = 21504;
        }
        public override byte[] GetPayloadBuffer ()
        {
            byte[] payloadBuffer = new byte[5];
            Array.Copy(BitConverter.GetBytes(port), 0, payloadBuffer, 37, 4);
            return payloadBuffer;
        }
        public override void SetPayload(byte[] payloadBuffer)
        {
            if (payloadBuffer[0] == 1)
            {
                service = SERVICE.UDP;
            }
            else
            {
                service = SERVICE.TCP;
            }

            port = BitConverter.ToUInt16(payloadBuffer, 1);
        }

    }
    /// <summary>
    /// Power Management Packets
    /// </summary>
    public class LIFX_GetPowerState : LIFXPacket
    {
        // Packet type 0x14 - app to bulb
        // No Payload
        public LIFX_GetPowerState()
        {
            size = 36;
        }
    }
    public class LIFX_SetPowerState : LIFXPacket
    {
        // Packet type 0x15 - app to bulb
        // 2 Byte Payload
        public ONOFF OnOff = ONOFF.ON;
        public LIFX_SetPowerState()
        {
            OnOff = ONOFF.ON;
            size = 36 + 2;
        }
        public LIFX_SetPowerState(LIFXBulb bulb)
        {
            OnOff = bulb.Power_State;
            size = 36 + 2;
        }
        public override byte[] GetPayloadBuffer()
        {
            byte[] payloadBuffer = new byte[2];
            Array.Copy(BitConverter.GetBytes((UInt16)OnOff), 0, payloadBuffer, 0, 2);
            return payloadBuffer;
        }
        public override void SetPayload(byte[] payloadBuffer)
        {
            UInt16 value = BitConverter.ToUInt16(payloadBuffer, 0);
            if (value == 0)
            { OnOff = ONOFF.OFF;}
            else
            {OnOff = ONOFF.ON;}
        }
    }
    public class LIFX_PowerState : LIFXPacket
    {
        // Packet type 0x16 - bulb to app
        // 2 byte payload
        public ONOFF OnOff;
        public LIFX_PowerState()
        {
            OnOff = ONOFF.ON;
            size = 36 + 2;
        }
        public override byte[] GetPayloadBuffer()
        {
            byte[] payloadBuffer = new byte[2];
            Array.Copy(BitConverter.GetBytes((UInt16)OnOff), 0, payloadBuffer, 36, 2);
            return payloadBuffer;
        }
        public override void SetPayload(byte[] payloadBuffer)
        {
            UInt16 value = BitConverter.ToUInt16(payloadBuffer, 0);
            if (value == 0)
            { OnOff = ONOFF.OFF; }
            else
            { OnOff = ONOFF.ON; }
        }
    }
    /// <summary>
    /// Wireless Management
    /// </summary>
    public class LIFX_GetWifiInfo : LIFXPacket
    {
        // Packet type 0x10 - app to bulb
        // No Payload
        public LIFX_GetWifiInfo ()
        {
        }
    }
    public class LIFX_WifiInfo : LIFXPacket
    {
        // Packet type 0x11 - bulb to app
        // 14 Byte payload
        public Single Signal = 0;
        public int Tx = 0;
        public int Rx = 0;
        public short Mcu_temperature = 0;
        
        public LIFX_WifiInfo()
        {
            Signal = 0;
            Tx = 0;
            Rx = 0;
            Mcu_temperature = 0;
            size = 36 + 14;
        }
        public override byte[] GetPayloadBuffer()
        {
            byte[] payloadBuffer = new byte[14];
            Array.Copy(BitConverter.GetBytes(Signal), 0, payloadBuffer, 36, 4);
            Array.Copy(BitConverter.GetBytes(Tx), 0, payloadBuffer, 40, 4);
            Array.Copy(BitConverter.GetBytes(Rx), 0, payloadBuffer, 44, 4);
            Array.Copy(BitConverter.GetBytes(Mcu_temperature), 0, payloadBuffer, 46, 2);
            return payloadBuffer;
        }
        public override void SetPayload(byte[] payloadBuffer)
        {
            Signal = BitConverter.ToSingle(payloadBuffer, 0);
            Tx = BitConverter.ToUInt16(payloadBuffer, 4);
            Rx = BitConverter.ToUInt16(payloadBuffer, 8);
            Mcu_temperature = BitConverter.ToInt16(payloadBuffer, 12);
        }
    }
    public class LIFX_GetWifiFirmwareState : LIFXPacket
    {
        // Packet type 0x12 - app to bulb
        // No Payload
        public LIFX_GetWifiFirmwareState()
        {
            size = 36;
        }
    }
    public class LIFX_WifiFirmwareState : LIFXPacket
    {
        // Packet type 0x13 - bulb to app
        // 20 Byte Payload
        public byte[] buildTimestamp;
        public byte[] installTimestamp;
        public UInt32 version;
        public LIFX_WifiFirmwareState()
        {
            buildTimestamp = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            installTimestamp = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            version = 0;
        }
        public override byte[] GetPayloadBuffer()
        {
            byte[] payloadBuffer = new byte[20];
            Array.Copy(buildTimestamp, 0, payloadBuffer, 0, 8);
            Array.Copy(installTimestamp, 0, payloadBuffer, 8, 8);
            Array.Copy(BitConverter.GetBytes(version), 0, payloadBuffer, 40, 4);
            return payloadBuffer;
        }
        public override void SetPayload(byte[] payloadBuffer)
        {
            Array.Copy(payloadBuffer, 0, buildTimestamp, 0, 8);
            Array.Copy(payloadBuffer, 8, installTimestamp, 0, 8);
            version = BitConverter.ToUInt16(payloadBuffer, 16);
        }
    }
    public class LIFX_GetWifiState : LIFXPacket
    {
        // Packet type 0x12d - app to bulb
        // 1 byte payload
        public byte interface_type;
        public LIFX_GetWifiState()
        {
            interface_type = 0;
            size = 36 + 1;
        }
        public override byte[] GetPayloadBuffer()
        {
            byte[] payloadBuffer = new byte[1];
            payloadBuffer[0] = interface_type;
            return payloadBuffer;
        }
        public override void SetPayload(byte[] payloadBuffer)
        {
            interface_type = payloadBuffer[0];
        }
    }
    public class LIFX_SetWifiState : LIFXPacket
    {
        // Packet type 0x12e - app to bulb
        // 22 byte payload
        public INTERFACE interface_type;
        public WIFI_STATUS wifi_status;
        public byte[] ip4_address;
        public byte[] ip6_address;
        public LIFX_SetWifiState()
        {
            interface_type = 0;
            wifi_status = 0;
            ip4_address = new byte[] { 0x00, 0x00, 0x00, 0x00 };
            ip6_address = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 , 0x00, 0x00, 0x00, 0x00};
            size = 36 + 22;
        }
        public LIFX_SetWifiState(LIFXBulb bulb)
        {
            interface_type = bulb.interface_type;
            wifi_status = bulb.wifi_status;
            ip4_address = bulb.ip4_address;
            ip6_address = bulb.ip6_address;
            size = 36 + 22;
        }
        public override byte[] GetPayloadBuffer()
        {
            byte[] payloadBuffer = new byte[22];
            payloadBuffer[0] = (byte)interface_type;
            payloadBuffer[1] = (byte)wifi_status;
            Array.Copy(ip4_address, 0, payloadBuffer, 2, 4);
            Array.Copy(ip6_address, 0, payloadBuffer, 8, 8);
            return payloadBuffer;
        }
        public override void SetPayload(byte[] payloadBuffer)
        {
            byte value1 = payloadBuffer[0];
            if (value1 == 1)
            { interface_type = INTERFACE.SOFT_AP; }
            else
            { interface_type = INTERFACE.STATION; }
            value1 = payloadBuffer[1];
            switch(value1)
            {
                case 1:
                    wifi_status = WIFI_STATUS.CONNECTED;
                break;
                case 2:
                    wifi_status = WIFI_STATUS.CONNECTING;
                break;
                case 3:
                    wifi_status = WIFI_STATUS.FAILED;
                break;
                default:
                    wifi_status = WIFI_STATUS.OFF;
                break;

            }
            Array.Copy(payloadBuffer, 0, ip4_address, 2, 4);
            Array.Copy(payloadBuffer, 8, ip6_address, 6, 16);
        }
    }
    public class LIFX_WifiState : LIFXPacket
    {
        // Packet type 0x12f - bulb to app
        // 22 byte payload
        public INTERFACE interface_type;
        public WIFI_STATUS wifi_status;
        public byte[] ip4_address;
        public byte[] ip6_address;
        public LIFX_WifiState()
        {
            interface_type = 0;
            wifi_status = 0;
            ip4_address = new byte[] { 0x00, 0x00, 0x00, 0x00 };
            ip6_address = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 , 0x00, 0x00, 0x00, 0x00};
            size = 36 + 22;
        }
        public override byte[] GetPayloadBuffer()
        {
            byte[] payloadBuffer = new byte[22];
            payloadBuffer[0] = (byte)interface_type;
            payloadBuffer[1] = (byte)wifi_status;
            Array.Copy(ip4_address, 0, payloadBuffer, 2, 4);
            Array.Copy(ip6_address, 0, payloadBuffer, 6, 16);
            return payloadBuffer;
        }
        public override void SetPayload(byte[] payloadBuffer)
        {
            byte value1 = payloadBuffer[0];
            if (value1 == 1)
            { interface_type = INTERFACE.SOFT_AP; }
            else
            { interface_type = INTERFACE.STATION; }
            value1 = payloadBuffer[1];
            switch (value1)
            {
                case 1:
                    wifi_status = WIFI_STATUS.CONNECTED;
                    break;
                case 2:
                    wifi_status = WIFI_STATUS.CONNECTING;
                    break;
                case 3:
                    wifi_status = WIFI_STATUS.FAILED;
                    break;
                default:
                    wifi_status = WIFI_STATUS.OFF;
                    break;

            }
            Array.Copy(payloadBuffer, 2, ip4_address, 0, 4);
            Array.Copy(payloadBuffer, 6, ip6_address, 0, 16);
        }
    }
    public class LIFX_GetAccessPoints : LIFXPacket
    {
        // Packet type 0x130 - app to bulb
        // No Payload
        public LIFX_GetAccessPoints()
        {
            size = 36;
        }
    }
    public class LIFX_SetAccessPoint : LIFXPacket
    {
        // Packet type 0x131 - app to bulb
        // 98 byte payload 
        public INTERFACE interface_type;
        public byte[] ssid;      // UTF-8 encoded string
        public byte[] password;  // UTF-8 encoded string
        public SECURITY_PROTOCOL security_protocol; 

        public LIFX_SetAccessPoint()
        {
            interface_type = INTERFACE.STATION;
            ssid = new byte[32];
            password = new byte[64];
            security_protocol = SECURITY_PROTOCOL.OPEN;
            size = 36 + 98;
        }
        public LIFX_SetAccessPoint(LIFXBulb bulb)
        {
            interface_type = bulb.interface_type;
            ssid = bulb.ssid;
            password = bulb.password;
            security_protocol = bulb.security_protocol;
            size = 36 + 98;
        }
        public override byte[] GetPayloadBuffer()
        {
            byte[] payloadBuffer = new byte[98];
            payloadBuffer[0] = (byte)interface_type;
            Array.Copy(ssid, 0, payloadBuffer, 1, 32);
            Array.Copy(password, 0, payloadBuffer, 33, 64);
            payloadBuffer[97] = (byte)security_protocol;
            return payloadBuffer;
        }
        public override void SetPayload(byte[] payloadBuffer)
        {
            interface_type = (INTERFACE)payloadBuffer[0];
            Array.Copy(payloadBuffer, 0, ssid, 1, 32);
            Array.Copy(payloadBuffer, 8, password, 33, 64);
            security_protocol = (SECURITY_PROTOCOL) payloadBuffer[97];
        }

    }
    public class LIFX_AccessPoint : LIFXPacket
    {
        // Packet type 0x132 - bulb to app
        // 38 byte payload
        public INTERFACE interface_type; // seems to always be 0x00, bug?
        public byte[] ssid;       // UTF-8 encoded string
        public SECURITY_PROTOCOL security_protocol;
        public UInt16 strength;
        public UInt16 channel;
        public LIFX_AccessPoint()
        {
            interface_type = INTERFACE.STATION;
            ssid = new byte[32];
            security_protocol = SECURITY_PROTOCOL.OPEN;
            strength = 0;
            channel = 0;
            size = 36 + 38;
        }
        public override byte[] GetPayloadBuffer()
        {
            byte[] payloadBuffer = new byte[38];
            payloadBuffer[0] = (byte)interface_type;
            Array.Copy(ssid, 0, payloadBuffer, 1, 32);
            payloadBuffer[33] = (byte)security_protocol;
            Array.Copy(BitConverter.GetBytes(strength), 0, payloadBuffer, 34, 2);
            Array.Copy(BitConverter.GetBytes(strength), 0, payloadBuffer, 36, 2);
            return payloadBuffer;
        }
        public override void SetPayload(byte[] payloadBuffer)
        {
            interface_type = (INTERFACE)payloadBuffer[0];
            Array.Copy(payloadBuffer, 0, ssid, 1, 32);
            security_protocol = (SECURITY_PROTOCOL)payloadBuffer[33];
            strength = BitConverter.ToUInt16(payloadBuffer, 34);
            channel = BitConverter.ToUInt16(payloadBuffer, 36);
        }
    }
    /// <summary>
    /// Labels and Tags
    /// </summary>
    public class LIFX_GetBulbLabel : LIFXPacket
    {
        // Packet type 0x17 - app to bulb
        // no payload
        public LIFX_GetBulbLabel()
        {
            size = 36;
        }
    }
    public class LIFX_SetBulbLabel : LIFXPacket
    {
        // Packet type 0x18 - app to bulb
        // 32 byte payload
        public byte[] label;
        public LIFX_SetBulbLabel()
        {
            label = new byte[32];
            size = 36 + 32;
        }
        public LIFX_SetBulbLabel(LIFXBulb bulb)
        {
            label = new byte[32];
            Array.Copy(System.Text.Encoding.Default.GetBytes(bulb.Label), 0, label, 0, bulb.Label.Length);
            size = 36 + 32;
        }
        public override byte[] GetPayloadBuffer()
        {
            byte[] payloadBuffer = new byte[32];
            Array.Copy(label, 0, payloadBuffer, 0, label.Length);
            return payloadBuffer;
        }
        public override void SetPayload(byte[] payloadBuffer)
        {
            Array.Copy(payloadBuffer, 0, label, 0, 32);
        }
    }
    public class LIFX_BulbLabel : LIFXPacket
    {
        // Packet type 0x19 - bulb to app
        // 32 byte payload
        public byte[] label;
        public LIFX_BulbLabel()
        {
            label = new byte[32];
            size = 36 + 32;
        }
        public override byte[] GetPayloadBuffer()
        {
            byte[] payloadBuffer = new byte[32];
            Array.Copy(label, 0, payloadBuffer, 0, 32);
            return payloadBuffer;
        }
        public override void SetPayload(byte[] payloadBuffer)
        {
            Array.Copy(payloadBuffer, 0, label, 0, 32);
        }
    }
    public class LIFX_GetTags : LIFXPacket
    {
        // Packet type 0x1a - app to bulb
        // no payload
        public LIFX_GetTags()
        {
            size = 36;
        }
    }
    public class LIFX_SetTags : LIFXPacket
    {
        // Packet type 0x1b - app to bulb
        // 8 byte payload
        public UInt64 tags;
        public LIFX_SetTags()
        {
            tags = 0;
            size = 36 + 8;
        }
        public LIFX_SetTags(LIFXBulb bulb)
        {
            tags = bulb.Tags;
            size = 36 + 8;
        }
        public override byte[] GetPayloadBuffer()
        {
            byte[] payloadBuffer = new byte[8];
            Array.Copy(BitConverter.GetBytes(tags), 0, payloadBuffer, 0, 8);
            return payloadBuffer;
        }
        public override void SetPayload(byte[] payloadBuffer)
        {
            tags = BitConverter.ToUInt64(payloadBuffer, 0);
        }
    }
    public class LIFX_Tags : LIFXPacket
    {
        // Packet type 0x1c - bulb to app
        // 8 byte payload
        public UInt64 tags;
        public LIFX_Tags()
        {
            tags = 0;
            size = 36 + 8;
        }
        public override byte[] GetPayloadBuffer()
        {
            byte[] payloadBuffer = new byte[8];
            Array.Copy(BitConverter.GetBytes(tags), 0, payloadBuffer, 0, 8);
            return payloadBuffer;
        }
        public override void SetPayload(byte[] payloadBuffer)
        {
            tags = BitConverter.ToUInt64(payloadBuffer, 0);
        }
    }
    public class LIFX_GetTagLabels : LIFXPacket
    {
        // Packet type 0xx1d - app to bulb
        // 8 byte payload
        public UInt64 tags;
        public LIFX_GetTagLabels()
        {
            tags = 0;
            size = 36 + 8;
        }
        public LIFX_GetTagLabels(LIFXBulb bulb)
        {
            tags = bulb.Tags;
            size = 36 + 8;
        }
        public override byte[] GetPayloadBuffer()
        {
            byte[] payloadBuffer = new byte[8];
            Array.Copy(BitConverter.GetBytes(tags), 0, payloadBuffer, 0, 8);
            return payloadBuffer;
        }
        public override void SetPayload(byte[] payloadBuffer)
        {
            tags = BitConverter.ToUInt64(payloadBuffer, 0);
        }
    }
    public class LIFX_SetTagLabels : LIFXPacket
    {
        // Packet type 0x1e - app to bulb
        // 40 byte payload
        public UInt64 tags;
        byte[] label;
        public LIFX_SetTagLabels()
        {
            tags = 0;
            label = new byte[32];
            size = 36 + 40;
        }
        public LIFX_SetTagLabels(LIFXBulb bulb)
        {
            tags = bulb.Tags;
            label = System.Text.Encoding.Default.GetBytes(bulb.TagLabel);
            size = 36 + 40;
        }
        public override byte[] GetPayloadBuffer()
        {
            byte[] payloadBuffer = new byte[8];
            Array.Copy(BitConverter.GetBytes(tags), 0, payloadBuffer, 0, 8);
            Array.Copy(label, 0, payloadBuffer, 1, 32);
            return payloadBuffer;
        }
        public override void SetPayload(byte[] payloadBuffer)
        {
            tags = BitConverter.ToUInt64(payloadBuffer, 0);
            Array.Copy(payloadBuffer, 1, label, 0, 32);
        }
    }
    public class LIFX_TagLabels : LIFXPacket
    {
        // Packet type 0x1f - bulb to app
        // 40 byte payload
        public UInt64 tags;
        public byte[] label;
        public LIFX_TagLabels()
        {
            tags = 0;
            label = new byte[32];
            size = 36 + 40;
        }
        public LIFX_TagLabels(LIFXBulb bulb)
        {
            tags = bulb.Tags;
            label = System.Text.Encoding.Default.GetBytes(bulb.TagLabel);
            size = 36 + 40;
        }
        public override byte[] GetPayloadBuffer()
        {
            byte[] payloadBuffer = new byte[8];
            Array.Copy(BitConverter.GetBytes(tags), 0, payloadBuffer, 0, 8);
            Array.Copy(label, 0, payloadBuffer, 1, 32);
            return payloadBuffer;
        }
        public override void SetPayload(byte[] payloadBuffer)
        {
            tags = BitConverter.ToUInt64(payloadBuffer, 0);
            Array.Copy(payloadBuffer, 8, label, 0, 32);
        }
    }
    /// <summary>
    /// Brightness and Colors
    /// </summary>
    public class LIFX_GetLightState : LIFXPacket
    {
        // Packet type 0x65 - app to bulb
        // No payload.  Expect one or more 0x6b Light Status packets response
        public LIFX_GetLightState()
        {
            size = 36;
        }
    }
    public class LIFX_SetLightColor : LIFXPacket
    {
        // Packet type 0x66 - app to bulb
        // 13 Byte Payload
        public byte stream;
        public UInt16 hue;
        public UInt16 saturation;
        public UInt16 brightness;
        public UInt16 kelvin;
        public UInt32 fadeTime;
        public LIFX_SetLightColor()
        {
            stream = 0;
            hue = 0;
            saturation = 0;
            brightness = 0;
            kelvin = 0;
            fadeTime = 0;
            size = 36 + 13;
        }
        public LIFX_SetLightColor(LIFXBulb bulb)
        {
            size = 49;
            hue = bulb.Hue;
            saturation = bulb.Saturation;
            brightness = bulb.Brightness;
            kelvin = bulb.Kelvin;
            fadeTime = bulb.Time_Delay;
            site = bulb.BulbGateWay;
            target_mac_address = bulb.BulbMac;
        }
        public override byte[] GetPayloadBuffer()
        {
            byte[] payloadBuffer = new byte[13];
            payloadBuffer[0] = stream;
            Array.Copy(BitConverter.GetBytes(hue), 0, payloadBuffer, 1, 2);
            Array.Copy(BitConverter.GetBytes(saturation), 0, payloadBuffer, 3, 2);
            Array.Copy(BitConverter.GetBytes(brightness), 0, payloadBuffer, 5, 2);
            Array.Copy(BitConverter.GetBytes(kelvin), 0, payloadBuffer, 7, 2);
            Array.Copy(BitConverter.GetBytes(fadeTime), 0, payloadBuffer, 9, 4);
            return payloadBuffer;
        }
        public override void SetPayload(byte[] payloadBuffer)
        {
            stream = payloadBuffer[0];
            hue = BitConverter.ToUInt16(payloadBuffer, 1);
            saturation = BitConverter.ToUInt16(payloadBuffer, 3);
            brightness = BitConverter.ToUInt16(payloadBuffer, 5);
            kelvin = BitConverter.ToUInt16(payloadBuffer, 7);
            fadeTime = BitConverter.ToUInt32(payloadBuffer, 9);
        }
    }
    public class LIFX_SetWaveForm : LIFXPacket
    {
        // Packet type 0x67 - app to bulb
        // 21 byte payload
        // Same as SetLightColor
        public byte stream;
        public UInt16 hue;
        public UInt16 saturation;
        public UInt16 brightness;
        public UInt16 kelvin;
        //New fields
        public UInt32 period;
        public float cycles;
        public UInt16 dutyCycles;
        public byte waveform;
        public byte transient;

        public LIFX_SetWaveForm()
        {
            stream = 0;
            transient = 0;
            hue = 0;
            saturation = 0;
            brightness = 0;
            kelvin = 0;
            period = 0;
            cycles = 0;
            dutyCycles = 0;
            waveform = 0;
            size = 36 +21;
        }
        public LIFX_SetWaveForm(LIFXBulb bulb)
        {
            stream = 0; // Watch this for protocol updates
            transient = bulb.transient;
            hue = bulb.Hue;
            saturation = bulb.Saturation;
            brightness = bulb.Brightness;
            kelvin = bulb.Kelvin;
            period = bulb.period;
            cycles = bulb.cycles;
            dutyCycles = bulb.dutyCycles;
            waveform = bulb.waveform;
            size = 36 + 21;
        }
        public override byte[] GetPayloadBuffer()
        {
            byte[] payloadBuffer = new byte[21];
            payloadBuffer[0] = stream;
            payloadBuffer[1] = transient;
            Array.Copy(BitConverter.GetBytes(hue), 0, payloadBuffer, 2, 2);
            Array.Copy(BitConverter.GetBytes(saturation), 0, payloadBuffer, 4, 2);
            Array.Copy(BitConverter.GetBytes(brightness), 0, payloadBuffer, 6, 2);
            Array.Copy(BitConverter.GetBytes(kelvin), 0, payloadBuffer, 8, 2);
            Array.Copy(BitConverter.GetBytes(period), 0, payloadBuffer, 10, 4);
            Array.Copy(BitConverter.GetBytes(cycles), 0, payloadBuffer, 14, 4);
            Array.Copy(BitConverter.GetBytes(dutyCycles), 0, payloadBuffer, 18, 2);
            payloadBuffer[20] = waveform;
            return payloadBuffer;
        }
        public override void SetPayload(byte[] payloadBuffer)
        {
            stream = payloadBuffer[0];
            transient = payloadBuffer[1];
            hue = BitConverter.ToUInt16(payloadBuffer, 2);
            saturation = BitConverter.ToUInt16(payloadBuffer, 4);
            brightness = BitConverter.ToUInt16(payloadBuffer, 6);
            kelvin = BitConverter.ToUInt16(payloadBuffer, 8);
            period = BitConverter.ToUInt32(payloadBuffer, 10);
            cycles = BitConverter.ToSingle(payloadBuffer, 14);
            dutyCycles = BitConverter.ToUInt16(payloadBuffer, 18);
            waveform = payloadBuffer[20];
        }
    }
    public class LIFX_SetDimAbsolute : LIFXPacket
    {
        // Packet type 0x68 - app to bulb
        // 6 byte payload
        public Int16 brightness;
        public UInt32 duration;
        public LIFX_SetDimAbsolute()
        {
            brightness = 0;
            duration = 0;
            size = 36 + 6;
        }
        public LIFX_SetDimAbsolute(LIFXBulb bulb)
        {
            brightness = bulb.Dim;
            duration = bulb.Time_Delay;
            size = 36 + 6;
        }
        public override byte[] GetPayloadBuffer()
        {
            byte[] payloadBuffer = new byte[6];
            Array.Copy(BitConverter.GetBytes(brightness), 0, payloadBuffer, 0, 2);
            Array.Copy(BitConverter.GetBytes(duration), 0, payloadBuffer, 2, 4);
            return payloadBuffer;
        }
        public override void SetPayload(byte[] payloadBuffer)
        {
            brightness = BitConverter.ToInt16(payloadBuffer, 0);
            duration = BitConverter.ToUInt32(payloadBuffer, 2);
        }

    }
    public class LIFX_SetDimRelative : LIFXPacket
    {
        // Packet type 0x69 - app to bulb
        // 6 byte payload
        public Int16 brightness;
        public UInt32 duration;
        public LIFX_SetDimRelative()
        {
            brightness = 0;
            duration = 0;
            size = 36 + 6;
        }
        public LIFX_SetDimRelative(LIFXBulb bulb)
        {
            brightness = bulb.Dim;
            duration = bulb.Time_Delay;
            size = 36 + 6;
        }
        public override byte[] GetPayloadBuffer()
        {
            byte[] payloadBuffer = new byte[6];
            Array.Copy(BitConverter.GetBytes(brightness), 0, payloadBuffer, 0, 2);
            Array.Copy(BitConverter.GetBytes(duration), 0, payloadBuffer, 2, 4);
            return payloadBuffer;
        }
        public override void SetPayload(byte[] payloadBuffer)
        {
            brightness = BitConverter.ToInt16(payloadBuffer, 0);
            duration = BitConverter.ToUInt32(payloadBuffer, 2);
        }

    }
    public class LIFX_LightStatus : LIFXPacket
    {
        // Packet type 0x6b - bulb to app
        // 52 byte payload
        public UInt16 hue;
        public UInt16 saturation;
        public UInt16 brightness;
        public UInt16 kelvin;
        public UInt16 dim;
        public UInt16 power;
        public string bulb_label;
        public UInt64 tags;

        public LIFX_LightStatus()
        {
            hue = 0;
            saturation = 0;
            brightness = 0;
            kelvin = 0;
            dim = 0;
            power = 0;
            bulb_label = "";
            tags = 0;
            size = 36 + 52;
        }
        public override byte[] GetPayloadBuffer()
        {
            byte[] payloadBuffer = new byte[52];
            Array.Copy(BitConverter.GetBytes(hue), 0, payloadBuffer, 0, 2);
            Array.Copy(BitConverter.GetBytes(saturation), 0, payloadBuffer, 2, 2);
            Array.Copy(BitConverter.GetBytes(brightness), 0, payloadBuffer, 4, 2);
            Array.Copy(BitConverter.GetBytes(kelvin), 0, payloadBuffer, 6, 2);
            Array.Copy(BitConverter.GetBytes(dim), 0, payloadBuffer, 8, 2);
            Array.Copy(BitConverter.GetBytes(power), 0, payloadBuffer, 10, 2);
            Array.Copy(System.Text.Encoding.Default.GetBytes(bulb_label),0, payloadBuffer, 12, 32);
            Array.Copy(BitConverter.GetBytes(tags), 0, payloadBuffer, 44, 8);
            return payloadBuffer;
        }
        public override void SetPayload(byte[] payloadBuffer)
        {
            hue = BitConverter.ToUInt16(payloadBuffer, 0);
            saturation = BitConverter.ToUInt16(payloadBuffer, 2);
            brightness = BitConverter.ToUInt16(payloadBuffer, 4);
            kelvin = BitConverter.ToUInt16(payloadBuffer, 6);
            dim = BitConverter.ToUInt16(payloadBuffer, 8);
            power = BitConverter.ToUInt16(payloadBuffer, 10);
            bulb_label = System.Text.Encoding.Default.GetString(payloadBuffer, 12, 32);
            tags = BitConverter.ToUInt64(payloadBuffer, 44);
        }

    }
    /// <summary>
    /// Time
    /// </summary>
    public class LIFX_GetTime : LIFXPacket
    {
        // Packet type 0x04 - app to bulb
        // No Payload - expect a 0x05 - Time State packet in response
        public LIFX_GetTime()
        {
            size = 36;
        }

    }
    public class LIFX_SetTime : LIFXPacket
    {
        // Packet type 0x05 - app to bulb
        // 8 byte payload
        public UInt64 time;
        public LIFX_SetTime()
        {
            time = 0;
            size = 36 + 8;
        }
        public LIFX_SetTime(LIFXBulb bulb)
        {
            time = bulb.time;
            size = 36 + 8;
        }
        public override byte[] GetPayloadBuffer()
        {
            byte[] payloadBuffer = new byte[4];
            Array.Copy(BitConverter.GetBytes(time), 0, payloadBuffer, 0, 8);
            return payloadBuffer;
        }
        public override void SetPayload(byte[] payloadBuffer)
        {
            time = BitConverter.ToUInt64(payloadBuffer, 0);
        }
    }
    public class LIFX_TimeState : LIFXPacket
    {
        // Packet type 0x06 - app to bulb
        // 8 byte payload
        public UInt64 time;
        public LIFX_TimeState()
        {
            time = 0;
            size = 36 + 8;
        }
        public override byte[] GetPayloadBuffer()
        {
            byte[] payloadBuffer = new byte[4];
            Array.Copy(BitConverter.GetBytes(time), 0, payloadBuffer, 0, 8);
            return payloadBuffer;
        }
        public override void SetPayload(byte[] payloadBuffer)
        {
            time = BitConverter.ToUInt64(payloadBuffer, 0);
        }
    }
    /// <summary>
    /// Diagnostic
    /// </summary>
    public class LIFX_GetResetSwitch : LIFXPacket
    {
        // Packet type 0x07 - app to bulb
        // No Payload.  Expect a 0x08 Reset Switch State packet in response.
        public LIFX_GetResetSwitch()
        {
            size = 36;
        }
    }
    public class LIFX_ResetSwtichState : LIFXPacket
    {
        // Packet type 0x08 - bulb to app
        public RESET_SWITCH_POSITION resetSwitchPosition;
        public LIFX_ResetSwtichState()
        {
            //Just arbitrary - should never set here.
            resetSwitchPosition = RESET_SWITCH_POSITION.UP;
            size = 36 + 2;
        }
        public override byte[] GetPayloadBuffer()
        {
            byte[] payloadBuffer = new byte[2];
            if (resetSwitchPosition == RESET_SWITCH_POSITION.UP)
            {
                Array.Copy(BitConverter.GetBytes(0), 0, payloadBuffer, 0, 2);
            }
            else
            {
                Array.Copy(BitConverter.GetBytes(1), 0, payloadBuffer, 0, 2);
            }
            return payloadBuffer;
        }
        public override void SetPayload(byte[] payloadBuffer)
        {
            if (payloadBuffer[0] == 0)
            {
                resetSwitchPosition = RESET_SWITCH_POSITION.UP;
            }
            else
            {
                resetSwitchPosition = RESET_SWITCH_POSITION.DOWN;
            }
        }
    }
    public class LIFX_GetDummyLoad : LIFXPacket
    {
        // Packet type 0x09 - app to bulb
        public LIFX_GetDummyLoad()
        {
            throw new NotImplementedException();
        }
    }
    public class LIFX_SetDummyLoad : LIFXPacket
    {
        // Packet type 0x0a - app to bulb
        public LIFX_SetDummyLoad()
        {
            throw new NotImplementedException();
        }
    }
    public class LIFX_DummyLoad : LIFXPacket
    {
        // Packet type 0x0b - bulb to app
        public LIFX_DummyLoad()
        {
            throw new NotImplementedException();
        }
    }
    public class LIFX_GetMeshInfo : LIFXPacket
    {
        // Packet type 0x0c - app to bulb
        // No Payload - Expect 0x0d Mesh Info packet in response
        public LIFX_GetMeshInfo()
        {
            size = 36;
        }
    }
    public class LIFX_MeshInfo : LIFXPacket
    {
        // Packet type 0x0d - bulb to app
        // 14 byte payload
        public float signal;
        public int tx;
        public int rx;
        public short mcu_temperature;
        public LIFX_MeshInfo()
        {
            size = 36 + 14;
        }
        public override byte[] GetPayloadBuffer()
        {
            byte[] payloadBuffer = new byte[14];
            Array.Copy(BitConverter.GetBytes(signal), 0, payloadBuffer, 0, 4);
            Array.Copy(BitConverter.GetBytes(tx), 0, payloadBuffer, 4, 4);
            Array.Copy(BitConverter.GetBytes(rx), 0, payloadBuffer, 8, 4);
            Array.Copy(BitConverter.GetBytes(mcu_temperature), 0, payloadBuffer, 12, 2);
            return payloadBuffer;
        }
        public override void SetPayload(byte[] payloadBuffer)
        {
            signal = BitConverter.ToSingle(payloadBuffer, 0);
            tx = BitConverter.ToInt32(payloadBuffer, 4);
            rx = BitConverter.ToInt32(payloadBuffer, 8);
            mcu_temperature = BitConverter.ToInt16(payloadBuffer, 12);
        }
    }
    public class LIFX_GetMeshFirmware : LIFXPacket
    {
        // Packet type 0x0e - app to bulb
        // No Payload - Expect a 0x0f Mesh Firmware State packet in response
        public LIFX_GetMeshFirmware()
        {
            size = 36;
        }
    }
    public class LIFX_MeshFirmwareState : LIFXPacket
    {
        // Packet type 0x0f - bulb to app
        // 20 byte payload
        public LIFX_TIMESTAMP fwBuild;
        public LIFX_TIMESTAMP fwInstall;
        public UInt32 fwVersion;
        public LIFX_MeshFirmwareState()
        {
            fwBuild = new LIFX_TIMESTAMP();
            fwInstall = new LIFX_TIMESTAMP();
            fwVersion = 0;
        }
        public override byte[] GetPayloadBuffer()
        {
            byte[] payloadBuffer = new byte[20];
            payloadBuffer[0] = fwBuild.second;
            payloadBuffer[1] = fwBuild.minute;
            payloadBuffer[2] = fwBuild.hour;
            payloadBuffer[3] = fwBuild.day;
            payloadBuffer[4] = fwBuild.month[0];
            payloadBuffer[5] = fwBuild.month[1];
            payloadBuffer[6] = fwBuild.month[2];
            payloadBuffer[7] = fwBuild.year;
            payloadBuffer[8] = fwInstall.second;
            payloadBuffer[9] = fwInstall.minute;
            payloadBuffer[10] = fwInstall.hour;
            payloadBuffer[11] = fwInstall.day;
            payloadBuffer[12] = fwInstall.month[0];
            payloadBuffer[13] = fwInstall.month[1];
            payloadBuffer[14] = fwInstall.month[2];
            payloadBuffer[15] = fwInstall.year;
            Array.Copy(BitConverter.GetBytes(fwVersion), 0, payloadBuffer, 16, 4);
            return new byte[0];
        }
        public override void SetPayload(byte[] payloadBuffer)
        {
            fwBuild.second = payloadBuffer[0];
            fwBuild.minute = payloadBuffer[1];
            fwBuild.hour = payloadBuffer[2];
            fwBuild.day = payloadBuffer[3];
            fwBuild.month = new byte[3];
            fwBuild.month[0] = payloadBuffer[4];
            fwBuild.month[1] = payloadBuffer[5];
            fwBuild.month[2] = payloadBuffer[6];
            fwBuild.year = payloadBuffer[7];
            fwInstall.second = payloadBuffer[8];
            fwInstall.minute = payloadBuffer[9];
            fwInstall.hour = payloadBuffer[10];
            fwInstall.day = payloadBuffer[11];
            fwInstall.month = new byte[3];
            fwInstall.month[0] = payloadBuffer[12];
            fwInstall.month[1] = payloadBuffer[13];
            fwInstall.month[2] = payloadBuffer[14];
            fwInstall.year = payloadBuffer[15];
            Array.Copy(BitConverter.GetBytes(fwVersion), 0, payloadBuffer, 16, 4);
        }
    }
    public class LIFX_GetVersion : LIFXPacket
    {
        // Packet type 0x20 - app to bulb
        // No Payload.  Expect 0x21 Version State packet in response.
        public LIFX_GetVersion()
        {
            size = 36;
        }
    }
    public class LIFX_VersionState : LIFXPacket
    {
        // Packet type 0x21 - app to bulb
        // 12 byte payload
        public UInt32 bulb_vendor;
        public UInt32 bulb_product;
        public UInt32 bulb_version;
        public LIFX_VersionState()
        {
            bulb_vendor = 0;
            bulb_product = 0;
            bulb_version = 0;
        }
        public override byte[] GetPayloadBuffer()
        {
            byte[] payloadBuffer = new byte[12];
            Array.Copy(BitConverter.GetBytes(bulb_vendor), 0, payloadBuffer, 0, 4);
            Array.Copy(BitConverter.GetBytes(bulb_product), 0, payloadBuffer, 4, 4);
            Array.Copy(BitConverter.GetBytes(bulb_version), 0, payloadBuffer, 8, 4);
            return payloadBuffer;
        }
        public override void SetPayload(byte[] payloadBuffer)
        {
            bulb_vendor = BitConverter.ToUInt32(payloadBuffer, 0);
            bulb_product = BitConverter.ToUInt32(payloadBuffer, 4);
            bulb_version = BitConverter.ToUInt32(payloadBuffer, 8);
        }
    }
    public class LIFX_GetInfo : LIFXPacket
    {
        // Packet type 0x22 - app to bulb
        // No Payload.  Expect packet 0x23 Info packet in response
        public LIFX_GetInfo()
        {
            size = 36;
        }
    }
    public class LIFX_Info : LIFXPacket
    {
        // Packet type 0x23 - bulb to app
        // 24 Byte payload
        public UInt64 time;
        public UInt64 uptime;
        public UInt64 downtime;
        public LIFX_Info()
        {
            time = 0;
            uptime = 0;
            downtime = 0;
            size = 36 + 24;
        }
        public override byte[] GetPayloadBuffer()
        {
            byte[] payloadBuffer = new byte[24];
            Array.Copy(BitConverter.GetBytes(time), 0, payloadBuffer, 0, 8);
            Array.Copy(BitConverter.GetBytes(uptime), 0, payloadBuffer, 4, 8);
            Array.Copy(BitConverter.GetBytes(downtime), 0, payloadBuffer, 8, 8);
            return payloadBuffer;
        }
        public override void SetPayload(byte[] payloadBuffer)
        {
            time = BitConverter.ToUInt64(payloadBuffer, 0);
            uptime = BitConverter.ToUInt64(payloadBuffer, 8);
            downtime = BitConverter.ToUInt64(payloadBuffer, 16);
        }
    }
    public class LIFX_GetMCURailVoltage : LIFXPacket
    {
        // Packet type 0x24 - app to bulb
        // No Payload.  Expect Packet 0x25 MCU Rail Voltage in response.
        public LIFX_GetMCURailVoltage()
        {
            size = 36;
        }
    }
    public class LIFX_MCURailVoltage : LIFXPacket
    {
        // Packet type 0x25 - bulb to app
        // 4 byte payload
        public UInt32 voltage;
        public LIFX_MCURailVoltage()
        {
            voltage = 0;
            size = 36 +4;
        }
        public override byte[] GetPayloadBuffer()
        {
            byte[] payloadBuffer = new byte[12];
            Array.Copy(BitConverter.GetBytes(voltage), 0, payloadBuffer, 0, 4);
            return payloadBuffer;
        }
        public override void SetPayload(byte[] payloadBuffer)
        {
            voltage = BitConverter.ToUInt32(payloadBuffer, 0);
        }
    }
    public class LIFX_Reboot : LIFXPacket
    {
        // Packet type 0x26 - app to bulb
        // No payload.  No packet response expected
        public LIFX_Reboot()
        {
            size = 36;
        }
    }
    public class LIFX_SetFactoryTestMode : LIFXPacket
    {
        // Packet type 0x27 - app to bulb
        // 1 byte payload, unknown contents.
        public byte unknown;
        public LIFX_SetFactoryTestMode()
        {
            unknown = 1;
            size = 36 +1;
        }
        public LIFX_SetFactoryTestMode(LIFXBulb bulb)
        {
            unknown = bulb.testmode;
            size = 36 + 1;
        }
        public override byte[] GetPayloadBuffer()
        {
            byte[] payloadBuffer = new byte[1];
            payloadBuffer[0] = 1;
            return payloadBuffer;
        }
        public override void SetPayload(byte[] payloadBuffer)
        {
            unknown = payloadBuffer[0];
        }
    }
    public class LIFX_DisableFactoryTestMode : LIFXPacket
    {
        // Packet type 0x28 - app to bulb
        // No payload.  No packet response expected
        public LIFX_DisableFactoryTestMode()
        {
            size = 36;
        }
    }
}
