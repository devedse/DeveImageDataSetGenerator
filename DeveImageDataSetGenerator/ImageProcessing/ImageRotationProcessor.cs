using DeveImageDataSetGenerator.DataInputOutput;
using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Units;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DeveImageDataSetGenerator.ImageProcessing
{
    public class ImageRotationProcessor
    {
        private readonly string _outputDirImages;
        private readonly string _outputDirectoryAnnotations;
        private readonly int[] _desiredRotations;

        public ImageRotationProcessor(string outputDirImages, string outputFileAnnotations, params int[] desiredRotations)
        {
            _outputDirImages = outputDirImages;
            _outputDirectoryAnnotations = Path.GetDirectoryName(outputFileAnnotations);
            _desiredRotations = desiredRotations;
        }

        public List<Annotations> ProcessImage(string imagePath, IEnumerable<Annotations> annotations)
        {
            var newAnnotations = new List<Annotations>();

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
                        Console.WriteLine($"\tStoring: {outFileName}");
                        cloneOfImage.SaveAsJpeg(outStream);
                    }

                    var rotationMatrix = Matrix2D.Rotation(new Angle(desiredRotation, new Degrees()));

                    var topLeft = new Vector2D(0, 0);
                    var topRight = new Vector2D(image.Width, 0);
                    var bottomLeft = new Vector2D(0, image.Height);
                    var bottomRight = new Vector2D(image.Width, image.Height);

                    var rotatedTopLeft = topLeft.TransformBy(rotationMatrix);
                    var rotatedTopRight = topRight.TransformBy(rotationMatrix);
                    var rotatedBottomLeft = bottomLeft.TransformBy(rotationMatrix);
                    var rotatedBottomRight = bottomRight.TransformBy(rotationMatrix);

                    var allRotatedPositions = new List<Vector2D>()
                    {
                        rotatedTopLeft,
                        rotatedTopRight,
                        rotatedBottomLeft,
                        rotatedBottomRight
                    };

                    var toAddInX = Math.Abs(allRotatedPositions.Min(t => t.X));
                    var toAddInY = Math.Abs(allRotatedPositions.Min(t => t.Y));

                    var vectorToAdd = new Vector2D(toAddInX, toAddInY);

                    foreach (var tag in annotations)
                    {
                        var tagTopLeft = new Vector2D(tag.X1.Value, tag.Y1.Value);
                        var tagTopRight = new Vector2D(tag.X2.Value, tag.Y1.Value);
                        var tagBottomLeft = new Vector2D(tag.X1.Value, tag.Y2.Value);
                        var tagBottomRight = new Vector2D(tag.X2.Value, tag.Y2.Value);

                        var tagRotatedTopLeft = tagTopLeft.TransformBy(rotationMatrix) + vectorToAdd;
                        var tagRotatedTopRight = tagTopRight.TransformBy(rotationMatrix) + vectorToAdd;
                        var tagRotatedBottomLeft = tagBottomLeft.TransformBy(rotationMatrix) + vectorToAdd;
                        var tagRotatedBottomRight = tagBottomRight.TransformBy(rotationMatrix) + vectorToAdd;


                        var allNewPositions = new List<Vector2D>()
                        {
                            tagRotatedTopLeft,
                            tagRotatedTopRight,
                            tagRotatedBottomLeft,
                            tagRotatedBottomRight
                        };

                        var newX1 = allNewPositions.Min(t => t.X);
                        var newX2 = allNewPositions.Max(t => t.X);
                        var newY1 = allNewPositions.Min(t => t.Y);
                        var newY2 = allNewPositions.Max(t => t.Y);

                        var newRelativeImagePath = Uri.UnescapeDataString(Path.GetRelativePath(_outputDirectoryAnnotations, outPath));

                        newAnnotations.Add(new Annotations()
                        {
                            ImagePath = newRelativeImagePath.Replace('\\', '/'),
                            Tag = tag.Tag,
                            X1 = (int)newX1,
                            X2 = (int)newX2,
                            Y1 = (int)newY1,
                            Y2 = (int)newY2
                        });
                    }
                }
            }

            return newAnnotations;
        }
    }
}
