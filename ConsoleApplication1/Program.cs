using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    using System.IO;
    using System.IO.Compression;
    using System.Xml.Linq;

    using ConsoleApplication1.Data;
    using ConsoleApplication1.Models;
    using ConsoleApplication1.XmlCompare;    

    static class Program
    {
        private static List<DiffDataElement> data;

        private static XDocument doc1;

        private static XDocument doc2;


        private static void Main(string[] args)
        {
            //// compare
            DiffDataGenerator gen = new DiffDataGenerator(@"D:\a.arxml", @"D:\b.arxml", "AUTOSAR.xsd");
            var res = gen.GenerateDiffData();
            DataStore.Save(res, "data.res");

            //// merge
           //var result = DataStore.Load<List<DiffDataElement>>("data.res");            
           //DiffDataReader reader = new DiffDataReader(result);
           //reader.ApplyDifferencesToFile(@"d:\a1.arxml");

            //Directory.Delete("config", true);
            //ZipData.UnZip("config.zip", "config");
            //ZipData.UnZip("config.zip", "result");
            //ZipData.UnZip("config1.zip", "config1");
            //string[] array1 = Directory.GetFiles(Directory.GetCurrentDirectory() + "\\config", "*.arxml", SearchOption.AllDirectories);
            //string[] array2 = Directory.GetFiles(Directory.GetCurrentDirectory() + "\\config1", "*.arxml", SearchOption.AllDirectories);
            //string[] array3 = Directory.GetFiles(Directory.GetCurrentDirectory() + "\\result", "*.arxml", SearchOption.AllDirectories);

            ////DeleteDirectory(Directory.GetCurrentDirectory() + "\\result", true);

            //for (int i = 0; i < array1.Length; i++)
            //{
            //    if (FileCompare(array1[i], array2[i]) == false)
            //    {
            //        Console.WriteLine(array3[i]);
            //        DiffDataGenerator gen = new DiffDataGenerator(array1[i], array2[i], "AUTOSAR.xsd");
            //        var res = gen.GenerateDiffData();
            //        File.Delete(array3[i]);
            //        using (File.Create(array3[i]));
            //        DataStore.Save(res, array3[i]);                    
            //    }               
            //}

            //for (int i = 0; i < array1.Length; i++)
            //{
            //    if (FileCompare(array1[i], array2[i]) == false)
            //    {
            //        Console.WriteLine("Patching ... "  + array3[i]);
            //        var result = DataStore.Load<List<DiffDataElement>>(array3[i]);            
            //        DiffDataReader reader = new DiffDataReader(result);                    
            //        reader.ApplyDifferencesToFile(array1[i]);                
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
