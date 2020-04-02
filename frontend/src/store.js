import Vue from 'vue';
import Vuex from 'vuex';
import VueResource from 'vue-resource';
import LocalStorage from 'vue-localstorage';

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

import lifecycleActions from './lifecycle/actions';
import masterActions from './master/actions';
import turnsActions from './turns/actions';
import usersActions from './users/actions';

Vue.use(Vuex);
Vue.use(VueResource);
Vue.use(LocalStorage, { name: 'ls', bind: true });

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
    ...lifecycleActions,
    ...masterActions,
    ...turnsActions,
    ...usersActions
  }
});
