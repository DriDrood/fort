namespace Fort.Services
{
    public enum ELifecycleState
    {
        /// <summary>
        ///   The game didn't start yet
        /// </summary>
        Ready,

        /// <summary>
        ///   Play fast!
        /// </summary>
        Running,
        
        /// <summary>
        ///   turn started but never ends
        /// </summary>
        Paused,

        /// <summary>
        ///   gap between turns
        /// </summary>
        Finalizing,

        /// <summary>
        ///   Game has a winner
        /// </summary>
        End,
    }
}