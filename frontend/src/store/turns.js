import Vue from "vue";

export default {
  state: () => ({
    activeId: 0,
    currentId: 0,
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
  }),
  getters: {
    isTurnCurrent: (state) => state.activeId == state.currentId,
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

      state.currentId = Math.max(...payload.turns.map(t => t.id));
      state.activeId = state.currentId;
    },
    // sourceId, targetId, amount
    turnsOrder(state, payload) {
      const currentTurn = state.data[state.currentId];
      const sourceCity = currentTurn.cityOccupations[payload.id.split(">>")[0]];
  
      const max = ((currentTurn.orders[payload.id] && currentTurn.orders[payload.id].amount) || 0) + sourceCity.availableArmy;
      if (payload.amount > 0)
        Vue.set(currentTurn.orders, payload.id, payload);
      else if (currentTurn.orders[payload.id])
        Vue.delete(currentTurn.orders, payload.id);
  
      sourceCity.availableArmy = max - payload.amount;
    },
    turnsPrev: async (state) => {
      // invalid command
      if (state.activeId <= 0 || state.turnChangeProgress.armiesPosition != 0) return;

      console.log("turnsPrev", state.data, state.activeId);
  
      // init
      const orders = state.data[state.activeId - 1].orders;
      var met = helpers.meetingResults(state, orders);
      helpers.createMove(state, orders, met, true);
  
      // move
      await helpers.sleep(10);
      state.turnChangeProgress.armiesPosition = 1;
      await helpers.sleep(state.config.armyRunDuration * 1000);
      state.turnChangeProgress.armiesPosition = 2;
      await helpers.sleep(state.config.armyRunDuration * 1000);
  
      // decrease active
      state.activeId -= 1;
  
      // clean
      Vue.set(state.turnChangeProgress, 'armies', []);
      state.turnChangeProgress.armiesPosition = 0;
    },
    turnsNext: async (state) => {
      // invalid command
      if (state.activeId >= state.data.last || state.turnChangeProgress.armiesPosition != 0) return;
  
      // init
      const orders = state.activeTurn.orders;
      let met = helpers.meetingResults(state, orders);
      helpers.createMove(state, orders, met, false);
  
      // move
      await helpers.sleep(10);
      state.turnChangeProgress.armiesPosition = 1;
      await helpers.sleep(state.config.armyRunDuration * 1000);
      state.turnChangeProgress.armiesPosition = 2;
      await helpers.sleep(state.config.armyRunDuration * 1000);
  
      // increase active
      state.activeId += 1;
  
      // clean
      Vue.set(state.turnChangeProgress, 'armies', []);
      state.turnChangeProgress.armiesPosition = 0;
    }
  },
  actions: {
    turnsInit: context => {
      context.commit("commRegisterReceiver", { route: "player/init", callback: "turnsInitData" });
      context.commit("commRegisterReceiver", { route: "player/login", callback: "turnsInitData" });

      context.commit("commRegisterReceiver", { route: "player/setOrder", callback: "turnsOrder" });
    },
    // sourceId, targetId, amount
    turnsOrder(context, payload) {
      const source = context.getters.activeTurn.cityOccupations[payload.sourceId];
      if (source.playerId != context.state.login.id) return;
  
      context.dispatch("commSend", { route: "player/setOrder", data: payload });
    },
    turnsPrev(context) {
      // invalid command - first turn || already running
      if (context.state.activeId <= 0 || context.state.turnChangeProgress.armiesPosition != 0) return;
  
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
        context.commit('turnsPrev');
      }
    },
    turnsNext(context) {
      // invalid command - last turn || already running
      if (context.state.activeId >= context.state.data.last || context.state.turnChangeProgress.armiesPosition != 0) return;
  
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
        context.commit("turnsNext");
      }
    },
    turnsReceive: (context, payload) => {
      Vue.set(context.state.data, payload.id, payload);

      if (payload.id < context.state.activeId)
        context.commit("turnsPrev");
      else if (payload.id > context.state.activeId)
        context.commit("turnsNext")
    },
  },
}


const helpers = {
  meetingResults(state, orders) {
    let met = {};
    // meeting
    Object.keys(orders).forEach(orderId => {
      // already met
      if (met[orderId] != null) return;

      const [sourceId, targetId] = orderId.split('>>');
      const reverseOrderId = `${targetId}>>${sourceId}`;

      const order = orders[orderId];
      const reverseOrder = orders[reverseOrderId];
      // no enemy reverse turn
      if (!reverseOrder || state.players[order.playerId].teamId == state.players[reverseOrder.playerId].teamId) return;

      // meeting ;-)
      // first win
      if (order.size > reverseOrder.size) {
        met[orderId] = this.getSizeAfterMeeting(order.size, reverseOrder.size);
        met[reverseOrderId] = 0;
      }
      // second win
      else if (order.size < reverseOrder.size) {
        met[orderId] = 0;
        met[reverseOrderId] = this.getSizeAfterMeeting(reverseOrder.size, order.size);
      }
      // same size
      else {
        met[orderId] = 5;
        met[reverseOrderId] = 5;
      }
    });

    return met;
  },
  createMove(state, orders, met, reverse) {
    // create
    Object.keys(orders).forEach(orderId => {
      // init
      const order = orders[orderId];
      const [sourceId, targetId] = orderId.split('>>');
      // get positions
      const start = state.cities[reverse ? targetId : sourceId];
      const end = state.cities[reverse ? sourceId : targetId];
      // get sizes
      const size1 = reverse
        ? met[orderId] || order.size
        : order.size;
      const size2 = reverse
        ? order.size
        : met[orderId] || order.size;

      // return
      state.turnChangeProgress.armies.push({
        startX: start.x,
        startY: start.y,
        endX: end.x,
        endY: end.y,
        size1: size1,
        size2: size2,
        playerId: order.playerId
      });
    });
  },
  getSizeAfterMeeting(biggerArmySize, smallerArmySize) {
    return Math.floor(Math.sqrt(Math.pow(biggerArmySize - 5, 2) - Math.pow(smallerArmySize - 5, 2))) + 5;
  },
  getArmySize(army) {
    if (!army)
      return null;

    return Math.floor(Math.sqrt(army)) + 5;
  },
  sleep(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
  }
}