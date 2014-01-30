using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LIFX;
using System.Threading;
using Mono.Documentation;

namespace LifxController
{
    class Program
    {
        static LIFXNetwork Network;
        static void Main(string[] args)
        {
            bool help = false;
            bool scan = false;
            string hue = "0";
            string sat = "0";
            string bright = "0";
            string kelvin = "0";
            string fade = "0";
            string label = "";

            Options options = new Options();
            options.Add("?|help", "Prints out the options.", option => help = option != null);
            options.Add("scan", "Scans the network and looks for bulbs", option => scan = option != null);
            options.Add("h|hue", "Set the hue", option => hue = option);
            options.Add("s|sat|saturation", "Set the saturation", option => sat = option);
            options.Add("b|bright|brightness", "Set the brightness", option => bright = option);
            options.Add("k|kelvin|t|temperature|temp", "Set the colour temperature", option => kelvin = option);
            options.Add("f|fade", "Set the fade time.", option => fade = option);
            options.Add("l|label", "Target bulb with this label", option => label = option);
            try
            {
                options.Parse(args);
            }
            catch (Exception)
            {
                show_help("Error - usage is:", options);
            }

            if (help)
            {
                const string usage_message =
                    "LifxController.exe /h[ue] VALUE /s[aturation] VALUE /b[rightness] VALUE /k[elvin] VALUE /f[ade] VALUE /l[abel] VALUE";
                show_help(usage_message, options);
            }
            Network = new LIFXNetwork();
            
            if (scan)
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
                    bulb.BulbSocket.Connect(bg[0].endPoint);
                }
                            
                SetBulbValue(ushort.Parse(hue), ushort.Parse(sat), ushort.Parse(bright), ushort.Parse(kelvin), uint.Parse(fade), Network.bulbs, label);
            }
        }
        static void SetBulbValue(ushort hue, ushort saturation, ushort brightness, ushort kelvin, uint fade, Bulbs bulbs, string Label)
        {
            foreach (LIFX.LIFXBulb bulb in bulbs)
                if (bulb.Label == Label)
                    Network.SetBulbValues(hue, saturation, brightness, kelvin, fade, bulb);
        }

        static void show_help(string message, Options options)
        {
            Console.Error.WriteLine(message);
            options.WriteOptionDescriptions(Console.Error);
            Environment.Exit(-1);
        }

    }
}
