namespace Fort.Helpers
{
    public static class Position
    {
        public static double RealHeight { get; set; } = 720;
        public static double RealWidth { get; set; } = 1280;

        
        public static double VirtualHeight { get; } = 720;
        public static double VirtualWidth { get; } = 1280;

        public static double ToRealHeight(double virtualPosition)
        {
            return (virtualPosition / VirtualHeight) * RealHeight;
        }
        public static double ToRealWidth(double virtualPosition)
        {
            return (virtualPosition / VirtualWidth) * RealWidth;
        }
    }
}