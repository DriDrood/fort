import comm from '../comm/comm';

const actions = {
  checkStatePeriodicaly(context) {
    setInterval(() => {
      if (context.state.login.id) {
        comm.post(
          'play/checkState',
          { state: context.state.currentTurn.state, turnId: context.state.currentTurn.id },
          context,
          (data) => context.commit('updateTurn', data));
      }
    }, 10000);
  }
};
export default actions;
