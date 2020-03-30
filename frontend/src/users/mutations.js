const mutations = {
  login(state, payload) { // id, name, jwtToken
    state.login = payload;
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