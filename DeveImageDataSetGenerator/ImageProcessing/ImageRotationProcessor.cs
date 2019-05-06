using DeveImageDataSetGenerator.DataInputOutput;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;

namespace DeveImageDataSetGenerator.ImageProcessing
{
    public class ImageRotationProcessor
    {
        private readonly string _outputDirImages;
        private readonly int[] _desiredRotations;

        public ImageRotationProcessor(string outputDirImages, params int[] desiredRotations)
        {
            _outputDirImages = outputDirImages;
            _desiredRotations = desiredRotations;
        }

        public void ProcessImage(string imagePath, IEnumerable<Annotations> annotations)
        {
            Console.WriteLine($"Handling image: {imagePath}");

            using (var image = Image.Load<Rgba32>(imagePath))
            {

                for (int i = 0; i < _desiredRotations.Length; i++)
                {
                    var desiredRotation = _desiredRotations[i];
                    var cloneOfImage = image.Clone();
                    cloneOfImage.Mutate(t => t.Rotate(desiredRotation));

                    var outFileName = $"{Path.GetFileNameWithoutExtension(imagePath)}_{i.ToString().PadLeft(2, '0')}.jpg";
                    var outPath = Path.Combine(_outputDirImages, outFileName);

                    using (var outStream = new FileStream(outPath, FileMode.Create, FileAccess.Write, FileShare.Read))
                    {
                        cloneOfImage.SaveAsJpeg(outStream);
                    }
                }
            }
        }

    }
}
