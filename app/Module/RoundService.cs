namespace app.Module
{
    public class RoundService
    {
        public Status State { get; private set; }
        public void StartGame(StartMode startMode) { }
        public void ResetGame() { }
        public void Pause() { }
        public void Resume() { }
        public void FinishTimer() { }

        private void InitRound() { }
        private void StartRound() { }
        private void EndRound() { } // show end, cound result
        private void ShowResult() { }

        public enum StartMode
        {
            NewGame,
            NewRound,
            ContinueLastRound
        }
        public enum Status
        {
            None,
            New,
            Starting,
            Running,
            Ending,
            Ended,
            ShowingResult
        }
    }
}