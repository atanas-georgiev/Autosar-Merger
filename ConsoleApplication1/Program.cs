using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    using System.CodeDom.Compiler;
    using System.Data;
    using System.Dynamic;
    using System.IO;
    using System.IO.Compression;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.XPath;

    using AutosarData;

    using ConsoleApplication1.Data;

    using XmlSpecificationCompare.XPathDiscovery;

    static class Program
    {

        private static void ExecuteCommandSync(object command)
        {
            try
            {
                // create the ProcessStartInfo using "cmd" as the program to be run,
                // and "/c " as the parameters.
                // Incidentally, /c tells cmd that we want it to execute the command that follows,
                // and then exit.
                System.Diagnostics.ProcessStartInfo procStartInfo =
                    new System.Diagnostics.ProcessStartInfo("cmd", "/c " + command);

                // The following commands are needed to redirect the standard output.
                // This means that it will be redirected to the Process.StandardOutput StreamReader.
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.UseShellExecute = false;
                // Do not create the black window.
                procStartInfo.CreateNoWindow = true;
                // Now we create a process, assign its ProcessStartInfo and start it
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo = procStartInfo;
                proc.Start();
                // Get the output into a string
                string result = proc.StandardOutput.ReadToEnd();
                // Display the command output.
                Console.WriteLine(result);
            }
            catch (Exception objException)
            {
                // Log the exception
            }
        }

        private static void CreateDiff(string f1, string f2, string o)
        {
            ExecuteCommandSync(@".\diff\xmldiff.exe diff " + f1 + " " + f2 + " " + o + " --keep-diff-only no --tag-childs no --diff-ns no");
        }

        private static void ApplyDiff(string originalFile, string diffFile, string targetFile)
        {
            var d = XDocument.Load(diffFile);

            var modifiedElements = d.Descendants().Where(x => x.Attributes().Any(y => y.Name.LocalName == "status"));

            Console.WriteLine("Compare complete!");
            var originalDoc = XDocument.Load(originalFile).Root;
            var changedDoc = XDocument.Load(targetFile).Root;
            var elementsToDelete = new List<XElement>();

            if (originalDoc != null)
            {
                foreach (var e in modifiedElements)
                {
                    if (e.Attribute("status").Value == "modified")
                    {
                        // modify
                        var el = FindCorrespondingElement(e, originalDoc, changedDoc);
                        ApplyChangedData(e, el);
                    }
                    else if (e.Attribute("status").Value == "removed")
                    {
                        // remove
                        var el = FindCorrespondingElement(e, originalDoc, changedDoc);
                        elementsToDelete.Add(el);
                    }
                    else if (e.Attribute("status").Value == "added")
                    {
                        // add                        
                        var position = e.Position();
                        var pathToParent = e.Parent.AbsoluteXPath();
                        var foundParent = changedDoc.XPathSelectElement(pathToParent);
                        e.Attribute("status").Remove();
                        if (position == 0)
                        {
                            foundParent.AddFirst(e);
                        }
                        else
                        {
                            int pos = 0;

                            foreach (var child in foundParent.Elements())
                            {
                                if (pos == position - 1)
                                {
                                    child.AddAfterSelf(e);
                                    break;
                                }

                                pos++;
                            }
                        }
                    }
                }
            }

            foreach (var del in elementsToDelete)
            {
                del.Remove();
            }

            changedDoc.Save(targetFile);
        }

        private static string AddNsTOXPath(string path)
        {
            var result = path.Split('/').Where(x => !string.IsNullOrEmpty(x)).ToArray();
            for (int i = 0; i < result.Length; i++)
            {
                if (!result[i].Contains("["))
                {
                    result[i] = "*[local-name()='" + result[i] + "']";
                }
                else
                {
                    var result2 = result[i].Split('[');
                    result[i] = "*[local-name()='" + result2[0] + "'][" + result2[1];
                }
            }
            var res = string.Join("/", result);
            return "/" + res;
        }

        private static void Main(string[] args)
        {

            AutosarFileCollection collection = new AutosarFileCollection();
            collection.ParseFiles(new DirectoryInfo(Directory.GetCurrentDirectory() + "\\c"));

            AutosarFileCollection collection2 = new AutosarFileCollection();
            collection2.ParseFiles(new DirectoryInfo(Directory.GetCurrentDirectory() + "\\c1"));

            AutosarFileDiffCollection diff = new AutosarFileDiffCollection(collection, collection2, "temp");

            foreach (var diffEntry in diff.GetAllDifferences())
            {
                CreateDiff(diffEntry.FirstFile, diffEntry.SecondFile, diffEntry.DiffFile);
                File.Copy(diffEntry.FirstFile, diffEntry.DiffOriginalFile);
            }

            //var f1 =
            //    @"D:\Systems\Autosar-Merger\ConsoleApplication1\bin\Debug\c1\Config\Developer\ComponentTypes\SwcTnk.arxml";
            //var f2 = @"D:\Systems\Autosar-Merger\ConsoleApplication1\bin\Debug\c\Config\Developer\ComponentTypes\SwcTnk.arxml";
            //var f3 = "out1.xml";

            //CreateDiff(f1, f2, f3);
            //File.Copy(f1, "out1111.xml");
            //ApplyDiff(f1, f3, "out1111.xml");

            //ZipData.UnZip("config_p1.zip", "c");            
            //ZipData.UnZip("config_p2.zip", "c1");
            //ZipData.UnZip("config_p2.zip", "result");

            //string[] array1 = Directory.GetFiles(Directory.GetCurrentDirectory() + "\\c", "*.arxml", SearchOption.AllDirectories);
            //string[] array2 = Directory.GetFiles(Directory.GetCurrentDirectory() + "\\c1", "*.arxml", SearchOption.AllDirectories);
            //string[] arrayResult = Directory.GetFiles(Directory.GetCurrentDirectory() + "\\result", "*.arxml", SearchOption.AllDirectories);

            //for (int i = 0; i < array1.Length; i++)
            //{
            //    if (FileCompare(array1[i], array2[i]) == false)
            //    {
            //        //var originalDoc = XDocument.Load(array1[i]);
            //        //var changedDoc = XDocument.Load(array2[i]);

            //        //File.Delete(arrayResult[i]);
            //        //using (File.Create(arrayResult[i]))
            //        //{
            //        //}
            //        Console.WriteLine("Comparing {0} and {1}", array1[i], array2[i]);
            //        CreateDiff(array1[i], array2[i], arrayResult[i]);

            //        //ApplyDiff(array1[i], arrayResult[i], array2[i]);
            //        ////var resultDoc = 
            //    }
            //}

            //for (int i = 0; i < array1.Length; i++)
            //{
            //    if (FileCompare(array1[i], array2[i]) == false)
            //    {
            //        try
            //        {
            //          Console.WriteLine("Diff {0} and {1}", array1[i], array2[i]);                      
            //          ApplyDiff(array1[i], arrayResult[i], array1[i]);
            //        }
            //        catch (Exception)
            //        {
            //            Console.WriteLine("ERROR!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");                        
            //        }

            //    }
            //}

            /*

                    */
        }

        private static void ApplyChangedData(XElement change, XElement element)
        {
            // change attributes
            var attributes = change.Attributes().Where(x => x.Name != "status");
            foreach (var changedAttr in attributes)
            {
                if (changedAttr.Value.Contains('|'))
                {
                    var oldNew = changedAttr.Value.Split('|');
                    element.Attribute(changedAttr.Name).Value = oldNew[1];
                }
            }

            if (!change.HasElements && change.Value != string.Empty && change.Value.Contains('|'))
            {
                var oldNew = change.Value.Split('|');
                element.Value = oldNew[1];
            }
        }

        private static XElement FindCorrespondingElement(XElement element, XElement originalDoc, XElement changedDoc)
        {
            var originalElement = originalDoc.XPathSelectElement(element.AbsoluteXPath());
            if (originalElement == null)
            {
                throw new NullReferenceException("value");
            }

            var valuePath = originalElement.AbsoluteXPath();
            var foundElement = changedDoc.XPathSelectElement(valuePath);

            if (foundElement != null && XNode.DeepEquals(originalElement, foundElement))
            {
                return foundElement;
            }
            else
            {
              //  var pathToParent = originalElement.Parent.AbsoluteXPath();
                //var foundParent = changedDoc.XPathSelectElement(pathToParent);

                var elements = changedDoc.Descendants().Where(x => XNode.DeepEquals(x, originalElement) && x.Value == originalElement.Value);

                if (elements != null && elements.Count() == 1)
                {
                    return elements.First();
                }
                else
                {
                    throw new Exception("Not found element!");
                }
            }
           
           throw new Exception("Not found element!");            
        }


        public static void DeleteDirectory(string path, bool recursive)
        {
            // Delete all files and sub-folders?
            if (recursive)
            {
                // Yep... Let's do this
                var subfolders = Directory.GetDirectories(path);
                foreach (var s in subfolders)
                {
                    DeleteDirectory(s, recursive);
                }
            }

            // Get all files of the folder
            var files = Directory.GetFiles(path);
            foreach (var f in files)
            {
                // Get the attributes of the file
                var attr = File.GetAttributes(f);

                // Is this file marked as 'read-only'?
                if ((attr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    // Yes... Remove the 'read-only' attribute, then
                    File.SetAttributes(f, attr ^ FileAttributes.ReadOnly);
                }

                // Delete the file
                File.Delete(f);
            }

            // When we get here, all the files of the folder were
            // already deleted, so we just delete the empty folder
            Directory.Delete(path);
        }

        private static bool FileCompare(string file1, string file2)
        {
            int file1byte;
            int file2byte;
            FileStream fs1;
            FileStream fs2;

            // Determine if the same file was referenced two times.
            if (file1 == file2)
            {
                // Return true to indicate that the files are the same.
                return true;
            }

            // Open the two files.
            fs1 = new FileStream(file1, FileMode.Open, FileAccess.Read);
            fs2 = new FileStream(file2, FileMode.Open, FileAccess.Read);

            // Check the file sizes. If they are not the same, the files 
            // are not the same.
            if (fs1.Length != fs2.Length)
            {
                // Close the file
                fs1.Close();
                fs2.Close();

                // Return false to indicate files are different
                return false;
            }

            // Read and compare a byte from each file until either a
            // non-matching set of bytes is found or until the end of
            // file1 is reached.
            do
            {
                // Read one byte from each file.
                file1byte = fs1.ReadByte();
                file2byte = fs2.ReadByte();
            }
            while ((file1byte == file2byte) && (file1byte != -1));

            // Close the files.
            fs1.Close();
            fs2.Close();

            // Return the success of the comparison. "file1byte" is 
            // equal to "file2byte" at this point only if the files are 
            // the same.
            return ((file1byte - file2byte) == 0);
        }

    }
}
