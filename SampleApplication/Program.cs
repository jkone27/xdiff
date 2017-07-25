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
            var diffs = XmlDiffUtils.ComputeDiff("A.xml", "B.xml", new string[] {});
            Console.WriteLine("diff for A.XML vs B.XML\r\n");
            foreach (var d in diffs)
                Console.WriteLine(d);
            Console.ReadKey();
        }
    }
}
