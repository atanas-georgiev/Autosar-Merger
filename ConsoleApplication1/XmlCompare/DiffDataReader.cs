using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1.XmlCompare
{
    using System.Xml.Linq;
    using System.Xml.XPath;

    using ConsoleApplication1.Models;

    class DiffDataReader
    {
        private List<DiffDataElement> differences;

        public DiffDataReader(List<DiffDataElement> differences)
        {
            this.differences = differences;
        }

        public void ApplyDifferencesToFile(string outputFile)
        {
            var outDoc = XDocument.Load(outputFile);
            var elementsToDelete = new List<XElement>();            

            foreach (DiffDataElement diff in this.differences)
            {             
                switch (diff.Action)
                {
                    case "Added":
                        var path = ConvertAddPath(diff.FullXPath);
                        var node3 = outDoc.XPathSelectElement(path.Item1);

                        if (path.Item2 == 0)
                        {
                            node3.Add(new XElement(diff.Element));
                        }
                        else
                        {
                            var childrens = node3.Elements().ToArray();
                            var child = childrens[path.Item2 - 1];

                            child.AddAfterSelf(new XElement(diff.Element));
                        }

                        break;

                    case "Changed":
                        var node = outDoc.XPathSelectElement(diff.FullXPath);

                        if (node == null)
                        {
                            throw new Exception();
                        }    

                        node.ReplaceWith(new XElement(diff.ChangedElement));

                        break;

                    case "Removed":
                        var node2 = outDoc.XPathSelectElement(diff.FullXPath);
                        try
                        {
                            elementsToDelete.Add(node2);
                        }
                        catch (Exception)
                        {

                            var a = 1;
                        }


                        break;

                }
            }


            foreach (var del in elementsToDelete)
            {
                del.Remove();
            }

            outDoc.Save(outputFile);
        }

        private static Tuple<string, int> ConvertAddPath(string path)
        {
            var fullPath = path.Split('/');
            var sb = new StringBuilder();

            for (int i = 0; i < fullPath.Length - 1; i++)
            {
                if (fullPath[i] != "")
                {
                    sb.Append("/");
                    sb.Append(fullPath[i]);
                }
            }

            var fullIndexes = path.Split(new char[] { '[', ']' });
            var indexToAdd = int.Parse(fullIndexes[fullIndexes.Length - 2]);

            if (indexToAdd > 0)
            {
                indexToAdd -= 1;
            }

            var result = new Tuple<string, int>(sb.ToString(), indexToAdd);

            return result;
        }
    }
}
