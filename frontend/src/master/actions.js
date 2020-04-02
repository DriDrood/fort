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
        context.commit('init', data);
        context.state.login.id = id;
        context.state.login.name = name;
      });
    }
  }
};
export default actions;
