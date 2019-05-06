using CommandLine;
using DeveImageDataSetGenerator.CommandLine;
using System;
using System.Threading.Tasks;

namespace DeveImageDataSetGenerator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<CommandLineArguments>(args)
                   .WithParsed<CommandLineArguments>(o =>
                   {
                       MainWithArgs(o);
                   });
        }

        public static void MainWithArgs(CommandLineArguments args)
        {
            var gen = new ImageDataSetGenerator(args.AnnotationsCsvPath, args.OutputDirImages, args.OutputDirAnnotations);
            gen.GoProcess();
        }
    }
}
