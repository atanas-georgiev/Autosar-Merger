// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DiffDataElement.cs" company="">
//   
// </copyright>
// <summary>
//   The diff data element.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ConsoleApplication1.Models
{
    using System;
    using System.Xml.Linq;

    /// <summary>
    /// The diff data element.
    /// </summary>
    [Serializable]
    public class DiffDataElement
    {
        /// <summary>
        /// Gets or sets the action.
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// Gets or sets the full x path.
        /// </summary>
        public string FullXPath { get; set; }

        /// <summary>
        /// Gets or sets the element.
        /// </summary>
        public XElement Element { get; set; }

        /// <summary>
        /// Gets or sets the changed element.
        /// </summary>
        public XElement ChangedElement { get; set; }

        public string ToBeDeleted { get; set; }
    }
}