using CsvHelper;
using CsvHelper.Configuration;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace DeveImageDataSetGenerator.DataInputOutput
{
    public static class AnnotationsReaderWriter
    {
        public static List<Annotations> ReadAnnotationsFromFile(string filePath)
        {
            var config = new Configuration()
            {
                HasHeaderRecord = false,
                CultureInfo = CultureInfo.InvariantCulture,
                Delimiter = ","
            };

            using (var reader = new StreamReader(filePath))
            {
                using (var csv = new CsvReader(reader, config))
                {
                    var records = csv.GetRecords<Annotations>().ToList();
                    return records;
                }
            }
        }

        public static void WriteAnnotationsToFile(string filePath, List<Annotations> annotations)
        {
            var config = new Configuration()
            {
                HasHeaderRecord = false,
                CultureInfo = CultureInfo.InvariantCulture,
                Delimiter = ","
            };

            using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                using (var writer = new StreamWriter(stream))
                {
                    using (var csv = new CsvWriter(writer, config))
                    {
                        csv.WriteRecords(annotations);
                    }
                }
            }
        }
    }
}
