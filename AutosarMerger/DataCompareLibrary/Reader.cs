namespace DataCompareLibrary
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Xml;

    using DataCompareLibrary.Models;

    using Microsoft.XmlDiffPatch;

    public class Reader
    {
        private const string DiffNameStrings = "tmpFile";

        private readonly IList<string> originalFiles;
        private readonly IList<string> mergeFiles;
        private readonly string xsdSchema;
        private readonly IList<string> tempDiffFiles;
        
        public Reader(IList<string> originalFiles, IList<string> mergeFiles, string xsdSchema)
        {
            // validate if the files are the same
            // validate files vs schema         
            this.originalFiles = originalFiles;
            this.mergeFiles = mergeFiles;
            this.xsdSchema = xsdSchema;
            this.tempDiffFiles = new List<string>();
        }

        public void CreateDiffFiles()
        {
            for (int i = 0; i < this.originalFiles.Count; i++)
            {
                var diff = new XmlDiff(XmlDiffOptions.IgnoreChildOrder | XmlDiffOptions.IgnoreNamespaces | XmlDiffOptions.IgnoreXmlDecl);
                var fileName = DiffNameStrings + (this.tempDiffFiles.Count + 1).ToString() + ".xml";
                this.tempDiffFiles.Add(fileName);

                var firstFile = new XmlDocument();
                var secondFIle = new XmlDocument();
                firstFile.Load(this.originalFiles[i]);
                secondFIle.Load(this.mergeFiles[i]);
                //var writer = System.Xml.XmlWriter.Create(fileName);

                //diff.Compare(firstFile.DocumentElement, secondFIle.DocumentElement, writer);

                //writer.Close();

                XmlDiffView dv = new XmlDiffView();
                //TextWriter aaa = new StringWriter();

             //   using (TextWriter aaa = File.CreateText(@"out.html"))
             //   {

                    //XmlTextReader orig = new XmlTextReader(this.originalFiles[i]);
                    //XmlTextReader diffGram = new XmlTextReader(fileName);
                    //dv.Load(orig,diffGram);

                File.Delete(@"test.html");
                    
                var res = dv.DifferencesSideBySideAsHtml(this.originalFiles[i], this.mergeFiles[i], @"test.html", false, XmlDiffOptions.IgnoreWhitespace);
              //  }

                //System.IO.File.WriteAllText(@"test.html", res.ReadToEnd());



            }

            this.ParseDiffFiles();
        }

        public static void CopyStream(Stream stream, string destPath)
        {
            using (var fileStream = new FileStream(destPath, FileMode.Create, FileAccess.Write))
            {
                stream.CopyTo(fileStream);
            }
        }

        //private void ApplyDiffgram(XmlNode diffgramParent, XmlDiffViewParentNode sourceParent)
        //{
        //    sourceParent.CreateSourceNodesIndex();
        //    XmlDiffViewNode currentPosition = null;

        //    IEnumerator diffgramChildren = diffgramParent.ChildNodes.GetEnumerator();
        //    while (diffgramChildren.MoveNext())
        //    {
        //        XmlNode diffgramNode = (XmlNode)diffgramChildren.Current;
        //        if (diffgramNode.NodeType == XmlNodeType.Comment)
        //            continue;

        //        XmlElement diffgramElement = diffgramChildren.Current as XmlElement;

        //        if (diffgramElement == null)
        //            throw new Exception("Invalid node in diffgram.");

        //        if (diffgramElement.NamespaceURI != XmlDiff.NamespaceUri)
        //            throw new Exception("Invalid element in diffgram.");

        //        string matchAttr = diffgramElement.GetAttribute("match");
        //        XmlDiffPathNodeList matchNodes = null;
        //        if (matchAttr != string.Empty)
        //            matchNodes = XmlDiffPath.SelectNodes(_doc, sourceParent, matchAttr);

        //        switch (diffgramElement.LocalName)
        //        {
        //            case "node":
        //                if (matchNodes.Count != 1)
        //                    throw new Exception("The 'match' attribute of 'node' element must select a single node.");
        //                matchNodes.MoveNext();
        //                if (diffgramElement.ChildNodes.Count > 0)
        //                    ApplyDiffgram(diffgramElement, (XmlDiffViewParentNode)matchNodes.Current);
        //                currentPosition = matchNodes.Current;
        //                break;
        //            case "add":
        //                if (matchAttr != string.Empty)
        //                {
        //                    OnAddMatch(diffgramElement, matchNodes, sourceParent, ref currentPosition);
        //                }
        //                else
        //                {
        //                    string typeAttr = diffgramElement.GetAttribute("type");
        //                    if (typeAttr != string.Empty)
        //                    {
        //                        OnAddNode(diffgramElement, typeAttr, sourceParent, ref currentPosition);
        //                    }
        //                    else
        //                    {
        //                        OnAddFragment(diffgramElement, sourceParent, ref currentPosition);
        //                    }
        //                }
        //                break;
        //            case "remove":
        //                OnRemove(diffgramElement, matchNodes, sourceParent, ref currentPosition);
        //                break;
        //            case "change":
        //                OnChange(diffgramElement, matchNodes, sourceParent, ref currentPosition);
        //                break;
        //        }
        //    }
        //}

        //private static 

        public void ParseDiffFiles()
        {
            for (int i = 0; i < this.tempDiffFiles.Count; i++)
            {
                var a = DiffRowReader.Read(this.tempDiffFiles[i]);
            }
        }
    }
}
