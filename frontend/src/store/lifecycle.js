import Vue from "vue";

export default {
  state: () => ({
    state: {
      endsAt: null, // new Date(2021, 1, 1),
      key: 'Ready',
      remains: '-:--',
      closed: false
    },
    countDown: null,
  }),
  mutations: {
    // id, key, endsAt
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
    },
    lifecycleResetGame: () => {
      location.reload();
    }
  },
  actions: {
    lifecycleInit: context => {
      context.commit("commRegisterReceiver", { route: "player/init", callback: "lifecycleUpdateState", callbackType: "action" });
      context.commit("commRegisterReceiver", { route: "player/login", callback: "lifecycleUpdateState", callbackType: "action" });

      context.commit("commRegisterReceiver", { route: "player/setTurnClosed", callback: "lifecycleToggleClose" });
      context.commit("commRegisterReceiver", { route: "player/stateChanged", callback: "lifecycleUpdateState", callbackType: "action" });
      context.commit("commRegisterReceiver", { route: "player/resetGame", callback: "lifecycleResetGame" });
    },
    lifecycleToggleClose: context => {
      context.dispatch("commSend", { route: "player/setTurnClosed", data: { closed: !context.state.state.closed }});
    },
    // id, key, endsAt
    lifecycleUpdateState: (context, payload) => {
      const currentState = context.state.state.key;

      context.commit("lifecycleUpdateState", payload);
      context.dispatch("lifecycleCountDown");
      
      // next turn
      if (currentState == "Finalizing" && payload.state.key == "Running")
      {
        var isLastTurn = context.getters.isTurnCurrent;
        Vue.set(context.rootState.turns.data, context.rootState.turns.tempTurn.id, context.rootState.turns.tempTurn);
        if (isLastTurn)
          context.dispatch("turnsNext");
      }
    },
    lifecycleCountDown: (context) => {
      setTimeout(() =>
      {
        context.state.countDown = setInterval(() => {
          if (context.state.state.endsAt) {
            let remains = context.state.state.endsAt - new Date();
  
            // turn end
            if (remains <= 0) {
              remains = 0;
              context.dispatch("lifecycleStopCountDown");
            }

            const remainsDate = new Date(remains);
            context.state.state.remains = `${remainsDate.getMinutes()}:${remainsDate.getSeconds().toString().padStart(2, '0')}`;
          }
          else {
            context.state.state.remains = '-:--';
            context.dispatch("lifecycleStopCountDown");
          }
        }, 1000);
      }, new Date(context.state.state.endsAt - new Date()).getMilliseconds());
    },
    lifecycleStopCountDown: context => {
      clearInterval(context.state.countDown);
    }
  },
}
