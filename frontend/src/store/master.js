export default {
  state: () => ({
    config: {
      armyRunDuration: 0.4,
      notificationDuration: 10
    },
  }),
  getters: {
    generateGuid: () => `${helpers.s4()}${helpers.s4()}-${helpers.s4()}-4${helpers.s4().substr(0, 3)}-${helpers.s4()}-${helpers.s4()}${helpers.s4()}${helpers.s4()}`.toLowerCase(),
  },
  mutations: {
    masterInitData(state, payload) {
      state.config = payload.config;
    },
  },
  actions: {
    masterInit(context) {
      context.commit("commRegisterReceiver", { route: "player/init", callback: "masterInitData" });
      context.commit("commRegisterReceiver", { route: "player/login", callback: "masterInitData" });
      
      if (context.rootState.user.login.jwtToken)
        context.dispatch("commSend", { route: "player/init" });
    }
  }
};

var helpers = {
  s4: () => (((1 + Math.random()) * 0x10000) | 0).toString(16).substring(1),
};
