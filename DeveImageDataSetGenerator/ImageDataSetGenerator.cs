using DeveImageDataSetGenerator.DataInputOutput;
using DeveImageDataSetGenerator.ImageProcessing;
using System.Linq;

namespace DeveImageDataSetGenerator
{
    public class ImageDataSetGenerator
    {
        private readonly string _inputAnnotationsFile;
        private readonly string _outputDirImages;
        private readonly string _outputDirAnnoations;
        private readonly ImageProcessor _imageProcessor;

        public ImageDataSetGenerator(string inputAnnotationsFile, string outputDirImages, string outputDirAnnoations)
        {
            _inputAnnotationsFile = inputAnnotationsFile;
            _outputDirImages = outputDirImages;
            _outputDirAnnoations = outputDirAnnoations;

            _imageProcessor = new ImageProcessor(_outputDirImages);
        }

        public void GoProcess()
        {
            var allTags = AnnotationsReader.ReadAnnotationsFromFileAsync(_inputAnnotationsFile);

            var groupedTagsPerImage = allTags.GroupBy(t => t.ImagePath);

            foreach (var imageWithTags in groupedTagsPerImage)
            {
                var imagePath = imageWithTags.Key;
                _imageProcessor.ProcessImage(imagePath, imageWithTags);
            }
        }
    }
}
