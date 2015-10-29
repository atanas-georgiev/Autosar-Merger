namespace ConsoleApplication1.XmlCompare
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using System.Xml.Schema;
    using System.Xml.XPath;

    using ConsoleApplication1.Models;
    using ConsoleApplication1;

    using XmlDiff;

    class DiffDataGenerator
    {

        private string xsdSchema;

        private XDocument firstDoc;

        private XDocument secondDoc;   

        private List<DiffDataElement> diffData;

        public DiffDataGenerator(string xmlFile1, string xmlFile2, string xsdSchema)
        {            
            // validate input files
            this.firstDoc = XDocument.Load(xmlFile1);
            this.secondDoc = XDocument.Load(xmlFile2);

            if (this.firstDoc == null || this.secondDoc == null)
            {
                throw new ArgumentNullException("Input files are not valid xml documents!");
            }

            this.xsdSchema = xsdSchema;

            // validate input documents
      //todo: schema validation      this.XsdSchemaValidation();            
        }

        private bool AreSimilarChilds(XElement main, XElement toCompare)
        {
            if (main == null || toCompare == null)
            {
                return false;
            }

            if (XNode.DeepEquals(main, toCompare))
            {
                return true;
            }

            var elements = main.Elements().ToList();

            for (int i = 0; i < elements.Count(); i++)
            {
                if (XNode.DeepEquals(elements[i], toCompare))
                {
                    elements[i] = new XElement(toCompare);
                    return true;
                }
            }

            return false;
        }

        private void NormalizeDataDel(List<DiffDataElement> data)
        {
            if (data.Count > 1)
            {
                for (int i = 1; i < data.Count; i++)
                {
                    var 
                        el =
                        data[i].Element.DescendantsAndSelf().FirstOrDefault(x => XNode.DeepEquals(x, data[i - 1].ChangedElement));
                    
                    if (el != null)
                    {
                        el.RemoveAll();
                        el.Value = data[i - 1].Element.Value;

                        foreach (var attr in data[i - 1].Element.Attributes())
                        {
                            el.Add(attr);
                        }

                        foreach (var elements in data[i - 1].Element.Elements())
                        {
                            el.Add(elements);
                        }

                        data[i - 1].ToBeDeleted = "true";
                    }
                }

                data.RemoveAll(x => x.ToBeDeleted != null);
            }
        }

        private void NormalizeDataAdd(List<DiffDataElement> data)
        {
            if (data.Count > 1)
            {
                for (int i = data.Count - 1; i >= 1; i--)
                {
                    var el =
                        data[i].Element.DescendantsAndSelf().FirstOrDefault(x => XNode.DeepEquals(x, data[i - 1].Element));

                    if (el != null && data[i].ChangedElement == null && data[i - 1].ChangedElement != null)
                    {
                        el.RemoveAll();
                        el.Value = data[i - 1].ChangedElement.Value;

                        foreach (var attr in data[i - 1].ChangedElement.Attributes())
                        {
                            el.Add(attr);
                        }

                        foreach (var elements in data[i - 1].ChangedElement.Elements())
                        {
                            el.Add(elements);
                        }
                        data.Remove(data[i - 1]);                        
                    }
                }                
            }
        }

        private void NormalizeDataChanged(List<DiffDataElement> data)
        {
            foreach (var d in data.Where(x => x.Action == "Changed").Where(d => XNode.DeepEquals(d.Element, d.ChangedElement)))
            {
                d.ToBeDeleted = "true";
            }
            data.RemoveAll(x => x.ToBeDeleted != null);
        }

        public List<DiffDataElement> GenerateDiffData()
        {
            var comparer = new XmlComparer();        
            var diffNode = comparer.Compare(this.firstDoc.Root, this.secondDoc.Root);
            this.diffData = new List<DiffDataElement>();
            this.GetElements(diffNode);
            this.NormalizeDataDel(this.diffData);
            this.NormalizeDataAdd(this.diffData);
            this.NormalizeDataChanged(this.diffData);
            return this.diffData;
        }

        private void XsdSchemaValidation()
        {
            var ns = this.firstDoc.Root.GetDefaultNamespace();
            var ns2 = this.secondDoc.Root.GetDefaultNamespace();

            if (ns.NamespaceName != ns2.NamespaceName)
            {
                throw new XmlSchemaException("Input files do not have the same namespaces!");
            }

            var schemas = new XmlSchemaSet();
            schemas.Add(ns.NamespaceName, this.xsdSchema);

            this.firstDoc.Validate(schemas, this.ValidationEventHandler);
            this.secondDoc.Validate(schemas, this.ValidationEventHandler);            
        }

        private void ValidationEventHandler(object sender, ValidationEventArgs validationEventArgs)
        {
            Console.WriteLine("Done!");            
        }

        private void GetElements(DiffNode element)
        {
            var elements = element.Childs;

            foreach (var e in elements)
            {
                if (e.DiffAction != null)
                {
                    var data1 = new DiffDataElement
                                    {
                                        Element = e.Raw,
                                        Action = e.DiffAction.ToString(),
                                        FullXPath = e.Raw.AbsoluteXPath()
                                    };


                    this.diffData.Add(data1);
                }
                else if (e.IsChanged && e.Childs.ToList().Count == 0)
                {
                    var data1 = new DiffDataElement
                                    {
                                        ChangedElement = e.Raw,
                                        Action = "Changed",
                                        FullXPath = e.Raw.AbsoluteXPath()
                                    };
                    data1.Element = this.firstDoc.XPathSelectElement(data1.FullXPath);
                    
                    if (data1.Element == null)
                    {
                        throw new Exception();
                    }

                    this.diffData.Add(data1);
                }

                this.GetElements(e);
            }
        }
    }
}
