using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.XPath;

    using ConsoleApplication1.Models;
    using ConsoleApplication1.XmlCompare;

    using XmlDiff;



    static class Program
    {
        private static List<DiffDataElement> data;

        private static XDocument doc1;

        private static XDocument doc2;


        private static void Main(string[] args)
        {
            // compare
            DiffDataGenerator gen = new DiffDataGenerator(@"D:\old.xml", @"D:\new.xml", "AUTOSAR.xsd");
            var res = gen.GenerateDiffData();
            DataStore.Save(res, "data.res");

            // merge

            var result = DataStore.Load<List<DiffDataElement>>("data.res");            
            DiffDataReader reader = new DiffDataReader(result);
            reader.ApplyDifferencesToFile(@"d:\old1.xml");

        }

    }
}
