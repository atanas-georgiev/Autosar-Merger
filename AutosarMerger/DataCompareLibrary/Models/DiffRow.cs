namespace DataCompareLibrary.Models
{
    using System.Xml.Linq;

    public class DiffRow
    {
        public RowType Type { get; set; }

        public int Offset { get; set; }

        public string Data { get; set; }
        public string Name { get; set; }
    }
}
