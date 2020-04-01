import comm from '../comm/comm';

const actions = {
  login(context, payload) { // username, password
    comm.post("play/login", payload, context,
      (data) => {
        context.commit("login", data);
        context.commit("init", data);
      });
  }
};
export default actions;
