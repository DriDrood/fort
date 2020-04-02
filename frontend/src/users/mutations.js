import Vue from 'vue';

const mutations = {
  login(state, payload) {
    state.login = payload.login;

    Vue.ls.set('id', payload.login.id);
    Vue.ls.set('name', payload.login.name);
    Vue.ls.set('jwtToken', payload.login.jwtToken);
  },
  logout(state) {
    state.login = {
      id: null,
      name: null,
      jwtToken: null
    };
  }
}
export default mutations;