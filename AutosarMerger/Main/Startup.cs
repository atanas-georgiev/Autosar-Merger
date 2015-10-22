using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main
{
    using DataCompareLibrary;

    class Startup
    {
        static void Main()
        {
            var list1 = new List<string>();
            var list2 = new List<string>();

            list1.Add(@"D:\New.xml");
         //   list1.Add(@"D:\test\Rte.xml");
            list2.Add(@"D:\Old.xml");
         //   list2.Add(@"D:\test1\Rte.xml");

            var merger = new Reader(list1, list2, "");
            merger.CreateDiffFiles();
        }
    }
}
