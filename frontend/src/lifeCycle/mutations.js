export default {
  done: (state, payload) => state.currentTurn.done = payload.done,
  countDown: (state) => {
    setTimeout(() =>
      setInterval(() => {
        if (state.currentTurn.endsAt) {
          const remainsDate = new Date(state.currentTurn.endsAt - new Date());
          state.currentTurn.remains = `${remainsDate.getMinutes()}:${remainsDate.getSeconds().toString().padStart(2, '0')}`;
        }
        else {
          state.currentTurn.remains = '-:--';
        }
      }, 1000), new Date(state.currentTurn.endsAt - new Date()).getMilliseconds());
  }
}
