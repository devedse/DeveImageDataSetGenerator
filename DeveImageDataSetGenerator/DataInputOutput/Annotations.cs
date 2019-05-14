using CsvHelper.Configuration.Attributes;

namespace DeveImageDataSetGenerator.DataInputOutput
{
    public class Annotations
    {
        [Index(0)]
        public string ImagePath { get; set; }

        [Index(1)]
        public int? X1 { get; set; }

        [Index(2)]
        public int? Y1 { get; set; }

        [Index(3)]
        public int? X2 { get; set; }

        [Index(4)]
        public int? Y2 { get; set; }

        [Index(5)]
        public string Tag { get; set; }

        [Ignore]
        public bool IsValid => X1 != null && X2 != null && Y1 != null && Y2 != null && !string.IsNullOrWhiteSpace(Tag);
    }
}
