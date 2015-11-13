namespace AutosarData
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class AutosarFileDiffCollection
    {
        private readonly AutosarFileCollection collection1;
        private readonly AutosarFileCollection collection2;
        private readonly string Folder;

        private HashSet<FileDiffData> diffCollection;

        public AutosarFileDiffCollection(AutosarFileCollection collection1, AutosarFileCollection collection2, string folder)
        {
            this.collection1 = collection1;
            this.collection2 = collection2;
            this.Folder = folder;

            if (Directory.Exists(folder))
            {
                this.Empty(new DirectoryInfo(folder));
            }

            Directory.CreateDirectory(folder);

            this.CreateDiffCollection();
        }

        private void Empty(DirectoryInfo directory)
        {
            foreach (System.IO.FileInfo file in directory.GetFiles()) file.Delete();
            foreach (System.IO.DirectoryInfo subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
        }

        private void CreateDiffCollection()
        {
            this.diffCollection = new HashSet<FileDiffData>();
            var col1 = this.collection1.GetAllFiles();
            var col2 = this.collection2.GetAllFiles();

            foreach (var file in col1)
            {
                var file2 =
                    col2.First(x => (x.RelativePath + " " + x.FileName) == (file.RelativePath + " " + file.FileName));

                if (file2 != null)
                {
                    var fullFile1 = this.collection1.Directory.FullName + "\\" + file.RelativePath + "\\" + file.FileName;
                    var fullFile2 = this.collection2.Directory.FullName + "\\" + file2.RelativePath + "\\" + file2.FileName;
                    if (File.Exists(fullFile1) && File.Exists(fullFile2) && FileCompare(fullFile1, fullFile2) == false)
                    {
                        this.diffCollection.Add(
                            new FileDiffData()
                                {
                                    FirstFile = fullFile1,
                                    SecondFile = fullFile2,
                                    DiffOriginalFile = this.Folder + "\\" + file.RelativePath + "\\" + file.FileName,
                                    DiffFile =
                                        this.Folder + "\\" + file.RelativePath + "\\" + file.FileName + "_diff"
                                });

                        if (!Directory.Exists(this.Folder + "\\" + file.RelativePath))
                        {
                            Directory.CreateDirectory(this.Folder + "\\" + file.RelativePath);
                        }
                    }
                }
            }
        }

        public IEnumerable<FileDiffData> GetAllDifferences()
        {
            return this.diffCollection;
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

