import comm from '../comm/comm';

const actions = {
  checkStatePeriodicaly(context) {
    setInterval(() => {
      if (context.state.login.id)
        actions.checkState(context);
    }, 10000);
  },
  checkState(context) {
    comm.post(
      'play/checkState',
      { state: context.state.currentTurn.state, turnId: context.state.currentTurn.id },
      context,
      (data) => {
        context.commit('updateCurrentTurn', data);
        context.commit('updateTurn', data);
      });
  }
};
export default actions;
