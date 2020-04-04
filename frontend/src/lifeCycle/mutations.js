export default {
  updateCurrentTurn(state, payload) {
    state.currentTurn.id = payload.currentTurn.id;
    state.currentTurn.state = payload.currentTurn.state;

    if (payload.currentTurn.endsAt != null) {
      var regex = /^(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2}):(\d{2})(\.\d*)?/;
      var dateRegex = payload.currentTurn.endsAt.match(regex);
      state.currentTurn.endsAt = new Date(Date.UTC(dateRegex[1], dateRegex[2] - 1, dateRegex[3], dateRegex[4], dateRegex[5], dateRegex[6]));
    }
    else
    {
      state.currentTurn.endsAt = null;
    }
  }
}
