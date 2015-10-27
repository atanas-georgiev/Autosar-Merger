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

    using XmlDiff;



    static class Program
    {
        private static List<DiffData> data;

        private static XDocument doc1;

        private static XDocument doc2;

        static void GetElements(DiffNode element)
        {
            var elements = element.Childs;

            foreach (var e in elements)
            {
                if (e.DiffAction != null)
                {
                    DiffData data1 = new DiffData();
                    data1.Element = e.Raw;
                    data1.Action = e.DiffAction.ToString();
                    data1.FullXPath = e.Raw.AbsoluteXPath();                    
                    data.Add(data1);
                }
                else if (e.IsChanged && e.Childs.ToList().Count == 0)
                {
                    DiffData data1 = new DiffData();
                    data1.ChangedElement = e.Raw;
                    data1.Action = "Changed";
                    data1.FullXPath = e.Raw.AbsoluteXPath();                                        
                    data1.Element = doc1.XPathSelectElement(data1.FullXPath);
                    if (data1.Element == null)
                    {
                        throw new Exception();
                    }
                    data.Add(data1);
                }

                GetElements(e);                
            }
        }

        public static void Sort(this XElement source, bool bSortAttributes = true)
        {
            //Make sure there is a valid source
            if (source == null) throw new ArgumentNullException("source");

            //Sort attributes if needed
            if (bSortAttributes)
            {
                List<XAttribute> sortedAttributes = source.Attributes().OrderBy(a => a.ToString()).ToList();
                sortedAttributes.ForEach(a => a.Remove());
                sortedAttributes.ForEach(a => source.Add(a));
            }

            //Sort the children IF any exist
            List<XElement> sortedChildren = source.Elements().OrderBy(e => e.Name.ToString()).ToList();
            if (source.HasElements)
            {
                source.RemoveNodes();
                sortedChildren.ForEach(c => c.Sort());
                sortedChildren.ForEach(c => source.Add(c));
            }
        }

        static void Main(string[] args)
        {
            data = new List<DiffData>();
            var comparer = new XmlComparer();

            doc1 = XDocument.Load(@"D:\Systems\35UP_PU1607_I410.03_004.013.004\KSS\Tools\Davinci_Appl\Config\Developer\ComponentTypes\Fscsm.arxml");
            doc2 = XDocument.Load(@"D:\Systems\35UP_PU1607_I410.05_004.013.008\KSS\Tools\Davinci_Appl\Config\Developer\ComponentTypes\Fscsm.arxml");
            //doc1.Root.Sort();
            //doc2.Root.Sort();

            var diff = comparer.Compare(doc1.Root, doc2.Root);
            GetElements(diff);
            

            //var d = data.g(x => x.FullXPath).ToList();


            //for (int i = 0; i < data.Count; i++)
            //{
            //    for (int j = 0; j < data.Count; j++)
            //    {
            //        if (XNode.DeepEquals(data[i].Element, data[j].ChangedElement)
            //            && XNode.DeepEquals(data[j].Element, data[i].ChangedElement))
            //        {
            //            data[i].ToBeDeleted = string.Empty;
            //            data[j].ToBeDeleted = string.Empty;
            //        }
            //    }
            //}

            //data.RemoveAll(x => x.ToBeDeleted == string.Empty);

            DataStore.Save(data, "data.res");

            // second part 

            var result = DataStore.Load<List<DiffData>>("data.res");

            foreach (DiffData r in result)
            {
                switch (r.Action)
                {
                    case "Changed":
                        var node = doc1.XPathSelectElement(r.FullXPath);

                        if (node == null)
                        {
                            throw new Exception();
                        }

                        //XAttribute attr = r.ChangedElement.Attribute("xmlns");
                        r.ChangedElement.Attribute("xmlns").Remove();
                        node.ReplaceWith(new XElement(r.ChangedElement));
                      
                        break;
                    case "Removed":
                        //var node2 = doc1.XPathSelectElement(r.FullXPath);

                        //if (node2 == null)
                        //{
                        //    throw new Exception();
                        //}  

                        //node2.Remove();

                        break;
                    case "Added":
                        //var node3 = doc1.XPathSelectElement(r.FullXPath);

                        //if (node3 == null)
                        //{
                        //    throw new Exception();
                        //}  

                        //node2.Remove();
                        break;

                }
            }



            doc1.Save("result.xml");
        }

        public static XElement RemoveAllNamespaces(XElement e)
        {
            return new XElement(e.Name.LocalName,
              (from n in e.Nodes()
               select ((n is XElement) ? RemoveAllNamespaces(n as XElement) : n)),
                  (e.HasAttributes) ?
                    (from a in e.Attributes()
                     where (!a.IsNamespaceDeclaration)
                     select new XAttribute(a.Name.LocalName, a.Value)) : null);
        }          
    }
}
