using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    using System.Data;
    using System.Dynamic;
    using System.IO;
    using System.IO.Compression;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.XPath;

    using ConsoleApplication1.Data;

    using CustomXMLDiff;
    using CustomXMLDiff.Core.Diagnostics;

    using Fasterflect;

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
            ExecuteCommandSync(@".\diff\DiffDogBatch.exe /cF " + f1 + " " + f2 + " /e /mX /dD /rX " + o);
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
            //// compare
            // DiffDataGenerator gen = new DiffDataGenerator(@"D:\test\Com.xml", @"D:\test1\Com.xml", "AUTOSAR.xsd");
            //var res = gen.GenerateDiffData();
            //DataStore.Save(res, "data.res");
            //var doc = XDocument.Load(
            //    @"D:\Systems\Autosar-Merger\ConsoleApplication1\bin\Debug\Config\Config\ECUC\IKE_BAC4.ecuc.arxml");
            //var doc2 = XDocument.Load(
            //    @"D:\Systems\Autosar-Merger\ConsoleApplication1\bin\Debug\Config1\Config\ECUC\IKE_BAC4.ecuc.arxml");


            var f1 =
                @"D:\Systems\Autosar-Merger\ConsoleApplication1\bin\Debug\Config\Config\Developer\ComponentTypes\SwcBc.arxml";
            var f2 =
                @"D:\Systems\Autosar-Merger\ConsoleApplication1\bin\Debug\Config1\Config\Developer\ComponentTypes\SwcBc.arxml";
            var f3 = "out.xml";
            var f4 = "out1.xml";

            File.Delete(f3);
            using (File.Create(f3))
            {
            }

            File.Delete(f4);

            File.Copy(f1, f4);

            CreateDiff(f1, f2, f3);

            var d = XDocument.Load(f3);
            string xPath;


            Console.WriteLine("Compare complete!");
            var originalDoc = XDocument.Load(f1).Root;
            var docNav = XDocument.Load(f4).Root;

            if (docNav != null && originalDoc != null)
            {
                foreach (var element in d.Root.Elements("xml_diff"))
                {
                    var leftContent = element.Element("left_content");
                    var rightContent = element.Element("right_content");

                    // case add, only right content
                    if (leftContent == null && rightContent != null)
                    {
                        var leftLocation = element.Element("left_location");
                        var rightLocation = element.Element("right_location");

                        if (leftLocation != null && rightLocation != null)
                        {
                            var leftParent = leftLocation.Element("parent");
                            var rightParent = rightLocation.Element("parent");

                            if (leftParent != null && rightParent != null)
                            {
                                var leftXPath = leftParent.Attribute("xpath");
                                var rightXPath = rightParent.Attribute("xpath");
                                var leftPosition = leftLocation.Element("position");
                                var rightPosition = rightLocation.Element("position");

                                if (leftXPath != null && rightXPath != null && leftPosition != null && rightPosition != null)
                                {
                                    var originalNode = originalDoc.XPathSelectElement(AddNsTOXPath(leftXPath.Value));                                    

                                    if (originalNode == null)
                                    {
                                        throw new Exception("Element cannot be found!");
                                    }
                                    
                                    var changeParentNode = docNav.XPathSelectElement(AddNsTOXPath(rightXPath.Value));

                                    if (changeParentNode == null)
                                    {
                                        //todo: search in right childs
                                        throw new Exception("Parent node deleted, element cannot be added!!!");
                                    }

                                    var rightContentData = rightContent.Element("element");

                                    if (rightContentData != null)
                                    {
                                        Console.WriteLine("Add");

                                        var position = rightLocation.Element("position");

                                        if (position != null)
                                        {
                                            var possitionInt = int.Parse(position.Value);
                                            var lastOrDefault = changeParentNode.Elements().Take(possitionInt - 1).LastOrDefault();

                                            if (lastOrDefault != null)
                                            {
                                                if (possitionInt == 2)
                                                {
                                                    // add as first child node
                                                    changeParentNode.AddFirst(rightContentData.Elements());
                                                }
                                                else
                                                {
                                                    // add in correspoding place
                                                    lastOrDefault.AddAfterSelf(rightContentData.Elements());
                                                }                                                
                                            }
                                            else
                                            {
                                                // add as first child node
                                                changeParentNode.AddFirst(rightContentData.Elements());
                                            }
                                        }
                                        else
                                        {
                                            throw new Exception("Position element is not correct!!!");
                                        }
                                    }
                                    else
                                    {
                                        throw new Exception("Right data is not ok!!!");
                                    }
                                }
                                else
                                {
                                    throw new Exception("Left or right xpath attribute is not ok!!!");
                                }
                            }
                            else
                            {
                                throw new Exception("Left or right parent is not ok!!!");
                            }
                        }
                        else
                        {
                            throw new Exception("Left or right location is not ok!!!");
                        }
                    }

                    // case delete
                    if (leftContent != null && rightContent == null)
                    {
                        var leftLocation = element.Element("left_location");
                        var rightLocation = element.Element("right_location");

                        if (leftLocation != null && rightLocation != null)
                        {
                            var leftParent = leftLocation.Element("parent");
                            var rightParent = rightLocation.Element("parent");

                            if (leftParent != null && rightParent != null)
                            {
                                var leftXPath = leftParent.Attribute("xpath");
                                var rightXPath = rightParent.Attribute("xpath");
                                var leftPosition = leftLocation.Element("position");
                                var rightPosition = rightLocation.Element("position");

                                if (leftXPath != null && rightXPath != null && leftPosition != null && rightPosition != null)
                                {
                                    var leftContentData = leftContent.Element("element");

                                    if (leftContentData != null)
                                    {
                                        var pathToDelete = docNav.XPathSelectElement(AddNsTOXPath(rightXPath.Value));

                                        if (pathToDelete == null)
                                        {
                                            throw new Exception("Path to delete element cannot be found!!!");
                                        }

                                        var elementToDelete = (XElement)leftContentData.FirstNode;

                                        // todo: remove element comparison
                                        var nodeToDelete =
                                            pathToDelete.Elements()
                                                .FirstOrDefault(x => x.Value == elementToDelete.Value);

                                        if (nodeToDelete == null)
                                        {
                                            throw new Exception("Element to delete cannot be found!!!");
                                        }

                                        Console.WriteLine("Remove");
                                        nodeToDelete.Remove();
                                    }
                                    else
                                    {
                                        throw new Exception("Left data is not ok!!");
                                    }
                                }
                                else
                                {
                                    throw new Exception("Right data is not ok!!!");
                                }
                            }
                            else
                            {
                                throw new Exception("Left or right xpath attribute is not ok!!!");
                            }
                        }
                        else
                        {
                            throw new Exception("Left or right parent is not ok!!!");
                        }
                    }

                    // case change
                    if (leftContent != null && rightContent != null)
                    {
                        var leftLocation = element.Element("left_location");
                        var rightLocation = element.Element("right_location");

                        if (leftLocation != null && rightLocation != null)
                        {
                            var leftParent = leftLocation.Element("parent");
                            var rightParent = rightLocation.Element("parent");

                            if (leftParent != null && rightParent != null)
                            {
                                var leftXPath = leftParent.Attribute("xpath");
                                var rightXPath = rightParent.Attribute("xpath");
                                var leftPosition = leftLocation.Element("position");
                                var rightPosition = rightLocation.Element("position");

                                if (leftXPath != null && rightXPath != null && leftPosition != null && rightPosition != null)
                                {
                                    var leftContentData = leftContent.Element("element");
                                    var rightContentData = rightContent.Element("element");
                                    var leftAttributeData = leftContent.Element("attribute");
                                    var rightAttributeData = rightContent.Element("attribute");

                                    if (leftContentData != null && rightContentData != null)
                                    {
                                        var elementToChange = docNav.XPathSelectElement(AddNsTOXPath(rightXPath.Value));

                                        if (elementToChange != null)
                                        {
                                            Console.WriteLine("Change element");
                                            // todo: change everything
                                            elementToChange.Value = rightContentData.Value;
                                        }
                                        else
                                        {
                                            // todo:
                                            throw new Exception("Element not found!!!");
                                        }
                                    }
                                    else if (leftAttributeData != null && rightAttributeData != null)
                                    {
                                        var elementToChange = docNav.XPathSelectElement(AddNsTOXPath(rightXPath.Value));

                                        if (elementToChange != null)
                                        {
                                            Console.WriteLine("Change attribute");

                                            var changedAttributes = rightAttributeData.Attributes();
                                            elementToChange.RemoveAttributes();
                                            elementToChange.Add(changedAttributes);
                                        }
                                        else
                                        {
                                            throw new Exception("Element not found!!!");
                                        }
                                    }
                                    else
                                    {
                                        throw new Exception("Data is not ok!!!");
                                    }
                                }
                                else
                                {
                                    throw new Exception("Right data is not ok!!!");
                                }
                            }
                            else
                            {
                                throw new Exception("Left or right xpath attribute is not ok!!!");
                            }
                        }
                        else
                        {
                            throw new Exception("Left or right parent is not ok!!!");
                        }
                    }
                }
            }

            docNav.Save(f4);



            //// //// merge
            //var result = DataStore.Load<List<DiffDataElement>>("data.res");            
            //DiffDataReader reader = new DiffDataReader(result);
            //File.Delete(@"d:\a1.arxml");
            //File.Copy(@"D:\Systems\Autosar-Merger\ConsoleApplication1\bin\Debug\config\Config\Developer\ComponentTypes\SwcBc.arxml", @"d:\a1.arxml");
            //reader.ApplyDifferencesToFile(@"d:\a1.arxml");

            // Directory.Delete("config", true);
            // ZipData.UnZip("config.zip", "c");
            //    ZipData.UnZip("config1.zip", "result");
            //    ZipData.UnZip("config1.zip", "c1");
            //string[] array1 = Directory.GetFiles(Directory.GetCurrentDirectory() + "\\c", "*.arxml", SearchOption.AllDirectories);
            //string[] array2 = Directory.GetFiles(Directory.GetCurrentDirectory() + "\\c1", "*.arxml", SearchOption.AllDirectories);
            //string[] array3 = Directory.GetFiles(Directory.GetCurrentDirectory() + "\\result", "*.arxml", SearchOption.AllDirectories);

            ////   // ////DeleteDirectory(Directory.GetCurrentDirectory() + "\\result", true);



            //for (int i = 0; i < array1.Length; i++)
            //{
            //    if (FileCompare(array1[i], array2[i]) == false)
            //    {
            //        Console.WriteLine(array3[i]);

            //        File.Delete(array3[i]);
            //        using (File.Create(array3[i]))
            //        {
            //        }

            //        CreateDiff(array1[i], array2[i], array3[i]);
            //    }
            //}

            //        DataStore.Save(res1, array3[i]);
            //    }
            //}               
            //    }

            //for (int i = 0; i < array1.Length; i++)
            //{
            //    if (FileCompare(array1[i], array2[i]) == false)
            //    {
            //        var d = XDocument.Load(array3[i]);
            //        string xPath;


            //        Console.WriteLine("Compare complete!");
            //        var docNav = XDocument.Load(array1[i]).Root;

            //        foreach (var element in d.Root.Elements("xml_diff"))
            //        {
            //            // case add
            //            if (element.Element("left_content") == null && element.Element("right_content") != null)
            //            {

            //                xPath = AddNsTOXPath(element.Element("left_location").Element("parent").Attribute("xpath").Value);
            //                var node1 = docNav.XPathSelectElement(xPath);

            //                if (node1 != null && element.Element("right_content").Element("element") != null)
            //                {
            //                    Console.WriteLine("Add");
            //                    //    node1.Add(element.Element("right_content").Element("element").FirstNode);

            //                    try
            //                    {
            //                        node1.Elements().Take(int.Parse(element.Element("left_location").Element("position").Value) - 1).LastOrDefault().AddAfterSelf(element.Element("right_content").Element("element").FirstNode);
            //                    }
            //                    catch (Exception)
            //                    {

            //                    }

            //                }
            //            }

            //            // case delete
            //            if (element.Element("left_content") != null && element.Element("right_content") == null)
            //            {

            //                xPath = AddNsTOXPath(element.Element("left_location").Element("parent").Attribute("xpath").Value);
            //                var node1 = docNav.XPathSelectElement(xPath);

            //                if (node1 != null && element.Element("left_content").Element("element") != null)
            //                {
            //                    Console.WriteLine("Delete");
            //                    var nodeToDelete = element.Element("left_content").Element("element").FirstNode;
            //                    var nodedel = node1.Elements().Where(x => XNode.DeepEquals(x, nodeToDelete)).FirstOrDefault();
            //                    try
            //                    {
            //                        nodedel.Remove();
            //                    }
            //                    catch (Exception)
            //                    {

            //                        //throw;
            //                    }

            //                }
            //            }

            //            // case change
            //            if (element.Element("left_content") != null && element.Element("right_content") != null)
            //            {

            //                xPath = AddNsTOXPath(element.Element("left_location").Element("parent").Attribute("xpath").Value);
            //                var node1 = docNav.XPathSelectElement(xPath);

            //                if (node1 != null && element.Element("right_content").Element("element") != null)
            //                {
            //                    Console.WriteLine("Change element");
            //                    var changedNode = element.Element("right_content").Element("element").Value;
            //                    node1.Value = changedNode;
            //                }

            //                if (node1 != null && element.Element("right_content").Element("attribute") != null)
            //                {
            //                    Console.WriteLine("Change attribute");
            //                    var changedAttributes = element.Element("right_content").Element("attribute").Attributes();
            //                    node1.RemoveAttributes();
            //                    node1.Add(changedAttributes);
            //                }

            //                //var nodedel = node1.Elements().Where(x => XNode.DeepEquals(x, nodeToDelete)).FirstOrDefault();
            //                //nodedel.Remove();
            //            }



            //            //nav = docNav.CreateNavigator();
            //            //xPath = element.Element("left_location").Element("parent").Attribute("xpath").Value;
            //            //string value = nav.SelectSingleNode(xPath).Value;

            //            //   Console.WriteLine(value);
            //        }

            //        docNav.Save(array1[i]);
            //    }
            //}
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
