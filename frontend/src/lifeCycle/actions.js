import comm from '../comm/comm';

const actions = {
  toggleDone(context) {
    var done = !context.state.currentTurn.done;
    comm.post('play/turnDone', { done }, context, () => {
      context.commit('done', { done });
    });
  }
};
export default actions;
