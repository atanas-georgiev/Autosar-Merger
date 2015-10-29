namespace ConsoleApplication1.Data
{
    using System.IO.Compression;

    static class ZipData
    {
        public static void UnZip(string file, string folder)
        {
          //  ZipFile.CreateFromDirectory(startPath, zipPath);
            ZipFile.ExtractToDirectory(file, folder);
        }
    }
}
