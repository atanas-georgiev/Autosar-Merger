using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    using System;
    using System.IO;
    using System.Xml;
    using System.Xml.Serialization;

    /// <summary>
    ///     The game data.
    /// </summary>
    public static class DataStore
    {        
        /// <summary>
        /// The player data save.
        /// </summary>
        /// <param name="serializableObject">
        /// The serializable object.
        /// </param>
        /// <param name="fileName">
        /// The file name.
        /// </param>
        /// <typeparam name="T">
        ///  The player data.
        /// </typeparam>
        public static void Save<T>(T serializableObject, string fileName)
        {
            if (serializableObject == null)
            {
                return;
            }

            try
            {
                var xmlDocument = new XmlDocument();
                var serializer = new XmlSerializer(serializableObject.GetType());
                using (var stream = new MemoryStream())
                {
                    serializer.Serialize(stream, serializableObject);
                    stream.Position = 0;
                    xmlDocument.Load(stream);
                    xmlDocument.Save(fileName);
                    stream.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// The player data load.
        /// </summary>
        /// <param name="fileName">
        /// The file name.
        /// </param>
        /// <typeparam name="T">
        ///     The player data.
        /// </typeparam>
        /// <returns>
        /// The player data <see cref="T"/>.
        /// </returns>
        public static T Load<T>(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return default(T);
            }

            var objectOut = default(T);

            try
            {
                var attributeXml = string.Empty;

                var xmlDocument = new XmlDocument();
                xmlDocument.Load(fileName);
                var xmlString = xmlDocument.OuterXml;

                using (var read = new StringReader(xmlString))
                {
                    var outType = typeof(T);

                    var serializer = new XmlSerializer(outType);
                    using (XmlReader reader = new XmlTextReader(read))
                    {
                        objectOut = (T)serializer.Deserialize(reader);
                        reader.Close();
                    }

                    read.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return objectOut;
        }
    }
}
