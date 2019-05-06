using DeveImageDataSetGenerator.DataInputOutput;
using DeveImageDataSetGenerator.Helpers;
using DeveImageDataSetGenerator.ImageProcessing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using System;
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

            allAnnotations.AddRange(GoGenerateImages(imagesWithNoCarsOnThem, allImagesWithMaxAmountOfCarsOnThem));
            allAnnotations.AddRange(GoCopyTheseImages(allImages));
            allAnnotations.AddRange(GoRotateTheseImages(imagesWithCarsOnThem));

            AnnotationsReaderWriter.WriteAnnotationsToFile(_outputAnnotationsFile, allAnnotations);
        }

        private List<Annotations> GoGenerateImages(List<IGrouping<string, Annotations>> imagesWithNoCars, List<IGrouping<string, Annotations>> imagesWithLotsOfCars)
        {
            var allAnnotations = new List<Annotations>();

            var outputAnnotationsDir = Path.GetDirectoryName(_outputAnnotationsFile);

            int minOfBoth = Math.Min(imagesWithNoCars.Count, imagesWithLotsOfCars.Count);
            int maxOfBoth = Math.Max(imagesWithNoCars.Count, imagesWithLotsOfCars.Count);

            for (int i = 0; i < maxOfBoth; i++)
            {
                var iNoCars = i % imagesWithNoCars.Count;
                var iLotsOfCars = i % imagesWithLotsOfCars.Count;

                var noCarImageTags = imagesWithNoCars[iNoCars];
                var lotsOfCarImageTags = imagesWithLotsOfCars[iLotsOfCars];

                var imagePathNoCars = Path.Combine(_dirOfAnnotationsFile, noCarImageTags.Key);
                var imagePathNoCarsResolved = Path.GetFullPath(imagePathNoCars);

                var imagePathLotsOfCars = Path.Combine(_dirOfAnnotationsFile, lotsOfCarImageTags.Key);
                var imagePathLotsOfCarsResolved = Path.GetFullPath(imagePathLotsOfCars);

                using (var imageNoCars = Image.Load(imagePathNoCarsResolved))
                {
                    using (var imageWithCars = Image.Load(imagePathLotsOfCarsResolved))
                    {

                        foreach (var carImageTags in lotsOfCarImageTags)
                        {
                            var rectangle = SpatialHelper.BoxToRectangle(carImageTags.X1.Value, carImageTags.Y1.Value, carImageTags.X2.Value, carImageTags.Y2.Value);

                            var cloneOfImageWithCars = imageWithCars.Clone();
                            cloneOfImageWithCars.Mutate(t => t.Crop(rectangle));

                            imageNoCars.Mutate(t => t.DrawImage(cloneOfImageWithCars, new Point(rectangle.X, rectangle.Y), GraphicsOptions.Default));
                        }
                    }

                    var outFileName = $"Generated_{i}.jpg";
                    var outPath = Path.Combine(_outputDirImages, outFileName);
                    using (var outStream = new FileStream(outPath, FileMode.Create, FileAccess.Write, FileShare.Read))
                    {
                        Console.WriteLine($"\tStoring: {outFileName}");
                        imageNoCars.SaveAsJpeg(outStream);
                    }

                    foreach (var existingTag in lotsOfCarImageTags)
                    {
                        var newTag = new Annotations()
                        {
                            Tag = existingTag.Tag,
                            ImagePath = Path.GetRelativePath(outputAnnotationsDir, outPath),
                            X1 = existingTag.X1,
                            X2 = existingTag.X2,
                            Y1 = existingTag.Y1,
                            Y2 = existingTag.Y2
                        };

                        allAnnotations.Add(newTag);
                    }
                }
            }

            return allAnnotations;
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
