namespace DataCompareLibrary
{
    using System.Collections.Generic;
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
                var writer = System.Xml.XmlWriter.Create(fileName);

                Diffgram res = diff.Compare(firstFile.DocumentElement, secondFIle.DocumentElement, writer);

                var patchFile = new XmlDocument();
                patchFile.Load("rte.xml");

                writer.Close();

                XmlReader reader = XmlReader.Create("tmpFile1.xml");
                XmlPatch patch = new XmlPatch();
                patch.Patch(patchFile, reader);


                writer.Close();
            }

            this.ParseDiffFiles();
        }

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
