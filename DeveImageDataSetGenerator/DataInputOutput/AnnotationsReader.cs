using CsvHelper;
using CsvHelper.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DeveImageDataSetGenerator.DataInputOutput
{
    public static class AnnotationsReader
    {
        public static List<Annotations> ReadAnnotationsFromFileAsync(string filePath)
        {
            var config = new Configuration()
            {
                HasHeaderRecord = false
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
    }
}
