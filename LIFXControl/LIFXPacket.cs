using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LIFX
{
    class LIFXPacketFactory
    {
        public LIFXPacket Getpacket()
        {
            LIFXPacket packet = new LIFXPacket();
            return packet;
        }
        public LIFXPacket Getpacket (UInt16 packetType)
        {
            LIFXPacket packet = null; ;
            switch (packetType)
            { 
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
                case 0x66:
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
                  throw new NotImplementedException();
                break;
            }
            packet.packet_type = packetType;
            if (packet.protocol == 0)
            { packet.protocol = 13312;}
            if (packet.size == 0)
            { packet.size = 36; }
            return packet;
        }

        public LIFXPacket Getpacket(byte[] packetBuffer)
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


        public byte[] PacketToBuffer(LIFXPacket encodePacket)
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
    class LIFXPacket
    {
        public LIFXPacket()
        {
            target_mac_address = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            site = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }; ;
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
    class LIFX_GetPANGateWay :LIFXPacket
    {
        // Packet type 0x02 - app to bulb
        // No Payload
        // Expect one or more 0x03 - LIFX_PANGateWay packets in response
        public LIFX_GetPANGateWay()
        {
        }
    }
    enum SERVICE : byte { UDP=1, TCP=2};
    class LIFX_PANGateWay : LIFXPacket
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
    class LIFX_GetPowerState : LIFXPacket
    {
        // Packet type 0x14 - app to bulb
        public LIFX_GetPowerState()
        {
            throw new NotImplementedException();
        }
    }
    class LIFX_SetPowerState : LIFXPacket
    {
        // Packet type 0x15 - app to bulb
        public LIFX_SetPowerState()
        {
            throw new NotImplementedException();
        }
    }
    class LIFX_PowerState : LIFXPacket
    {
        // Packet type 0x16 - bulb to app
        public LIFX_PowerState()
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// Wireless Management
    /// </summary>
    class LIFX_GetWifiInfo : LIFXPacket
    {
        // Packet type 0x10 - app to bulb
        public LIFX_GetWifiInfo ()
        {
            throw new NotImplementedException();
        }
    }
    class LIFX_WifiInfo : LIFXPacket
    {
        // Packet type 0x11 - bulb to app
        public LIFX_WifiInfo()
        {
            throw new NotImplementedException();
        }
    }
    class LIFX_GetWifiFirmwareState : LIFXPacket
    {
        // Packet type 0x12 - app to bulb
        public LIFX_GetWifiFirmwareState()
        {
            throw new NotImplementedException();
        }
    }
    class LIFX_WifiFirmwareState : LIFXPacket
    {
        // Packet type 0x13 - bulb to app
        public LIFX_WifiFirmwareState()
        {
            throw new NotImplementedException();
        }
    }
    class LIFX_GetWifiState : LIFXPacket
    {
        // Packet type 0x12d - app to bulb
        public LIFX_GetWifiState()
        {
            throw new NotImplementedException();
        }
    }
    class LIFX_WifiState : LIFXPacket
    {
        // Packet type 0x12f - bulb to app
        public LIFX_WifiState()
        {
            throw new NotImplementedException();
        }
    }
    class LIFX_SetWifiState : LIFXPacket
    {
        // Packet type 0x12e - app to bulb
        public LIFX_SetWifiState()
        {
            throw new NotImplementedException();
        }
    }
    class LIFX_GetAccessPoints : LIFXPacket
    {
        // Packet type 0x130 - app to bulb
        public LIFX_GetAccessPoints()
        {
            throw new NotImplementedException();
        }
    }
    class LIFX_SetAccessPoint : LIFXPacket
    {
        // Packet type 0x131 - app to bulb
        public LIFX_SetAccessPoint()
        {
            throw new NotImplementedException();
        }
    }
    class LIFX_AccessPoint : LIFXPacket
    {
        // Packet type 0x132 - app to bulb
        public LIFX_AccessPoint()
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// Labels and Tags
    /// </summary>
    class LIFX_GetBulbLabel : LIFXPacket
    {
        // Packet type 0x17 - app to bulb
        public LIFX_GetBulbLabel()
        {
            throw new NotImplementedException();
        }
    }
    class LIFX_SetBulbLabel : LIFXPacket
    {
        // Packet type 0x18 - app to bulb
        public LIFX_SetBulbLabel()
        {
            throw new NotImplementedException();
        }
    }
    class LIFX_BulbLabel : LIFXPacket
    {
        // Packet type 0x19 - bulb to app
        public LIFX_BulbLabel()
        {
            throw new NotImplementedException();
        }
    }
    class LIFX_GetTags : LIFXPacket
    {
        // Packet type 0x1a - app to bulb
        public LIFX_GetTags()
        {
            throw new NotImplementedException();
        }
    }
    class LIFX_SetTags : LIFXPacket
    {
        // Packet type 0x1b - app to bulb
        public LIFX_SetTags()
        {
            throw new NotImplementedException();
        }
    }
    class LIFX_Tags : LIFXPacket
    {
        // Packet type 0x1c - bulb to app
        public LIFX_Tags()
        {
            throw new NotImplementedException();
        }
    }
    class LIFX_GetTagLabels : LIFXPacket
    {
        // Packet type 0xx1d - app to bulb
        public LIFX_GetTagLabels()
        {
            throw new NotImplementedException();
        }
    }
    class LIFX_SetTagLabels : LIFXPacket
    {
        // Packet type 0x1e - app to bulb
        public LIFX_SetTagLabels()
        {
            throw new NotImplementedException();
        }
    }
    class LIFX_TagLabels : LIFXPacket
    {
        // Packet type 0x1f - bulb to app
        public LIFX_TagLabels()
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// Brightness and Colors
    /// </summary>
    class LIFX_GetLightState : LIFXPacket
    {
        // Packet type 0x65 - app to bulb
        // No payload.  Expect one or more 0x6b Light Status packets response
        public LIFX_GetLightState()
        {
        }
    }
    class LIFX_SetLightColor : LIFXPacket
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
    class LIFX_SetWaveForm : LIFXPacket
    {
        // Packet type 0x67 - app to bulb
        // 21 byte payload
        public byte stream;
        public byte transient;
        public UInt16 hue;
        public UInt16 saturation;
        public UInt16 brightness;
        public UInt16 kelvin;
        public UInt32 period;
        public float cycles;
        public UInt16 dutyCycles;
        public byte waveform;

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
    class LIFX_SetDimAbsolute : LIFXPacket
    {
        // Packet type 0x68 - app to bulb
        // 6 byte payload
        public Int16 brightness;
        public UInt32 duration;
        public LIFX_SetDimAbsolute()
        {
            brightness = 0;
            duration = 0;
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
    class LIFX_SetDimRelative : LIFXPacket
    {
        // Packet type 0x69 - app to bulb
        // 6 byte payload
        public Int16 brightness;
        public UInt32 duration;
        public LIFX_SetDimRelative()
        {
            brightness = 0;
            duration = 0;
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
    class LIFX_LightStatus : LIFXPacket
    {
        // Packet type 0x6b - bulb to app
        // 52 byte payload
        public ushort hue;
        public ushort saturation;
        public ushort brightness;
        public ushort kelvin;
        public ushort dim;
        public ushort power;
        public string bulb_label;
        public ulong tags;

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
    class LIFX_GetTime : LIFXPacket
    {
        // Packet type 0x04 - app to bulb
        // No Payload - expect a 0x05 - Time State packet in response
        public LIFX_GetTime()
        {
        }

    }
    class LIFX_SetTime : LIFXPacket
    {
        // Packet type 0x05 - app to bulb
        public UInt64 time;
        public LIFX_SetTime()
        {
            time = 0;
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
    class LIFX_TimeState : LIFXPacket
    {
        // Packet type 0x06 - app to bulb
        public UInt64 time;
        public LIFX_TimeState()
        {
            time = 0;
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
    class LIFX_GetResetSwitch : LIFXPacket
    {
        // Packet type 0x07 - app to bulb
        // No Payload.  Expect a 0x08 Reset Switch State packet in response.
        public LIFX_GetResetSwitch()
        {
        }
    }
    enum RESET_SWITCH_POSITION : byte {Up=0, Down=1};
    class LIFX_ResetSwtichState : LIFXPacket
    {
        // Packet type 0x08 - bulb to app
        public RESET_SWITCH_POSITION resetSwitchPosition;
        public LIFX_ResetSwtichState()
        {
            //Just arbitrary - should never set here.
            resetSwitchPosition = RESET_SWITCH_POSITION.Up;
        }
        public override byte[] GetPayloadBuffer()
        {
            byte[] payloadBuffer = new byte[2];
            if (resetSwitchPosition == RESET_SWITCH_POSITION.Up)
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
                resetSwitchPosition = RESET_SWITCH_POSITION.Up;
            }
            else
            {
                resetSwitchPosition = RESET_SWITCH_POSITION.Down;
            }
        }
    }
    class LIFX_GetDummyLoad : LIFXPacket
    {
        // Packet type 0x09 - app to bulb
        public LIFX_GetDummyLoad()
        {
            throw new NotImplementedException();
        }
    }
    class LIFX_SetDummyLoad : LIFXPacket
    {
        // Packet type 0x0a - app to bulb
        public LIFX_SetDummyLoad()
        {
            throw new NotImplementedException();
        }
    }
    class LIFX_DummyLoad : LIFXPacket
    {
        // Packet type 0x0b - bulb to app
        public LIFX_DummyLoad()
        {
            throw new NotImplementedException();
        }
    }
    class LIFX_GetMeshInfo : LIFXPacket
    {
        // Packet type 0x0c - app to bulb
        // No Payload - Expect 0x0d Mesh Info packet in response
        public LIFX_GetMeshInfo()
        {
        }
    }
    class LIFX_MeshInfo : LIFXPacket
    {
        // Packet type 0x0d - bulb to app
        // 14 byte payload
        public float signal;
        public int tx;
        public int rx;
        public short mcu_temperature;
        public LIFX_MeshInfo()
        {
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
    class LIFX_GetMeshFirmware : LIFXPacket
    {
        // Packet type 0x0e - app to bulb
        // No Payload - Expect a 0x0f Mesh Firmware State packet in response
        public LIFX_GetMeshFirmware()
        {
        }
    }
    public struct LIFX_TIMESTAMP
    {
      byte second;
      byte minute;
      byte hour;
      byte day;
      char[] month;
      byte year;
    }
    class LIFX_MeshFirmwareState : LIFXPacket
    {
        // Packet type 0x0f - bulb to app
        // 20 byte payload
        public LIFX_TIMESTAMP fwBuild;
        public LIFX_TIMESTAMP fwInstall;
        public UInt32 fwVersion;
        public LIFX_MeshFirmwareState()
        {
            throw new NotImplementedException();
        }
        public override byte[] GetPayloadBuffer()
        {
            return new byte[0];
        }
        public override void SetPayload(byte[] payloadBuffer)
        {
        }
    }
    class LIFX_GetVersion : LIFXPacket
    {
        // Packet type 0x20 - app to bulb
        // No Payload.  Expect 0x21 Version State packet in response.
        public LIFX_GetVersion()
        {
        }
    }
    class LIFX_VersionState : LIFXPacket
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
    class LIFX_GetInfo : LIFXPacket
    {
        // Packet type 0x22 - app to bulb
        // No Payload.  Expect packet 0x23 Info packet in response
        public LIFX_GetInfo()
        {
        }
    }
    class LIFX_Info : LIFXPacket
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
    class LIFX_GetMCURailVoltage : LIFXPacket
    {
        // Packet type 0x24 - app to bulb
        // No Payload.  Expect Packet 0x25 MCU Rail Voltage in response.
        public LIFX_GetMCURailVoltage()
        {
        }
    }
    class LIFX_MCURailVoltage : LIFXPacket
    {
        // Packet type 0x25 - bulb to app
        // 4 byte payload
        public UInt32 voltage;
        public LIFX_MCURailVoltage()
        {
            voltage = 0;
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
    class LIFX_Reboot : LIFXPacket
    {
        // Packet type 0x26 - app to bulb
        // No payload.  No packet response expected
        public LIFX_Reboot()
        {
        }
    }
    class LIFX_SetFactoryTestMode : LIFXPacket
    {
        // Packet type 0x27 - app to bulb
        // 1 byte payload, unknown contents.
        public byte unknown;
        public LIFX_SetFactoryTestMode()
        {
            unknown = 1;
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
    class LIFX_DisableFactoryTestMode : LIFXPacket
    {
        // Packet type 0x28 - app to bulb
        // No payload.  No packet response expected
        public LIFX_DisableFactoryTestMode()
        {
        }
    }
}
