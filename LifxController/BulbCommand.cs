using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LIFX.LifxController
{
    class BulbCommand
    {
        public bool help;
        public bool scan;
        public bool list;
        public string hue;
        public string sat;
        public string bright;
        public string kelvin;
        public string fade;
        public string label;
        public string scanduration;
        public BulbCommand()
        {
            hue = "0";
            sat = "0";
            bright = "0";
            kelvin = "0";
            fade = "0";
            scanduration = "4";
            label = "All";
            help = false;
            scan = false;
            list = false;
        }
    }
}
