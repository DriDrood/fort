import Vue from 'vue';
import turnsGetters from './getters';

export default {
  updateTurn(state, payload) {
    // add empty turns
    for (let i = state.turns.length; i < state.currentTurn.id; i++) {
      state.turns.push(null);
    }
    
    // add current turn
    Vue.set(state.turns, state.currentTurn.id, payload.currentTurn.turn);
  },
  updateOrder(state, payload) { // sourceId, targetId, amount
    const currentTurn = turnsGetters.activeTurn(state);
    const source = currentTurn.cityOccupations[payload.sourceId];
    if (source.playerId != state.login.id) return;

    const orderKey = `${payload.sourceId}>>${payload.targetId}`;
    const max = ((currentTurn.orders[orderKey] && currentTurn.orders[orderKey].amount) || 0) + source.availableArmy;
    if (payload.amount > 0)
      Vue.set(currentTurn.orders, orderKey, { playerId: state.login.id, amount: payload.amount, size: helpers.getArmySize(payload.amount) });
    else if (currentTurn.orders[orderKey])
      Vue.delete(currentTurn.orders, orderKey);
    // else nothing

    source.availableArmy = max - payload.amount;
  },
  updatePrevTurn: async (state) => {
    // invalid command
    if (state.activeTurnId <= 0 || state.turnRun.armiesPosition != 0) return;

    // init
    const orders = state.turns[state.activeTurnId - 1].orders;
    var met = helpers.meetingResults(state, orders);
    helpers.createMove(state, orders, met, true);

    // move
    await helpers.sleep(10);
    state.turnRun.armiesPosition = 1;
    await helpers.sleep(state.config.armyRunDuration * 1000);
    state.turnRun.armiesPosition = 2;
    await helpers.sleep(state.config.armyRunDuration * 1000);

    // decrease active
    state.activeTurnId -= 1;

    // clean
    Vue.set(state.turnRun, 'armies', []);
    state.turnRun.armiesPosition = 0;
  },
  updateNextTurn: async (state) => {
    // invalid command
    if (state.activeTurnId >= state.turns.last || state.turnRun.armiesPosition != 0) return;

    // init
    const orders = turnsGetters.activeTurn(state).orders;
    let met = helpers.meetingResults(state, orders);
    helpers.createMove(state, orders, met, false);

    // move
    await helpers.sleep(10);
    state.turnRun.armiesPosition = 1;
    await helpers.sleep(state.config.armyRunDuration * 1000);
    state.turnRun.armiesPosition = 2;
    await helpers.sleep(state.config.armyRunDuration * 1000);

    // increase active
    state.activeTurnId += 1;

    // clean
    Vue.set(state.turnRun, 'armies', []);
    state.turnRun.armiesPosition = 0;
  }
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
      state.turnRun.armies.push({
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