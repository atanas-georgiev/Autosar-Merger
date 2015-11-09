namespace ConsoleApplication1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Xml.Linq;

    public static class XExtensions
    {
        public static int Position(this XElement node)
        {
            return node.NodesBeforeSelf().Count();
        }

        public static String InnerXml(this XElement source)
        {
            return source.Descendants().Select(x => x.ToString()).Aggregate(String.Concat);
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

        /// <summary> 
        /// Get the absolute XPath to a given XElement
        /// (e.g. "/people/person[6]/name[1]/last[1]").
        /// </summary> 
        /// <param name="element"> 
        /// The element to get the index of.
        /// </param> 
        public static string AbsoluteXPath(this XElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            Func<XElement, string> relativeXPath = e =>
            {
                int index = e.IndexPosition();
                string name = e.Name.LocalName;

                // If the element is the root, no index is required

                return (index == -1) ? "/" + "*[local-name()='" + name + "']" : string.Format
                (
                    "/*[local-name()='{0}'][{1}]",
                    name,
                    index.ToString()
                );
            };

            var ancestors = from e in element.Ancestors()
                            select relativeXPath(e);

            return string.Concat(ancestors.Reverse().ToArray()) +
                   relativeXPath(element);
        }

        /// <summary> 
        /// Get the index of the given XElement relative to its
        /// siblings with identical names. If the given element is
        /// the root, -1 is returned.
        /// </summary> 
        /// <param name="element"> 
        /// The element to get the index of.
        /// </param> 
        public static int IndexPosition(this XElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            if (element.Parent == null)
            {
                return -1;
            }

            int i = 1; // Indexes for nodes start at 1, not 0

            foreach (var sibling in element.Parent.Elements(element.Name))
            {
                if (sibling == element)
                {
                    return i;
                }

                i++;
            }

            throw new InvalidOperationException
                ("element has been removed from its parent.");
        }
    }
}

