using SixLabors.Primitives;

namespace DeveImageDataSetGenerator.Helpers
{
    public static class SpatialHelper
    {
        public static Rectangle BoxToRectangle(int x1, int y1, int x2, int y2)
        {
            return new Rectangle(x1, y1, x2 - x1 - 1, y2 - y1 - 1);
        }
    }
}
