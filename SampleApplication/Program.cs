using System;
using XDiff;

namespace SampleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            var diffs = XmlDiffUtils.FilesDiff("A.xml", "B.xml", new string[] {});
            Console.WriteLine("diff for A.XML vs B.XML\r\n");
            foreach (var d in diffs)
                Console.WriteLine(d);
            Console.ReadKey();
        }
    }
}
