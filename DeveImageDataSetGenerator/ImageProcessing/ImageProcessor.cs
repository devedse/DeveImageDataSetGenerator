using DeveImageDataSetGenerator.DataInputOutput;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;

namespace DeveImageDataSetGenerator.ImageProcessing
{
    public class ImageProcessor
    {
        private readonly string _outputDirImages;

        public ImageProcessor(string outputDirImages)
        {
            _outputDirImages = outputDirImages;
        }

        public void ProcessImage(string imagePath, IEnumerable<Annotations> annotations)
        {
            Console.WriteLine($"Handling image: {imagePath}");

            using (var image = Image.Load<Rgba32>(imagePath))
            {
                var cloneOfImage = image.Clone();

                cloneOfImage.Mutate(t => t.Rotate(30));

                var outFileName = $"{Path.GetFileNameWithoutExtension(imagePath)}_01.jpg";
                var outPath = Path.Combine(_outputDirImages, outFileName);

                using (var outStream = new FileStream(outPath, FileMode.Create, FileAccess.Write, FileShare.Read))
                {
                    cloneOfImage.SaveAsJpeg(outStream);
                }
            }
        }

    }
}
