import Vue from 'vue'
import Vuex from 'vuex'

Vue.use(Vuex)

export default new Vuex.Store({
  state: {
    login: {
      id: '5',
      name: 'hello'
    },
    done: false,
    turn: {
      activeId: 0,
      endsAt: new Date(2020, 1, 1),
      remains: '-:--'
    },
    cities: {
      1: {
        id: 1,
        x: 500,
        y: 500,
        owner: '5',
        army: 11
      },
      2: {
        id: 2,
        x: 400,
        y: 600,
        owner: '3',
        army: 12
      },
      3: {
        id: 3,
        x: 300,
        y: 400,
        owner: '5',
        army: 3
      },
      4: {
        id: 4,
        x: 400,
        y: 370,
        owner: '4',
        army: 24
      }
    },
    roads: {
      1: [2, 3],
      2: [1],
      3: [1, 4],
      4: [3]
    },
    turns: [
      {
        '4-3': {
          amount: 10,
          playerId: '4'
        }
      },
      {
        '1-2': {
          amount: 10,
          playerId: '5'
        }
      }
    ],
    players: {
      '3': {
        name: 'uuu',
        teamId: 3
      },
      '4': {
        name: 'enemy',
        teamId: 2
      },
      '5': {
        name: 'hello',
        teamId: 1
      }
    },
    teams: {
      1: { color: '#83824b', light: '#c4c498' },
      2: { color: '#52834b', light: '#9dc498' },
      3: { color: '#4b7183', light: '#98b6c4' }
    },
    move: {
      armies: [],
      armiesPosition: 0,
      duration: 0.5
    }
  },
  getters: {
    isTurnCurrent: (state) => state.turn.activeId == state.turns.length - 1,
    distinctRoads: (state) => {
      let result = [];
      Object.keys(state.roads).forEach(id => {
        const sourceId = parseInt(id);
        const targetIds = state.roads[sourceId];
        targetIds.forEach(targetId => {
          if (sourceId < targetId)
            result.push({ source: state.cities[sourceId], target: state.cities[targetId] });
        })
      });
      return result;
    }
  },
  mutations: {
    toggleDone: (state) => state.done = !state.done,
    prevTurn: async (state) => {
      // invalid command
      if (state.turn.activeId <= 0) return;
      // decrease active
      state.turn.activeId -= 1;
      // army move
      const turn = state.turns[state.turn.activeId];
      let met = meetings(state, turn);
      entranceCities(state, turn, met, true);
      createMove(state, turn, met, true);
      await sleep(10);
      state.move.armiesPosition = 1;
      await sleep(state.move.duration * 1000);
      state.move.armiesPosition = 2;
      await sleep(state.move.duration * 1000);
      leaveCities(state, turn, true);
      Vue.set(state.move, 'armies', []);
      state.move.armiesPosition = 0;
    },
    nextTurn: async (state) => {
      // invalid command
      if (state.turn.activeId >= state.turn.last) return;
      // increase active
      state.turn.activeId += 1;
      // army move
      const turn = state.turns[state.turn.activeId - 1];
      let met = meetings(state, turn);
      leaveCities(state, turn, false);
      createMove(state, turn, met, false);
      await sleep(10);
      state.move.armiesPosition = 1;
      await sleep(state.move.duration * 1000);
      state.move.armiesPosition = 2;
      await sleep(state.move.duration * 1000);
      entranceCities(state, turn, met, false);
      Vue.set(state.move, 'armies', []);
      state.move.armiesPosition = 0;
    },
    order(state, payload) { // sourceId, targetId, amount, sourceCityRemains
      const source = state.cities[payload.sourceId];
      if (source.owner != state.login.id) return;
      if (payload.max < payload.amount) return;

      Vue.set(state.turns[state.turn.activeId], `${payload.sourceId}-${payload.targetId}`, { amount: payload.amount, playerId: state.login.id });
      source.army = payload.max - payload.amount;
    },
    countDown: (state) => {
      setTimeout(() =>
        setInterval(() => {
          if (state.turn.endsAt) {
            const remainsDate = new Date(state.turn.endsAt - new Date());
            state.turn.remains = `${remainsDate.getMinutes()}:${remainsDate.getSeconds().toString().padStart(2, '0')}`;
          }
          else {
            state.turn.remains = '-:--';
          }
        }, 1000), new Date(state.turn.endsAt - new Date()).getMilliseconds());
    }
  }
});

function meetings(state, turn) {
  let met = {};
  // meeting
  Object.keys(turn).forEach(orderId => {
    // already met
    if (met[orderId]) return;

    const [sourceId, targetId] = orderId.split('-');
    const reverseOrderId = `${targetId}-${sourceId}`;

    const order = turn[orderId];
    const reverseOrder = turn[reverseOrderId];
    // no enemy reverse turn
    if (!reverseOrder || state.players[order.playerId].teamId == state.players[reverseOrder.playerId].teamId) return;

    // meeting ;-)
    met[orderId] = Math.max(order.amount - reverseOrder.amount, 0);
    met[reverseOrderId] = Math.max(reverseOrderId - order.amount, 0);
  });

  return met;
}
function leaveCities(state, turn, reverse) {
  Object.keys(turn).forEach(orderId => {
    const { amount } = turn[orderId];
    const source = state.cities[orderId.split('-')[0]];

    if (!reverse)
      source.army -= amount;
    else
      source.army += amount;
  });
}
function entranceCities(state, turn, met, reverse) {
  let keys = Object.keys(turn);
  if (reverse) keys = keys.reverse();
  keys.forEach(orderId => {
    const target = state.cities[orderId.split('-')[1]];
    const order = turn[orderId];

    let amount = order.amount;
    if (met[orderId]) amount = met[orderId];
    if (amount == 0) return;

    if (!reverse) {
      // support
      if (order.playerId == target.owner || state.players[order.playerId].teamId == state.players[target.owner].teamId) {
        target.army += amount;
      }
      // attack holded
      else if (target.amount > amount) {
        target.army -= amount;
      }
      // conquered
      else {
        target.army = amount - target.army;

        if (!order.originOwnerId) order.originOwnerId = target.owner;
        target.owner = order.playerId;
      }
    }
    else {
      // conquered
      if (order.originOwnerId) {
        target.army = amount - target.army;
        target.owner = order.originOwnerId;
      }
      // support
      else if (order.playerId == target.owner || state.players[order.playerId].teamId == state.players[target.owner].teamId) {
        target.army -= amount;
      }
      // attack holded
      else {
        target.army += amount;
      }
    }
  });
}
function createMove(state, turn, met, reverse) {
  // create
  Object.keys(turn).forEach(orderId => {
    const order = turn[orderId];
    const [sourceId, targetId] = orderId.split('-');
    const start = state.cities[reverse ? targetId : sourceId];
    const end = state.cities[reverse ? sourceId : targetId];
    const size1 = reverse ? met[orderId] || order.amount : order.amount;
    const size2 = reverse ? order.amount : met[orderId] || order.amount;

    state.move.armies.push({
      startX: start.x,
      startY: start.y,
      endX: end.x,
      endY: end.y,
      size1: size1,
      size2: size2
    });
  });
}
function sleep(ms) {
  return new Promise(resolve => setTimeout(resolve, ms));
}