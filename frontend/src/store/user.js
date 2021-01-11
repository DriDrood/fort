import Vue from "vue";

export default {
  state: () => ({
    login: {
      id: null,
      name: null,
      jwtToken: null
    },
    players: {
      // '3': {
      //   name: 'uuu',
      //   teamId: '3'
      // },
      // '4': {
      //   name: 'enemy',
      //   teamId: '2'
      // },
      // '5': {
      //   name: 'hello',
      //   teamId: '1'
      // }
    },
    teams: {
      // '1': { color: '#83824b', colorLight: '#c4c498' },
      // '2': { color: '#52834b', colorLight: '#9dc498' },
      // '3': { color: '#4b7183', colorLight: '#98b6c4' }
    }
  }),
  getters: {
    userIsLogged: state => state.login.jwtToken != null
  },
  mutations: {
    userInit(state) {
      if (Vue.ls.get("jwtToken") != null)
      {
        state.login.id = Vue.ls.get("id");
        state.login.name = Vue.ls.get("name");
        state.login.jwtToken = Vue.ls.get("jwtToken");
      }
    },
    userLogged(state, payload) {
      state.login = payload.login;

      Vue.ls.set("id", payload.login.id);
      Vue.ls.set("name", payload.login.name);
      Vue.ls.set("jwtToken", payload.login.jwtToken);
    },
    userUpdate(state, payload) {
      state.players = payload.players;
      state.teams = payload.teams;
    },
    userLogout(state) {
      state.login = {
        id: null,
        name: null,
        jwtToken: null
      };
    }
  },
  actions: {
    // email, password
    userLogin(context, payload) {
      context.dispatch("commSend", { route: "player/login", data: payload });
    },
    userLogout(context) {
      context.commit("userLogout");
    }
  }
};
