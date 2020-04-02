import Vue from 'vue';
import comm from '../comm/comm';

const actions = {
  prevTurn(context) {
    // invalid command - first turn || already running
    if (context.state.activeTurnId <= 0 || context.state.turnRun.armiesPosition != 0) return;

    // null - load
    const finalTurn = context.state.activeTurnId - 1;
    if (context.state.turns[finalTurn] == null)
    {
      comm.post('play/getTurn', { id: finalTurn}, context, (data) => {
        Vue.set(context.state.turns, finalTurn, data);
        context.commit('prevTurn');
      });
    }
    // already loaded
    else
    {
      context.commit('prevTurn');
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
        context.commit('nextTurn');
      });
    }
    // already loaded
    else
    {
      context.commit('nextTurn');
    }
  },
  order(context, payload) { // sourceId, targetId, amount
    const source = context.getters.activeTurn.cityOccupations[payload.sourceId];
    if (source.playerId != context.state.login.id) return;

    comm.post('play/setorder', payload, context, () => {
      context.commit('order', payload);
    });
  }
};
export default actions;
