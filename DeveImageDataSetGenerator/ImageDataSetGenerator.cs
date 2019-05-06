using DeveImageDataSetGenerator.DataInputOutput;
using DeveImageDataSetGenerator.ImageProcessing;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DeveImageDataSetGenerator
{
    public class ImageDataSetGenerator
    {
        private readonly string _inputAnnotationsFile;
        private readonly string _outputDirImages;
        private readonly string _outputAnnotationsFile;
        private readonly ImageRotationProcessor _imageProcessor;
        private readonly string _dirOfAnnotationsFile;

        public ImageDataSetGenerator(string inputAnnotationsFile, string outputDirImages, string outputAnnotationsFile)
        {
            _inputAnnotationsFile = inputAnnotationsFile;
            _outputDirImages = outputDirImages;
            _outputAnnotationsFile = outputAnnotationsFile;

            _imageProcessor = new ImageRotationProcessor(_outputDirImages, _outputAnnotationsFile, 30, 90, 150, 230);
            _dirOfAnnotationsFile = Path.GetDirectoryName(_inputAnnotationsFile);
        }

        public void GoProcess()
        {
            var allTags = AnnotationsReaderWriter.ReadAnnotationsFromFile(_inputAnnotationsFile);

            var groupedTagsPerImage = allTags.GroupBy(t => t.ImagePath);

            var allAnnotations = new List<Annotations>();

            var outputAnnotationsDir = Path.GetDirectoryName(_outputAnnotationsFile);

            foreach (var imageWithTags in groupedTagsPerImage)
            {
                var imagePath = Path.Combine(_dirOfAnnotationsFile, imageWithTags.Key);
                var imagePathResolved = Path.GetFullPath(imagePath);

                var outputPath = Path.Combine(_outputDirImages, Path.GetFileName(imagePathResolved));
                File.Copy(imagePathResolved, outputPath, true);

                foreach (var existingTag in imageWithTags)
                {
                    var newTag = new Annotations()
                    {
                        Tag = existingTag.Tag,
                        ImagePath = Path.GetRelativePath(outputAnnotationsDir, outputPath),
                        X1 = existingTag.X1,
                        X2 = existingTag.X2,
                        Y1 = existingTag.Y1,
                        Y2 = existingTag.Y2
                    };
                }

                var newAnnotations = _imageProcessor.ProcessImage(imagePathResolved, imageWithTags);
                allAnnotations.AddRange(newAnnotations);
            }

            AnnotationsReaderWriter.WriteAnnotationsToFile(_outputAnnotationsFile, allAnnotations);
        }
    }
}
