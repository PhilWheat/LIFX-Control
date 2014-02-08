using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Diagnostics;

namespace LIFX.LifxController
{

    [Serializable]
    [XmlRoot("Bulbs")]
    public class SerializableBulbs : BindingList<SerializableBulb>
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
                XmlSerializer x = new XmlSerializer(typeof(SerializableBulbs));
                ns.Add("", "");

                StringWriter sw = new StringWriter();
                XmlWriter writer = new XmlWriterNoDeclaration(sw);
                x.Serialize(writer, this, ns);

                StreamWriter fs = File.CreateText(filename);
                fs.Write(sw.ToString());
                fs.Close();
            }
        }
        public SerializableBulbs(List<LIFX.LIFXBulb> bulbs)            
        {
            foreach (LIFXBulb bulb in bulbs)
            {
                this.Add(new SerializableBulb(bulb));
            }
        }
        public SerializableBulbs()
        {
        }
        public List<LIFXBulb> LIFXBulbs
        {
            get {
                List<LIFXBulb> l = new List<LIFXBulb>();
                foreach (SerializableBulb b in this)
                {
                    LIFXBulb bulb = new LIFXBulb();
                    bulb.BulbGateWay = b.BulbGateWay;
                    bulb.BulbMac = b.BulbMac;
                    l.Add(bulb);
                }
                return l;
            }
        }
        public static SerializableBulbs Load(string filename)
        {
            Debug.WriteLine(DateTime.Now.ToString() + ": Entering Bulbs.Load(string filename)");
            SerializableBulbs t;
            if (File.Exists(filename))
            {
                try
                {
                    XmlSerializer x = new XmlSerializer(typeof(SerializableBulbs));
                    using (StreamReader sr = new StreamReader(filename))
                        t = (SerializableBulbs)x.Deserialize(sr);
                    t.Filename = filename;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(DateTime.Now.ToString() + ": Error deserializing '" + filename + "', bailing!");
                    Debug.WriteLine(DateTime.Now.ToString() + ": " + ex.Message);
                    Debug.WriteLine(DateTime.Now.ToString() + ": " + ex.StackTrace);
                    t = new SerializableBulbs();
                    t.Filename = filename;
                }
            }
            else
            {
                t = new SerializableBulbs();
                t.Filename = filename;
            }
            Debug.WriteLine(DateTime.Now.ToString() + ": Finished SerializableBindingList.Load(string filename)");
            return t;
        }
    }
}
