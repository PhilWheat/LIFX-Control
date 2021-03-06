﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LIFX;
using System.Threading;

namespace LIFX.LifxController
{
    class Program
    {
        static IEnumerable<string> _(params string[] a)
        {
            return a;
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
                {
                    bc.scan = true;
                    if (arg.Substring(1).StartsWith("scan="))
                        bc.scanduration = arg.Substring(6);
                }
                else if (arg.StartsWith("-list"))
                    bc.list = true;
                else
                    bc.help = true;
            }
            return bc;
        }

        static LIFXNetwork Network;
        static void Main(string[] args)
        {
            BulbCommand bc = ParseArguments(_(args));
            if (bc.help)
                show_help();

            Network = new LIFXNetwork();
            SerializableBulbs bulbs;
            if (bc.scan)
            {
                Network.Start();
                Thread.Sleep(int.Parse(bc.scanduration) * 1000);
                Console.WriteLine("Found: " + Network.bulbs.Count + " bulb(s)");
                bulbs = new SerializableBulbs(Network.bulbs);
                bulbs.SaveAs("Bulbs.xml");
            }
            else
            {
                Network.Setup();
                Console.WriteLine("Loading saved bulb state information.");
                bulbs = (SerializableBulbs)SerializableBulbs.Load("Bulbs.xml");

                if (bulbs.Count > 0)
                {
                    Console.WriteLine("Found: " + bulbs.Count + " bulb(s)");
                    Network.bulbs = bulbs.LIFXBulbs;
                }
                else
                {
                    Console.WriteLine("No bulbs found in saved bulb state, scanning network.");
                    Network.Start();
                    Thread.Sleep(int.Parse(bc.scanduration)*1000);
                    
                    bulbs = new SerializableBulbs(Network.bulbs);
                    Console.WriteLine("Found: " + bulbs.Count + " bulb(s)");
                    bulbs.SaveAs("Bulbs.xml");
                }
            }

            if (bc.list)
                ListBulbs(Network.bulbs);
            else
            {
                if (bc.label.ToLower()=="all")
                    SetAllBulbValues(ushort.Parse(bc.hue), ushort.Parse(bc.sat), ushort.Parse(bc.bright), ushort.Parse(bc.kelvin), uint.Parse(bc.fade));
                else
                    SetBulbValue(ushort.Parse(bc.hue), ushort.Parse(bc.sat), ushort.Parse(bc.bright), ushort.Parse(bc.kelvin), uint.Parse(bc.fade), Network.bulbs, bc.label);
            }
        }
        static void ListBulbs(List<LIFX.LIFXBulb> bulbs)
        {
            Console.WriteLine("Bulbs:");
            foreach (LIFX.LIFXBulb bulb in bulbs)
                Console.WriteLine("\t"+bulb.Label);
        }
        static void SetAllBulbValues(ushort hue, ushort saturation, ushort brightness, ushort kelvin, uint fade)
        {
            Network.SetAllBulbValues(hue, saturation, brightness, kelvin, fade, 1);
        }
        static void SetBulbValue(ushort hue, ushort saturation, ushort brightness, ushort kelvin, uint fade, List<LIFXBulb> bulbs, string Label)
        {
            foreach (LIFX.LIFXBulb bulb in bulbs)
                if (bulb.Label == Label)
                    Network.SetBulbValues(hue, saturation, brightness, kelvin, fade, bulb);
        }

        static void show_help()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("LifxController.exe\t-h[ue]=VALUE");
            sb.AppendLine("\t\t\t-s[aturation]=VALUE");
            sb.AppendLine("\t\t\t-b[right]=VALUE");
            sb.AppendLine("\t\t\t-t[emp]=VALUE");
            sb.AppendLine("\t\t\t-f[ade]=VALUE");
            sb.AppendLine("\t\t\t-l[abel]=VALUE\n");
            sb.AppendLine("\t\t\t-help\n");
            sb.AppendLine("\t\t\t-scan[=VALUE]\n");
            sb.AppendLine("hue\tHue value between 0 and 65,536\n\t(only valid if saturation is greater than zero.)\n");
            sb.AppendLine("sat\tColour saturation value between 0 and 65,536\n");
            sb.AppendLine("bright\tBrightness value between 0 and 65,536\n");
            sb.AppendLine("temp\tColour temperare in Kelvin Value between 0 and 65,536");
            sb.AppendLine("\tTypical colour values are:");
            sb.AppendLine("\t\tcandle=1850-1930K");
            sb.AppendLine("\t\tsunrise/sunset=2000-3000K");
            sb.AppendLine("\t\thousehold incandescent=2500-2900K");
            sb.AppendLine("\t\tdaylight=5500-6500K");
            sb.AppendLine("\t\tovercast=6000-7500K");
            sb.AppendLine("\t\tcloudy sky=8000-10000K\n");
            sb.AppendLine("fade\tSeconds to fade between 0 and 65,536\n\t(only valid if saturation is greater than zero.)\n");
            sb.AppendLine("label\tWhich bulb you want to control\n\t(All for all detected bulbs, use -list to list bulbs)\n");
            sb.AppendLine("help\tThis help message\n");
            sb.AppendLine("scan\tScan for bulbs, VALUE is in seconds\n");
            sb.AppendLine("list\tList found bulbs\n");
            Console.Error.WriteLine(sb.ToString());
            Environment.Exit(-1);
        }

    }
}
