import Vue from 'vue'
import Vuex from 'vuex'

Vue.use(Vuex)

export default new Vuex.Store({
  state: {
    login: {
      name: 'hello'
    },
    cities: {
      1: {
        id: 1,
        x: 500,
        y: 500,
        army: 11
      },
      2: {
        id: 2,
        x: 400,
        y: 600,
        army: 12
      },
      3: {
        id: 3,
        x: 300,
        y: 400,
        army: 13
      },
      4: {
        id: 4,
        x: 400,
        y: 370,
        army: 14
      }
    },
    roads: {
      1: [ 2, 3 ],
      2: [ 1 ],
      3: [ 1, 4 ],
      4: [ 3 ]
    }
  },
  getters: {
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
  }
});