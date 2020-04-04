import comm from '../comm/comm';

const actions = {
  login(context, payload) { // email, password
    comm.post("play/login", payload, context,
      (data) => {
        context.commit("updateLogin", data);
        context.commit("updateInit", data);
      });
  }
};
export default actions;
