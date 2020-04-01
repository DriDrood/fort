import Vue from 'vue';
import Vuex from 'vuex';
import VueResource from 'vue-resource';

import commState from './comm/state';
import lifecycleState from './lifecycle/state';
import mapState from './map/state';
import masterState from './master/state';
import notifyState from './notify/state';
import turnsState from './turns/state';
import usersState from './users/state';

import commGetters from './comm/getters';
import mapGetters from './map/getters';
import turnsGetters from './turns/getters';

import commMutations from './comm/mutations';
import lifecycleMutations from './lifecycle/mutations';
import masterMutations from './master/mutations';
import notifyMutations from './notify/mutations';
import turnsMutations from './turns/mutations';
import usersMutations from './users/mutations';

import turnsActions from './turns/actions';
import usersActions from './users/actions';

Vue.use(Vuex);
Vue.use(VueResource);

export default new Vuex.Store({
  state: {
    ...masterState,
    ...lifecycleState,
    ...notifyState,
    ...turnsState,
    ...mapState,
    ...usersState,
    ...commState
  },
  getters: {
    ...commGetters,
    ...mapGetters,
    ...turnsGetters
  },
  mutations: {
    ...commMutations,
    ...lifecycleMutations,
    ...masterMutations,
    ...notifyMutations,
    ...turnsMutations,
    ...usersMutations
  },
  actions: {
    ...turnsActions,
    ...usersActions
  }
});
