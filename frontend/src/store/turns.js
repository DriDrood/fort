import Vue from "vue";

export default {
  state: () => ({
    activeId: 0,
    data: [
      // {
      //   cityOccupations: {
      //     '1': {
      //       playerId: '5',
      //       size: 50,
      //     },
      //     '2': {
      //       playerId: '4',
      //       size: 15
      //     },
      //   },
      //   orders: {
      //     '4>>3': {
      //       playerId: '4',
      //       amount: 10,
      //       size: 8
      //     },
      //   }
      // },
      // {
      //   cityOccupations: {
      //     '1': {
      //       playerId: '3',
      //       size: 15
      //     },
      //     '2': {
      //       playerId: '4',
      //       size: 15
      //     },
      //   },
      //   orders: {}
      // }
    ],
    moveProgress: 0, // 0, 1, 2
    tempTurn: null,
  }),
  getters: {
    isTurnCurrent: (state) => state.activeId == state.data.length - 1,
    activeTurn: (state) => state.data[state.activeId]
  },
  mutations: {
    turnsInitData(state, payload) {
      // no data
      if (!payload.turns)
        return;

      payload.turns.forEach(t => {
        Vue.set(state.data, t.id, t);
      });

      state.activeId = Math.max(...payload.turns.map(t => t.id));
    },
    turnsFinalized(state, payload) {
      state.tempTurn = payload.turn;
    },
    // id, playerId, startSize, endSize, startAmount, endAmount
    turnsOrder(state, payload) {
      const currentTurn = state.data[state.data.length - 1];
      const sourceCity = currentTurn.cityOccupations[payload.id.split(">>")[0]];
  
      const max = ((currentTurn.orders[payload.id] && currentTurn.orders[payload.id].startAmount) || 0) + sourceCity.availableArmy;
      if (payload.startAmount > 0)
        Vue.set(currentTurn.orders, payload.id, payload);
      else if (currentTurn.orders[payload.id])
        Vue.delete(currentTurn.orders, payload.id);
  
      sourceCity.availableArmy = max - payload.startAmount;
    },
    // armyMoveDuration
    turnsPrev: async (state, payload) => {
      // invalid command
      if (state.activeId <= 0 || state.moveProgress != 0) return;

      // init
      state.activeId -= 1;
      state.moveProgress = 2;
      await helpers.sleep(10);

      // move
      state.moveProgress = 1;
      await helpers.sleep(payload.armyMoveDuration * 1000);
      state.moveProgress = 0;
      await helpers.sleep(payload.armyMoveDuration * 1000);
    },
    // armyMoveDuration
    turnsNext: async (state, payload) => {
      // invalid command
      if (state.activeId >= state.data.last || state.moveProgress != 0) return;
  
      // move
      state.moveProgress = 1;
      await helpers.sleep(payload.armyMoveDuration * 1000);
      state.moveProgress = 2;
      await helpers.sleep(payload.armyMoveDuration * 1000);
  
      // increase active
      state.activeId += 1;
      state.moveProgress = 0;
    }
  },
  actions: {
    turnsInit: context => {
      context.commit("commRegisterReceiver", { route: "player/init", callback: "turnsInitData" });
      context.commit("commRegisterReceiver", { route: "player/login", callback: "turnsInitData" });

      context.commit("commRegisterReceiver", { route: "player/setOrder", callback: "turnsOrder" });
      context.commit("commRegisterReceiver", { route: "player/turnFinalized", callback: "turnsFinalized" });
    },
    // sourceId, targetId, amount
    turnsOrder(context, payload) {
      const source = context.getters.activeTurn.cityOccupations[payload.sourceId];
      if (source.playerId != context.rootState.user.login.id) return;
  
      context.dispatch("commSend", { route: "player/setOrder", data: payload });
    },
    turnsPrev(context) {
      // invalid command - first turn || already running
      if (context.state.activeId <= 0 || context.state.moveProgress != 0) return;
  
      // null - load
      const finalTurn = context.state.activeId - 1;
      if (context.state.data[finalTurn] == null)
      {
        context.dispatch("commSend", {
          route: "player/getTurn",
          data: { id: finalTurn },
          callback: { "turnsReceive": "action" },
        });
      }
      // already loaded
      else
      {
        context.commit('turnsPrev', { armyMoveDuration: context.rootState.master.config.armyMoveDuration });
      }
    },
    turnsNext(context) {
      // invalid command - last turn || already running
      if (context.state.activeId >= context.state.data.last || context.state.moveProgress != 0) return;
  
      // null - load
      const finalTurn = context.state.activeId + 1;
      if (context.state.data[finalTurn] == null)
      {
        context.dispatch("commSend", {
          route: "player/getTurn",
          data: { id: finalTurn },
          callback: { "turnsReceive": "action" },
        });
      }
      // already loaded
      else
      {
        context.commit("turnsNext", { armyMoveDuration: context.rootState.master.config.armyMoveDuration });
      }
    },
    turnsReceive: (context, payload) => {
      Vue.set(context.state.data, payload.id, payload);

      if (payload.id < context.state.activeId)
        context.commit("turnsPrev", { armyMoveDuration: context.rootState.master.config.armyMoveDuration });
      else if (payload.id > context.state.activeId)
        context.commit("turnsNext", { armyMoveDuration: context.rootState.master.config.armyMoveDuration });
    },
  },
}


const helpers = {
  sleep(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
  }
}
