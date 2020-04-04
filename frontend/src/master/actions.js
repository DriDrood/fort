import Vue from 'vue';
import comm from '../comm/comm';

const actions = {
  init(context) {
    var id = Vue.ls.get('id');
    var name = Vue.ls.get('name');
    var jwt = Vue.ls.get('jwtToken');

    if (id && name && jwt)
    {
      context.state.login.jwtToken = jwt;

      comm.post('play/init', null, context, (data) => {
        context.commit('updateInit', data);
        context.state.activeTurnId = context.state.currentTurn.id;
        context.state.login.id = id;
        context.state.login.name = name;
      });
    }
  },
  toggleDone(context) {
    var done = !context.state.currentTurn.done;
    comm.post('play/turnDone', { done }, context, () => context.commit('updateDone', { done }));
  }
};
export default actions;
