import Vue from "vue";
import Vuex from "vuex";
import LocalStorage from "vue-localstorage";

import comm from "./comm";
import lifecycle from "./lifecycle";
import map from "./map";
import master from "./master";
import notify from "./notify";
import turns from "./turns";
import user from "./user";

Vue.use(Vuex);
Vue.use(LocalStorage, { name: "ls", bind: true });

export default new Vuex.Store({
  state: {},
  getters: {},
  mutations: {},
  actions: {},
  modules: {
    comm,
    lifecycle,
    map,
    master,
    notify,
    turns,
    user,
  }
});
