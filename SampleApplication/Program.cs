using System;
using static XmlDifference.DiffExtensions.DifferenceExtension;

namespace SampleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            var diffs = "A.xml".Difference("B.Xml");
            Console.WriteLine("diff for A.XML vs B.XML\r\n");
            foreach (var d in diffs)
                Console.WriteLine(d);
            Console.ReadKey();
        }
    }
}
