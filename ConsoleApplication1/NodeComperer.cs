namespace ConsoleApplication1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;

    public static class NodeComperer
    {
        static public bool Compare(XElement x, XElement y)
        {
            if (x.Elements().Count() == y.Elements().Count() 
                && x.Attributes().Count() == y.Attributes().Count()
                && x.Value == y.Value
                )
            {
                Console.WriteLine("aaa");
                return true;
            }

            return false;
        }

        //public int GetHashCode(XElement number)
        //{
        //    return number.GetHashCode();
        //}
    }
}
