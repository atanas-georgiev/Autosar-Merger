// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Comparer.cs" company="">
//   
// </copyright>
// <summary>
//   The comparer.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CustomXMLDiff
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;

    using CustomXMLDiff.Core.Diagnostics;
    using CustomXMLDiff.DiffManager;
    using CustomXMLDiff.DiffManager.DiffResult;

    using Microsoft.XmlDiffPatch;

    /// <summary>
    /// The comparer.
    /// </summary>
    public class Comparer : IDisposable
    {
        /// <summary>
        /// The _results.
        /// </summary>
        private static List<BaseDiffResultObject> _results;

        /// <summary>
        /// The _diff.
        /// </summary>
        private readonly XmlDiff _diff;

        /// <summary>
        /// The _temp file path.
        /// </summary>
        private readonly string _tempFilePath;

        /// <summary>
        /// The _diff options.
        /// </summary>
        private XmlDiffOptions _diffOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="Comparer"/> class.
        /// </summary>
        public Comparer()
        {
            _results = new List<BaseDiffResultObject>();
            this._diff = new XmlDiff();
            this._diffOptions = new XmlDiffOptions();
            this._tempFilePath = Path.GetTempFileName();
            this.Manager = new BaseDiffManager();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Comparer"/> class.
        /// </summary>
        /// <param name="manager">
        /// The manager.
        /// </param>
        public Comparer(IDiffManager manager)
        {
            _results = new List<BaseDiffResultObject>();
            this._diff = new XmlDiff();
            this._diffOptions = new XmlDiffOptions();
            this._tempFilePath = Path.GetTempFileName();
            this.Manager = manager;
        }

        /// <summary>
        /// Gets or sets the manager.
        /// </summary>
        public IDiffManager Manager { get; set; }

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
            if (File.Exists(this._tempFilePath))
            {
                File.Delete(this._tempFilePath);
            }
        }

        /// <summary>
        /// The do compare.
        /// </summary>
        /// <param name="originalDocumentPath">
        /// The original document path.
        /// </param>
        /// <param name="changedDocumentPath">
        /// The changed document path.
        /// </param>
        /// <returns>
        /// The <see cref="BaseDiffResultObjectList"/>.
        /// </returns>
        public BaseDiffResultObjectList DoCompare(string originalDocumentPath, string changedDocumentPath)
        {
            return this.DoCompare(
                originalDocumentPath, 
                changedDocumentPath, 
                this._tempFilePath, 
                XmlDiffOptions.IgnoreChildOrder);
        }

        /// <summary>
        /// The do compare.
        /// </summary>
        /// <param name="originalDocumentPath">
        /// The original document path.
        /// </param>
        /// <param name="changedDocumentPath">
        /// The changed document path.
        /// </param>
        /// <param name="outputFilePath">
        /// The output file path.
        /// </param>
        /// <returns>
        /// The <see cref="BaseDiffResultObjectList"/>.
        /// </returns>
        public BaseDiffResultObjectList DoCompare(
            string originalDocumentPath, 
            string changedDocumentPath, 
            string outputFilePath)
        {
            return this.DoCompare(
                originalDocumentPath, 
                changedDocumentPath, 
                outputFilePath, 
                XmlDiffOptions.IgnoreChildOrder);
        }

        /// <summary>
        /// The do compare.
        /// </summary>
        /// <param name="originalDocumentPath">
        /// The original document path.
        /// </param>
        /// <param name="changedDocumentPath">
        /// The changed document path.
        /// </param>
        /// <param name="options">
        /// The options.
        /// </param>
        /// <returns>
        /// The <see cref="BaseDiffResultObjectList"/>.
        /// </returns>
        public BaseDiffResultObjectList DoCompare(
            string originalDocumentPath, 
            string changedDocumentPath, 
            XmlDiffOptions options)
        {
            return this.DoCompare(originalDocumentPath, changedDocumentPath, this._tempFilePath, options);
        }

        /// <summary>
        /// The do compare.
        /// </summary>
        /// <param name="originalDocumentPath">
        /// The original document path.
        /// </param>
        /// <param name="changedDocumentPath">
        /// The changed document path.
        /// </param>
        /// <param name="outputFilePath">
        /// The output file path.
        /// </param>
        /// <param name="options">
        /// The options.
        /// </param>
        /// <returns>
        /// The <see cref="BaseDiffResultObjectList"/>.
        /// </returns>
        public BaseDiffResultObjectList DoCompare(
            string originalDocumentPath, 
            string changedDocumentPath, 
            string outputFilePath, 
            XmlDiffOptions options)
        {
            Assert.StringIsNullOrEmpty(originalDocumentPath, "First File path is empty");
            Assert.StringIsNullOrEmpty(changedDocumentPath, "Second File path is empty");
            Assert.StringIsNullOrEmpty(outputFilePath, "Output File Path is empty");

            var originalDocumentReader = new XmlTextReader(new StreamReader(originalDocumentPath));
            var changedDocumentReader = new XmlTextReader(new StreamReader(changedDocumentPath));

            var originalDocument = new XmlDocument();

            originalDocument.Load(originalDocumentReader);
            originalDocumentReader.Close();

            var changedDocument = new XmlDocument();
            changedDocument.Load(changedDocumentReader);
            return this.DoCompare(
                originalDocument.DocumentElement, 
                changedDocument.DocumentElement, 
                outputFilePath, 
                options);
        }

        /// <summary>
        /// The do compare.
        /// </summary>
        /// <param name="originalDocument">
        /// The original document.
        /// </param>
        /// <param name="changedDocument">
        /// The changed document.
        /// </param>
        /// <param name="outputFilePath">
        /// The output file path.
        /// </param>
        /// <param name="options">
        /// The options.
        /// </param>
        /// <returns>
        /// The <see cref="BaseDiffResultObjectList"/>.
        /// </returns>
        public BaseDiffResultObjectList DoCompare(
            XmlDocument originalDocument, 
            XmlDocument changedDocument, 
            string outputFilePath, 
            XmlDiffOptions options)
        {
            return this.DoCompare(
                originalDocument.DocumentElement, 
                changedDocument.DocumentElement, 
                outputFilePath, 
                options);
        }

        /// <summary>
        /// The do compare.
        /// </summary>
        /// <param name="originalDocument">
        /// The original document.
        /// </param>
        /// <param name="changedDocument">
        /// The changed document.
        /// </param>
        /// <param name="options">
        /// The options.
        /// </param>
        /// <returns>
        /// The <see cref="BaseDiffResultObjectList"/>.
        /// </returns>
        public BaseDiffResultObjectList DoCompare(
            XmlDocument originalDocument, 
            XmlDocument changedDocument, 
            XmlDiffOptions options)
        {
            return this.DoCompare(
                originalDocument.DocumentElement, 
                changedDocument.DocumentElement, 
                this._tempFilePath, 
                options);
        }

        /// <summary>
        /// The do compare.
        /// </summary>
        /// <param name="originalDocument">
        /// The original document.
        /// </param>
        /// <param name="changedDocument">
        /// The changed document.
        /// </param>
        /// <returns>
        /// The <see cref="BaseDiffResultObjectList"/>.
        /// </returns>
        public BaseDiffResultObjectList DoCompare(XmlDocument originalDocument, XmlDocument changedDocument)
        {
            return this.DoCompare(
                originalDocument.DocumentElement, 
                changedDocument.DocumentElement, 
                this._tempFilePath, 
                XmlDiffOptions.IgnoreChildOrder);
        }

        /// <summary>
        /// The do compare.
        /// </summary>
        /// <param name="originalDocument">
        /// The original document.
        /// </param>
        /// <param name="changedDocument">
        /// The changed document.
        /// </param>
        /// <param name="options">
        /// The options.
        /// </param>
        /// <returns>
        /// The <see cref="BaseDiffResultObjectList"/>.
        /// </returns>
        public BaseDiffResultObjectList DoCompare(
            XmlNode originalDocument, 
            XmlNode changedDocument, 
            XmlDiffOptions options)
        {
            return this.DoCompare(originalDocument, changedDocument, this._tempFilePath, options);
        }

        /// <summary>
        /// The do compare.
        /// </summary>
        /// <param name="originalDocument">
        /// The original document.
        /// </param>
        /// <param name="changedDocument">
        /// The changed document.
        /// </param>
        /// <param name="outputFilePath">
        /// The output file path.
        /// </param>
        /// <param name="options">
        /// The options.
        /// </param>
        /// <returns>
        /// The <see cref="BaseDiffResultObjectList"/>.
        /// </returns>
        public BaseDiffResultObjectList DoCompare(
            XmlNode originalDocument, 
            XmlNode changedDocument, 
            string outputFilePath, 
            XmlDiffOptions options)
        {
            Assert.StringIsNullOrEmpty(outputFilePath, "Output File Path is empty");
            var results = new BaseDiffResultObjectList();
            var stream = new StreamWriter(outputFilePath, false);
            var tw = new XmlTextWriter(stream);

            tw.Formatting = Formatting.Indented;
            this.SetDiffOptions(options);
            var isEqual = false;

            try
            {
                isEqual = this._diff.Compare(originalDocument, changedDocument, tw);
            }
            finally
            {
                tw.Close();
                stream.Close();
            }

            if (isEqual)
            {
                return results;
            }

            using (var diffGram = new XmlTextReader(outputFilePath))
            {
                var diffgramDoc = new XmlDocument();
                diffgramDoc.Load(diffGram);
                this.Manager.ApplyDiff(diffgramDoc.DocumentElement.FirstChild, originalDocument, ref results);
            }

            return results;
        }

        /// <summary>
        /// The get result file.
        /// </summary>
        /// <param name="changes">
        /// The changes.
        /// </param>
        /// <param name="originalNode">
        /// The original node.
        /// </param>
        /// <returns>
        /// The <see cref="XmlDocument"/>.
        /// </returns>
        public XmlDocument GetResultFile(BaseDiffResultObjectList changes, XmlNode originalNode)
        {
            Assert.ArgumentNotNull(originalNode, "Original Node can't be null");
            return this.Manager.ApplyChanges(changes, originalNode);
        }

        /// <summary>
        /// The get result file.
        /// </summary>
        /// <param name="changes">
        /// The changes.
        /// </param>
        /// <param name="originalNodePath">
        /// The original node path.
        /// </param>
        /// <returns>
        /// The <see cref="XmlDocument"/>.
        /// </returns>
        public XmlDocument GetResultFile(BaseDiffResultObjectList changes, string originalNodePath)
        {
            Assert.StringIsNullOrEmpty(originalNodePath, "Output File Path is empty");

            var originalDocumentReader = new XmlTextReader(new StreamReader(originalNodePath));

            var originalDocument = new XmlDocument();

            originalDocument.Load(originalDocumentReader);
            originalDocumentReader.Close();

            return this.GetResultFile(changes, originalDocument);
        }

        /// <summary>
        /// The get result file.
        /// </summary>
        /// <param name="changes">
        /// The changes.
        /// </param>
        /// <param name="originalNode">
        /// The original node.
        /// </param>
        /// <returns>
        /// The <see cref="XmlDocument"/>.
        /// </returns>
        public XmlDocument GetResultFile(BaseDiffResultObjectList changes, XmlDocument originalNode)
        {
            Assert.ArgumentNotNull(originalNode, "Original document can't be null");
            return this.GetResultFile(changes, originalNode.DocumentElement);
        }

        /// <summary>
        /// The set diff options.
        /// </summary>
        /// <param name="options">
        /// The options.
        /// </param>
        private void SetDiffOptions(XmlDiffOptions options)
        {
            this._diffOptions = options;
        }
    }
}