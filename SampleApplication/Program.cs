using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using XDiff;

namespace SampleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            var a = XDocument.Parse(File.ReadAllText("A.xml"));
            var b = XDocument.Parse(File.ReadAllText("B.xml"));

            //TODO: --> put in library
            var aX = XmlDiffUtils.xelToDomain(a.Root, Types.XmlNode.Empty);
            var bX = XmlDiffUtils.xelToDomain(b.Root, Types.XmlNode.Empty);

            //TODO: --> expose easier types for C# client
            var diffs = XmlDiffUtils.MakeDiff(aX,bX)
        }
    }
}
