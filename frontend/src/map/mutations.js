const mutations = {
  updateMap(state, payload) {
    state.cities = payload.cities;
    state.roads = payload.roads;
  }
};
export default mutations;
