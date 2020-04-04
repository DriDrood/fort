export default {
  updateCurrentTurn(state, payload) {
    state.currentTurn.id = payload.currentTurn.id;
    state.currentTurn.endsAt = payload.currentTurn.endsAt;
    state.currentTurn.state = payload.currentTurn.state;
  }
}
