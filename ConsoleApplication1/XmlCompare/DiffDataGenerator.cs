namespace ConsoleApplication1.XmlCompare
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using System.Xml.Schema;
    using System.Xml.XPath;

    using ConsoleApplication1.Models;

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
            this.XsdSchemaValidation();            
        }

        public List<DiffDataElement> GenerateDiffData()
        {
            var comparer = new XmlComparer();
            var diffNode = comparer.Compare(this.firstDoc.Root, this.secondDoc.Root);
            this.diffData = new List<DiffDataElement>();
            this.GetElements(diffNode);
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
                    try
                    {
                        data1.Element.Attribute("xmlns").Remove();
                    }
                    catch (Exception)
                    {

                        // nothing
                    }

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

                    try
                    {
                        data1.ChangedElement.Attribute("xmlns").Remove();
                    }
                    catch (Exception)
                    {

                        // nothing
                    }

                    this.diffData.Add(data1);
                }

                this.GetElements(e);
            }
        }
    }
}
