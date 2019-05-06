using CommandLine;

namespace DeveImageDataSetGenerator.CommandLine
{
    public class CommandLineArguments
    {
        [Option('i', "InputAnnotationsCsvPath", Required = true, HelpText = "The path of the Annotations CSV file.")]
        public string AnnotationsCsvPath { get; set; }

        [Option('o', "OutputDirImages", Required = true, HelpText = "The output directory for the images.")]
        public string OutputDirImages { get; set; }

        [Option('a', "OutputDirAnnotations", Required = true, HelpText = "The output directory for the annotations.")]
        public string OutputDirAnnotations { get; set; }
    }
}
