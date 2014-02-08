using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Xml.Serialization;
namespace LIFX.LifxController
{
    [Serializable]
    public class SerializableBulb
    {
        public byte[] BulbGateWay;
        public byte[] BulbMac;
        public const int Port = 56700;
        public byte[] bulbEndpoint
        {
                get
                {
                    if (BulbEndpoint != null)
                        return BulbEndpoint.Address.GetAddressBytes();
                    else
                        return null;
                }
                set { BulbEndpoint = new IPEndPoint(new IPAddress(value), Port); }
            }
            [XmlIgnore]
            public IPEndPoint BulbEndpoint;

            public LIFX.LIFXBulb GetLIFXBulb()
            {
                LIFX.LIFXBulb bulb = new LIFX.LIFXBulb();
                bulb.BulbMac = BulbMac;
                bulb.BulbGateWay = BulbGateWay;
                return bulb;
            }

    }
}
