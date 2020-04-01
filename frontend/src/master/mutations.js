const mutations = {
  init(state, payload) { //
    state.cities = payload.cities;
    state.roads = payload.roads;
    state.players = payload.players;
    state.teams = payload.teams;
    state.config = payload.config;

    // turns
    state.activeTurnId = payload.currentTurn.id; 
    state.currentTurn.endsAt = payload.currentTurn.endsAt
    state.turns = [];
    for (let i = 0; i < payload.currentTurn.id; i++) {
      state.turns.push(null);
    }
    state.turns.push(payload.currentTurn.turn);
  },
  generateGuid: () => `${mutations.s4()}${mutations.s4()}-${mutations.s4()}-4${mutations.s4().substr(0, 3)}-${mutations.s4()}-${mutations.s4()}${mutations.s4()}${mutations.s4()}`.toLowerCase(),
  s4: () => (((1 + Math.random()) * 0x10000) | 0).toString(16).substring(1)
};
export default mutations;
