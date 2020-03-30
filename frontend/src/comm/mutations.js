import Vue from 'vue';

export default {
  addSpinner(state, payload) { // requestId
    Vue.set(state.requests, payload, true);
  },
  removeSpinner(state, payload) { // requestId
    Vue.delete(state.requests, payload);
  }
}