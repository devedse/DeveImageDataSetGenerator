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

            var allImages = allTags.GroupBy(t => t.ImagePath).ToList();

            var allAnnotations = new List<Annotations>();

            var imagesWithCarsOnThem = allImages.Where(t => t.All(z => z.IsValid)).ToList();
            var maxTags = imagesWithCarsOnThem.Max(t => t.Count());
            var allImagesWithMaxAmountOfCarsOnThem = imagesWithCarsOnThem.Where(t => t.Count() == maxTags).ToList();
            var imagesWithNoCarsOnThem = allImages.Where(t => t.All(z => !z.IsValid)).ToList();

            allAnnotations.AddRange(GoCopyTheseImages(allImages));
            allAnnotations.AddRange(GoRotateTheseImages(imagesWithCarsOnThem));

            AnnotationsReaderWriter.WriteAnnotationsToFile(_outputAnnotationsFile, allAnnotations);
        }

        private List<Annotations> GoCopyTheseImages(List<IGrouping<string, Annotations>> groupedTagsPerImage)
        {
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

                    allAnnotations.Add(newTag);
                }
            }

            return allAnnotations;
        }

        private List<Annotations> GoRotateTheseImages(List<IGrouping<string, Annotations>> groupedTagsPerImage)
        {
            var allAnnotations = new List<Annotations>();

            foreach (var imageWithTags in groupedTagsPerImage)
            {
                var imagePath = Path.Combine(_dirOfAnnotationsFile, imageWithTags.Key);
                var imagePathResolved = Path.GetFullPath(imagePath);

                var newAnnotations = _imageProcessor.ProcessImage(imagePathResolved, imageWithTags);
                allAnnotations.AddRange(newAnnotations);
            }

            return allAnnotations;
        }
    }
}
