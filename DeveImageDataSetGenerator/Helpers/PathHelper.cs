using System;

namespace DeveImageDataSetGenerator.Helpers
{
    public static class PathHelper
    {
        public static string EscapePathCorrectly(string inputPath)
        {
            return Uri.UnescapeDataString(inputPath).Replace("\\", "/");
        }
    }
}
