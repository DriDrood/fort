export default {
  state: () => ({
    state: {
      endsAt: null, // new Date(2021, 1, 1),
      key: 'Init',
      remains: '-:--',
      closed: false
    },
    countDown: null,
  }),
  mutations: {
    lifecycleUpdateState(state, payload) {
      state.state.id = payload.state.id;
      state.state.key = payload.state.key;
  
      if (payload.state.endsAt != null) {
        var regex = /^(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2}):(\d{2})(\.\d*)?/;
        var dateRegex = payload.state.endsAt.match(regex);
        state.state.endsAt = new Date(Date.UTC(dateRegex[1], dateRegex[2] - 1, dateRegex[3], dateRegex[4], dateRegex[5], dateRegex[6]));
      }
      else {
        state.state.endsAt = null;
      }
    },
    // closed
    lifecycleToggleClose: (state, payload) => {
      state.state.closed = payload.closed;
    }
  },
  actions: {
    lifecycleInit: context => {
      context.commit("commRegisterReceiver", { route: "player/setTurnClosed", callback: "lifecycleToggleClose" });
    },
    lifecycleToggleClose: context => {
      context.dispatch("commSend", { route: "player/setTurnClosed", data: { closed: !context.state.state.closed }});
    },
    lifecycleCountDown: (context) => {
      setTimeout(() =>
        context.state.countDown = setInterval(() => {
          if (context.state.endsAt) {
            let remains = context.state.endsAt - new Date();
  
            // turn end
            if (remains <= 0) {
              remains = 0;
              context.dispatch("masterStopCountDown");
            }
            
            const remainsDate = new Date(remains);
            context.state.remains = `${remainsDate.getMinutes()}:${remainsDate.getSeconds().toString().padStart(2, '0')}`;
          }
          else {
            context.rootState.lifecycle.state.remains = '-:--';
          }
        }, 1000), new Date(context.state.endsAt - new Date()).getMilliseconds());
    },
    lifecycleStopCountDown: context => {
      clearInterval(context.state.countDown);
    }
  },
}
