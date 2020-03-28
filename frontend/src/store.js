import Vue from 'vue'
import Vuex from 'vuex'

import lifecycleState from './lifecycle/state';
import mapState from './map/state';
import masterState from './master/state';
import notifyState from './notify/state';
import turnsState from './turns/state';
import usersState from './users/state';

import turnsGetters from './turns/getters';

import lifecycleMutations from './lifecycle/mutations';
import notifyMutations from './notify/mutations';
import turnsMutations from './turns/mutations';
import usersMutations from './users/mutations';

Vue.use(Vuex)

export default new Vuex.Store({
  state: {
    ...masterState,
    ...lifecycleState,
    ...notifyState,
    ...turnsState,
    ...mapState,
    ...usersState
  },
  getters: {
    ...turnsGetters
  },
  mutations: {
    ...lifecycleMutations,
    ...notifyMutations,
    ...turnsMutations,
    ...usersMutations
  }
});
