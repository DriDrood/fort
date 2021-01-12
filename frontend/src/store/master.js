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
    },
    masterCountDown: (context) => {
      setTimeout(() =>
        setInterval(() => {
          if (context.state.currentTurn.endsAt) {
            const remains = context.state.currentTurn.endsAt - new Date();
  
            // turn end
            if (remains <= 0) {
              context.dispatch('checkState');
            }
            // turn is running
            else {
              const remainsDate = new Date(remains);
              context.state.currentTurn.remains = `${remainsDate.getMinutes()}:${remainsDate.getSeconds().toString().padStart(2, '0')}`;
            }
          }
          else {
            context.state.currentTurn.remains = '-:--';
          }
        }, 1000), new Date(context.state.currentTurn.endsAt - new Date()).getMilliseconds());
    },
  }
};

var helpers = {
  s4: () => (((1 + Math.random()) * 0x10000) | 0).toString(16).substring(1),
};
