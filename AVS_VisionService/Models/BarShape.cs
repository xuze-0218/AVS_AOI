namespace AVS_Service.Models
{
    public enum BarShape { Circle, SquareBar }

    public static class BarShapeExtensions
    {
        public static string ToFileSuffix(this BarShape shape)
        {
            switch (shape)
            {
                case BarShape.SquareBar:
                    return "Square";
                case BarShape.Circle:
                    return "Circle";
                default:
                    return "Unknown";
            }
        }
    }
}
