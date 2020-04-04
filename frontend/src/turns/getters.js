export default {
  isTurnCurrent: (state) => state.activeTurnId == state.currentTurn.id,
  activeTurn: (state) => state.turns[state.activeTurnId]
}