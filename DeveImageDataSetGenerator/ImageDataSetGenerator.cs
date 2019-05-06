using DeveImageDataSetGenerator.DataInputOutput;
using DeveImageDataSetGenerator.ImageProcessing;
using System.IO;
using System.Linq;

namespace DeveImageDataSetGenerator
{
    public class ImageDataSetGenerator
    {
        private readonly string _inputAnnotationsFile;
        private readonly string _outputDirImages;
        private readonly string _outputDirAnnoations;
        private readonly ImageRotationProcessor _imageProcessor;
        private readonly string _dirOfAnnotationsFile;

        public ImageDataSetGenerator(string inputAnnotationsFile, string outputDirImages, string outputDirAnnoations)
        {
            _inputAnnotationsFile = inputAnnotationsFile;
            _outputDirImages = outputDirImages;
            _outputDirAnnoations = outputDirAnnoations;

            _imageProcessor = new ImageRotationProcessor(_outputDirImages, 30, 90, 150, 230);
            _dirOfAnnotationsFile = Path.GetDirectoryName(_inputAnnotationsFile);
        }

        public void GoProcess()
        {
            var allTags = AnnotationsReader.ReadAnnotationsFromFileAsync(_inputAnnotationsFile);

            var groupedTagsPerImage = allTags.GroupBy(t => t.ImagePath);

            foreach (var imageWithTags in groupedTagsPerImage)
            {

                var imagePath = Path.Combine(_dirOfAnnotationsFile, imageWithTags.Key);
                var imagePathResolved = Path.GetFullPath(imagePath);
                _imageProcessor.ProcessImage(imagePathResolved, imageWithTags);
            }
        }
    }
}
