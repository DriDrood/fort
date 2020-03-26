import Vue from 'vue'
import Vuex from 'vuex'

Vue.use(Vuex)

export default new Vuex.Store({
  state: {
    login: {
      id: '5',
      name: 'Veverka'
    },
    currentTurn: {
      activeId: 1,
      endsAt: new Date(2021, 1, 1),
      remains: '-:--',
      done: false
    },
    turns: [
      {
        cityOccupation: {
          '1': {
            playerId: '4',
            size: 50
          },
          '2': {
            playerId: '4',
            size: 15
          },
          '3': {
            playerId: '4',
            size: 12
          },
          '4': {
            playerId: '4',
            size: 12
          }
        },
        orders: {
          '4>>3': {
            playerId: '4',
            amount: 10,
            size: 8
          },
          '3>>4': {
            playerId: '5',
            amount: 5,
            size: 7
          }
        }
      },
      {
        cityOccupation: {
          '1': {
            playerId: '3',
            size: 15
          },
          '2': {
            playerId: '4',
            size: 15
          },
          '3': {
            playerId: '5',
            size: 12,
            army: 20
          },
          '4': {
            playerId: '4',
            size: 24
          }
        },
        orders: {}
      }
    ],

    staticData: {
      cities: {
        '1': {
          id: '1',
          x: 500,
          y: 500
        },
        '2': {
          id: '2',
          x: 400,
          y: 600
        },
        '3': {
          id: '3',
          x: 300,
          y: 400
        },
        '4': {
          id: '4',
          x: 400,
          y: 370
        }
      },
      roads: {
        '1': ['2', '3'],
        '2': ['1'],
        '3': ['1', '4'],
        '4': ['3']
      },
      players: {
        '3': {
          name: 'uuu',
          teamId: '3'
        },
        '4': {
          name: 'enemy',
          teamId: '2'
        },
        '5': {
          name: 'hello',
          teamId: '1'
        }
      },
      teams: {
        '1': { color: '#83824b', light: '#c4c498' },
        '2': { color: '#52834b', light: '#9dc498' },
        '3': { color: '#4b7183', light: '#98b6c4' }
      },
      config: {
        armyRunDuration: 0.4
      }
    },
    moveRun: {
      armies: [],
      armiesPosition: 0
    }
  },
  getters: {
    isTurnCurrent: (state) => state.currentTurn.activeId == state.turns.length - 1,
    currentTurn: (state) => state.turns[state.currentTurn.activeId],
    distinctRoads: (state) => {
      let result = [];
      Object.keys(state.staticData.roads).forEach(id => {
        const sourceId = parseInt(id);
        const targetIds = state.staticData.roads[sourceId];
        targetIds.forEach(targetId => {
          if (sourceId < targetId)
            result.push({ source: state.staticData.cities[sourceId], target: state.staticData.cities[targetId] });
        });
      });
      return result;
    }
  },
  mutations: {
    toggleDone: (state) => state.currentTurn.done = !state.currentTurn.done,
    prevTurn: async (state) => {
      // invalid command
      if (state.currentTurn.activeId <= 0 || state.moveRun.armiesPosition != 0) return;

      // init
      const orders = state.turns[state.currentTurn.activeId - 1].orders;
      var met = meetings(state, orders);
      createMove(state, orders, met, true);

      // move
      state.moveRun.armiesPosition = 1;
      await sleep(state.staticData.config.armyRunDuration * 1000);
      state.moveRun.armiesPosition = 2;
      await sleep(state.staticData.config.armyRunDuration * 1000);

      // decrease active
      state.currentTurn.activeId -= 1;

      // clean
      Vue.set(state.moveRun, 'armies', []);
      state.moveRun.armiesPosition = 0;
    },
    nextTurn: async (state) => {
      // invalid command
      if (state.currentTurn.activeId >= state.turns.last || state.moveRun.armiesPosition != 0) return;

      // init
      const orders = state.turns[state.currentTurn.activeId].orders;
      let met = meetings(state, orders);
      createMove(state, orders, met, false);
      
      // move
      state.moveRun.armiesPosition = 1;
      await sleep(state.staticData.config.armyRunDuration * 1000);
      state.moveRun.armiesPosition = 2;
      await sleep(state.staticData.config.armyRunDuration * 1000);

      // increase active
      state.currentTurn.activeId += 1;

      // clean
      Vue.set(state.moveRun, 'armies', []);
      state.moveRun.armiesPosition = 0;
    },
    order(state, payload) { // sourceId, targetId, amount, sourceCityRemains
      var currentTurn = state.turns[state.currentTurn.activeId];
      const source = currentTurn.cityOccupation[payload.sourceId];
      if (source.playerId != state.login.id) return;
      if (payload.max < payload.amount) return;

      const orderKey = `${payload.sourceId}>>${payload.targetId}`;
      if (payload.amount > 0)
        Vue.set(currentTurn.orders, orderKey, { playerId: state.login.id, amount: payload.amount, size: getSize(payload.amount) });
      else if (currentTurn.orders[orderKey])
        Vue.delete(currentTurn.orders, orderKey);
      // else nothing

      source.availableArmy = payload.max - payload.amount;
    },
    countDown: (state) => {
      setTimeout(() =>
        setInterval(() => {
          if (state.currentTurn.endsAt) {
            const remainsDate = new Date(state.currentTurn.endsAt - new Date());
            state.currentTurn.remains = `${remainsDate.getMinutes()}:${remainsDate.getSeconds().toString().padStart(2, '0')}`;
          }
          else {
            state.turn.remains = '-:--';
          }
        }, 1000), new Date(state.currentTurn.endsAt - new Date()).getMilliseconds());
    }
  }
});

function meetings(state, orders) {
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
    if (!reverseOrder || state.staticData.players[order.playerId].teamId == state.staticData.players[reverseOrder.playerId].teamId) return;

    // meeting ;-)
    // first win
    if (order.size > reverseOrder.size)
    {
      met[orderId] = meetingSize(order.size, reverseOrder.size);
      met[reverseOrderId] = 0;
    }
    // second win
    else if (order.size < reverseOrder.size)
    {
      met[orderId] = 0; 
      met[reverseOrderId] = meetingSize(reverseOrder.size, order.size);
    }
    // same size
    else
    {
      met[orderId] = 5; 
      met[reverseOrderId] = 5;
    }
  });

  return met;
}
function createMove(state, orders, met, reverse) {
  // create
  Object.keys(orders).forEach(orderId => {
    // init
    const order = orders[orderId];
    const [sourceId, targetId] = orderId.split('>>');
    // get positions
    const start = state.staticData.cities[reverse ? targetId : sourceId];
    const end = state.staticData.cities[reverse ? sourceId : targetId];
    // get sizes
    const size1 = reverse
      ? met[orderId] || order.size
      : order.size;
    const size2 = reverse
      ? order.size
      : met[orderId] || order.size;

    // return
    state.moveRun.armies.push({
      startX: start.x,
      startY: start.y,
      endX: end.x,
      endY: end.y,
      size1: size1,
      size2: size2,
      playerId: order.playerId
    });
  });
}
function meetingSize(biggerArmySize, smallerArmySize)
{
  return Math.floor(Math.sqrt(Math.pow(biggerArmySize - 5, 2) - Math.pow(smallerArmySize - 5, 2))) + 5;
}
function getSize(army)
{
  if (!army)
    return null;

  return Math.floor(Math.sqrt(army)) + 5;
}
function sleep(ms) {
  return new Promise(resolve => setTimeout(resolve, ms));
}