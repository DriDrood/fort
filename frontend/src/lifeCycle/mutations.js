export default {
  toggleDone: (state) => state.currentTurn.done = !state.currentTurn.done,
  countDown: (state) => {
    setTimeout(() =>
      setInterval(() => {
        if (state.currentTurn.endsAt) {
          const remainsDate = new Date(state.currentTurn.endsAt - new Date());
          state.currentTurn.remains = `${remainsDate.getMinutes()}:${remainsDate.getSeconds().toString().padStart(2, '0')}`;
        }
        else {
          state.turn.remains = '-:--';
        }
      }, 1000), new Date(state.currentTurn.endsAt - new Date()).getMilliseconds());
  }
}
