namespace Fort.Services
{
    public enum ELifecycleState
    {
        // Empty DB
        Init,

        // In DB are starting positions, not started yet
        Ready,

        // next turn ready, after pause
        Starting,
        
        // turn started but never ends
        Paused,

        // running turn - started, turn end is in the future
        Running,

        // turn end is in the past
        Finalizing,

        // next turn ready, waiting pause
        Finalized,
    }
}