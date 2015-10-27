namespace ConsoleApplication1
{
    using System;
    using System.IO;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Serialization;

    [Serializable]
    public class DiffData
    {        
        public string FullXPath { get; set; }
        public XElement Element { get; set; }
        public XElement ChangedElement { get; set; }
        public string Action { get; set; }
        public string ToBeDeleted { get; set; }
    }
}