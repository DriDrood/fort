const mutations = {
  login(state, payload) { // username, password
    state.login = {
      id: 5,
      name: payload.username
    }
  },
  logout(state) {
    state.login = null;
  }
}
export default mutations;