namespace Fort.Services
{
    public class RealPositionService
    {
        public RealPositionService()
        {
            RealHeight = Program.Config.RealHeight;
            RealWidth = Program.Config.RealWidth;
        }
        
        public double RealHeight { get; private set; }
        public double RealWidth { get; private set; }

        
        public double VirtualHeight { get; } = 720;
        public double VirtualWidth { get; } = 1280;

        public double ToRealHeight(double virtualPosition)
        {
            return (virtualPosition / VirtualHeight) * RealHeight;
        }
        public double ToRealWidth(double virtualPosition)
        {
            return (virtualPosition / VirtualWidth) * RealWidth;
        }
    }
}