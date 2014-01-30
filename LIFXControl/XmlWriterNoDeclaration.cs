using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;

namespace LIFX
{
    public class XmlWriterNoDeclaration : XmlTextWriter
    {
        public XmlWriterNoDeclaration(TextWriter w)
            : base(w)
        {
            Formatting = Formatting.Indented;
        }

        public override void WriteStartDocument() { }
    }
}
