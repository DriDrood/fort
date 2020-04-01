const mutations = {
  login(state, payload) {
    state.login = payload.login;
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