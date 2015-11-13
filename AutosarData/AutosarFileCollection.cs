namespace AutosarData
{
    using System.Collections.Generic;
    using System.IO;

    public class AutosarFileCollection
    {
        private const string AsFilePattern = "*.arxml";

        private HashSet<AutosarFile> files;

        private DirectoryInfo folder;

        public AutosarFileCollection()
        {
            this.files = new HashSet<AutosarFile>();
        }

        public void ParseFiles(DirectoryInfo input)
        {
            this.folder = input;

            if (!input.Exists)
            {
                throw new DirectoryNotFoundException(input + "not found!");
            }

            foreach (var file in this.folder.GetFiles(AsFilePattern, SearchOption.AllDirectories))
            {
                var fileToAdd = new AutosarFile();
                fileToAdd.FileName = file.Name;
                fileToAdd.RelativePath = file.DirectoryName.Replace(this.folder.FullName, string.Empty);

                this.files.Add(fileToAdd);
            }
        }

        public DirectoryInfo Directory
        {
            get
            {
                return this.folder;
            }
        }

        public void Add(AutosarFile file)
        {
            this.files.Add(file);
        }

        public IEnumerable<AutosarFile> GetAllFiles()
        {
            return this.files;
        }
    }
}
