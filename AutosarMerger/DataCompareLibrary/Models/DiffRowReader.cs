
namespace DataCompareLibrary.Models
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;

    public static class DiffRowReader
    {

        private static List<DiffRow> rows;

        public static List<DiffRow> Read(string fileName)
        {
            rows = new List<DiffRow>();

            // todo: input validation

            string value1 = File.ReadAllText(fileName);
            var tags = value1.Split('<');

            foreach (var t in tags)
            {
                var r = ParseLine(t);
                if (r.Type != RowType.Unknown)
                {
                    rows.Add(r);
                }
            }

            return rows;
        }

        private static DiffRow ParseLine(string line)
        {
            DiffRow row = new DiffRow()
                              {
                                  Type = RowType.Unknown
                              };

            // normalize line
            line = line.Trim();
            //line = line.Replace("<", string.Empty);
            line = line.Replace(">", string.Empty);
            line = line.Replace("match=", " ");
            line = line.Replace("match=", string.Empty);
            line = line.Replace("\"", " ");

            var data = line.Split(' ');
            data = data.Where(x => !string.IsNullOrEmpty(x)).ToArray();

            if (data.Length > 0 && !line.Contains("opid"))
            {
                if (data[0].StartsWith("xd:node"))
                {
                    row.Type = RowType.JumpRight;
                    row.Offset = int.Parse(data[1]);
                }
                else if (data[0].StartsWith("/xd:change"))
                {
                    row.Type = RowType.JumpLeft;
                    row.Offset = 1;
                }
                else if (data[0].StartsWith("xd:change") && !data[1].Contains("@"))
                {
                    row.Type = RowType.ChangeNode;
                    row.Offset = int.Parse(data[1]);
                    row.Data = data[2];
                }
                else if (data[0].StartsWith("xd:change") && data[1].Contains("@"))
                {
                    row.Type = RowType.ChangeAttribute;
                    row.Name = data[1].Substring(1, data[1].Length - 1);
                    row.Data = data[2];
                }
            }

            return row;
        }
    }
}
