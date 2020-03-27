import lifecycleGetters from '../lifecycle/getters';

export default {
  isTurnCurrent: (state) => state.activeTurnId == lifecycleGetters.currentTurnId(state),
  activeTurn: (state) => state.turns[state.activeTurnId]
}