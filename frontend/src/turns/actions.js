import Vue from 'vue';
import comm from '../comm/comm';

const actions = {
  order(context, payload) { // sourceId, targetId, amount
    const source = context.getters.activeTurn.cityOccupations[payload.sourceId];
    if (source.playerId != context.state.login.id) return;

    comm.post('play/setorder', payload, context, () => {
      context.commit('updateOrder', payload);
    });
  },
  prevTurn(context) {
    // invalid command - first turn || already running
    if (context.state.activeTurnId <= 0 || context.state.turnRun.armiesPosition != 0) return;

    // null - load
    const finalTurn = context.state.activeTurnId - 1;
    if (context.state.turns[finalTurn] == null)
    {
      comm.post('play/getTurn', { id: finalTurn}, context, (data) => {
        Vue.set(context.state.turns, finalTurn, data);
        context.commit('updatePrevTurn');
      });
    }
    // already loaded
    else
    {
      context.commit('updatePrevTurn');
    }
  },
  nextTurn(context) {
    // invalid command - last turn || already running
    if (context.state.activeTurnId >= context.state.turns.last || context.state.turnRun.armiesPosition != 0) return;

    // null - load
    const finalTurn = context.state.activeTurnId + 1;
    if (context.state.turns[finalTurn] == null)
    {
      comm.post('play/getTurn', { id: finalTurn}, context, (data) => {
        Vue.set(context.state.turns, finalTurn, data);
        context.commit('updateNextTurn');
      });
    }
    // already loaded
    else
    {
      context.commit('updateNextTurn');
    }
  }
};
export default actions;
