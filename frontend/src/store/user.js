import Vue from "vue";

export default {
  state: () => ({
    login: {
      id: null,
      name: null,
      role: null,
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
      // '3': { color: '#4b7183', colorLight: '#98b6c4' },
    }
  }),
  mutations: {
    userInit(state) {
      if (Vue.ls.get("jwtToken") != null)
      {
        state.login.id = Vue.ls.get("id");
        state.login.name = Vue.ls.get("name");
        state.login.role = Vue.ls.get("role");
        state.login.jwtToken = Vue.ls.get("jwtToken");
      }
    },
    userInitData(state, payload) {
      state.players = payload.players;
      state.teams = payload.teams;
    },
    userLogged(state, payload) {
      state.login = payload.login;
      state.players = payload.players;
      state.teams = payload.teams;

      Vue.ls.set("id", payload.login.id);
      Vue.ls.set("name", payload.login.name);
      Vue.ls.set("role", payload.login.role);
      Vue.ls.set("jwtToken", payload.login.jwtToken);
    },
    userLogout(state) {
      state.login = {
        id: null,
        name: null,
        role: null,
        jwtToken: null
      };
    }
  },
  actions: {
    userInit: context => {
      context.commit("userInit");

      context.commit("commRegisterReceiver", { route: "player/init", callback: "userInitData" });
      context.commit("commRegisterReceiver", { route: "player/login", callback: "userLogged" });
      context.commit("commRegisterReceiver", { route: "player/logout", callback: "userLogout" });
    },
    // email, password
    userLogin(context, payload) {
      context.dispatch("commSend", { route: "player/login", data: payload });
    },
    userLogout(context) {
      context.commit("userLogout");
    }
  }
};
