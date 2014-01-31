using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LIFX;
using System.Threading;

namespace LifxController
{
    class Program
    {
        static IEnumerable<string> _(params string[] a)
        {
            return a;
        }

        class BulbCommand
        {
            public bool help;
            public bool scan;
            public string hue;
            public string sat;
            public string bright;
            public string kelvin;
            public string fade;
            public string label;
            public BulbCommand()
            {
                hue = "0";
                sat = "0";
                bright = "0";
                kelvin = "0";
                fade = "0";
                label = "All";
                help = false;
                scan = false;
            }
        }
        static BulbCommand ParseArguments(IEnumerable<string> args)
        {
            BulbCommand bc = new BulbCommand();
            foreach (string arg in args)
            {
                if (arg.StartsWith("-h="))
                    bc.hue = arg.Substring(3);
                else if (arg.StartsWith("-hue="))
                    bc.hue = arg.Substring(5);
                else if (arg.StartsWith("-s="))
                    bc.sat = arg.Substring(3);
                else if (arg.StartsWith("-sat="))
                    bc.sat = arg.Substring(5);
                else if (arg.StartsWith("-saturation="))
                    bc.sat = arg.Substring(12);
                else if (arg.StartsWith("-b="))
                    bc.bright = arg.Substring(3);
                else if (arg.StartsWith("-bright="))
                    bc.bright = arg.Substring(8);
                else if (arg.StartsWith("-brightness="))
                    bc.bright = arg.Substring(12);
                else if (arg.StartsWith("-k="))
                    bc.kelvin = arg.Substring(3);
                else if (arg.StartsWith("-kelvin="))
                    bc.kelvin = arg.Substring(8);
                else if (arg.StartsWith("-t="))
                    bc.kelvin = arg.Substring(3);
                else if (arg.StartsWith("-temp="))
                    bc.kelvin = arg.Substring(6);
                else if (arg.StartsWith("-temperature="))
                    bc.kelvin = arg.Substring(13);
                else if (arg.StartsWith("-f="))
                    bc.fade = arg.Substring(3);
                else if (arg.StartsWith("-fade="))
                    bc.fade = arg.Substring(6);
                else if (arg.StartsWith("-l="))
                    bc.label = arg.Substring(3).Replace("'", "");
                else if (arg.StartsWith("-label="))
                    bc.label = arg.Substring(7).Replace("'", "");
                else if (arg.StartsWith("-scan") || arg.StartsWith("/scan"))
                    bc.scan = true;
                else
                    bc.help = true;
            }
            return bc;
        }

        static LIFXNetwork Network;
        static void Main(string[] args)
        {
            BulbCommand bc = ParseArguments(_("-scan", "-l='Lounge Room'", "-b=60000", "-h=0", "-s=0", "-k=3800", "-f=10"));
            if (bc.help)
            {
                const string usage_message =
                    "LifxController.exe /h[ue]=VALUE /s[aturation]=VALUE /b[rightness]=VALUE /k[elvin]=VALUE /f[ade]=VALUE /l[abel]=VALUE";
                show_help(usage_message);
            }
            Network = new LIFXNetwork();
            
            if (bc.scan)
            {
                Network.Start();                
                Thread.Sleep(4000);
                Console.WriteLine("Found: " + Network.bulbs.Count + " bulb(s)");
                Network.bulbs.SaveAs("Bulbs.xml");
                Network.tcpGateways.SaveAs("Gateways.xml");
            }
            else
            {
                Network.Setup();
                Bulbs b = (Bulbs)Bulbs.Load("Bulbs.xml");
                BulbGateways bg = (BulbGateways)BulbGateways.Load("Gateways.xml");

                if (b.Count > 0)
                {
                    Network.bulbs = b;
                    Network.tcpGateways = bg;
                }
                else
                {
                    Console.WriteLine("No bulbs found, scanning anyway.");
                    Network.Start();                
                    Thread.Sleep(4000);
                    Console.WriteLine("Found: " + Network.bulbs.Count + " bulb(s)");
                    Network.bulbs.SaveAs("Bulbs.xml");
                    Network.tcpGateways.SaveAs("Gateways.xml");
                }

                foreach (LIFX.LIFXBulb bulb in Network.bulbs)
                {
                    //Connect bulb socket's to gateway endpoint
                    try
                    {
                        if ((bg!=null) && (bg.Count>0))
                            bulb.BulbSocket.Connect(bg[0].endPoint);
                    }
                    catch (System.Net.Sockets.SocketException) { }
                }
                            
                SetBulbValue(ushort.Parse(bc.hue), ushort.Parse(bc.sat), ushort.Parse(bc.bright), ushort.Parse(bc.kelvin), uint.Parse(bc.fade), Network.bulbs, bc.label);
            }
        }
        static void SetBulbValue(ushort hue, ushort saturation, ushort brightness, ushort kelvin, uint fade, Bulbs bulbs, string Label)
        {
            foreach (LIFX.LIFXBulb bulb in bulbs)
                if (bulb.Label == Label)
                    Network.SetBulbValues(hue, saturation, brightness, kelvin, fade, bulb);
        }

        static void show_help(string message)
        {
            Console.Error.WriteLine(message);            
            Environment.Exit(-1);
        }

    }
}
