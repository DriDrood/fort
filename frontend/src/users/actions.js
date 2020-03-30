import comm from '../comm/comm';

const actions = {
  login(context, payload) { // username, password
    comm.post("login", payload, context,
      (data) => context.commit("login", data));
  }
};
export default actions;