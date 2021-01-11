import Vue from 'vue';
import master from "./master";

export default {
  state: () => ({
    connection: null,
    isOpened: false,
    loading: {},
  }),
  getters: {
    isLoading: state => Object.keys(state.loading).length != 0,
  },
  mutations: {
    // messageId
    commSendMessage: (state, payload) => {
      Vue.set(state.loading, payload.messageId, true);
    },
    // messageId
    commReceiveMessage: (state, payload) => {
      if (!state.loading[payload.messageId])
        return;
        
      Vue.delete(state.loading, payload.messageId);
    }
  },
  actions: {
    // callback
    commInit: (context, payload) => {
      context.state.connection = new WebSocket("ws://localhost:5000/ws");
      context.state.connection.onmessage = event => context.dispatch("commMessageReceive", { event });
      context.state.connection.onopen = event => context.dispatch("commConnectionOpened", { event, callback: payload && payload.callback });
      context.state.connection.onclose = event => context.dispatch("commConnectionClosed", { event });
      context.state.connection.onerror = event => context.dispatch("commOnError", { event });
    },
    // route, data
    commSend: (context, payload) => {
      payload.messageId = master.getters.generateGuid();
      payload.jwtToken = context.rootState.user.jwtToken;
      const payloadString = JSON.stringify(payload);

      if (!context.state.isOpened)
        context.dispatch("commInit", { callback: () => { context.commit("commSendMessage", payload); context.state.connection.send(payloadString); } });
        
      else {
        context.commit("commSendMessage", payload);
        context.state.connection.send(payloadString);
      }
    },
    
    // event
    commMessageReceive: (context, payload) => {
      const data = JSON.parse(payload.event.data);
      console.log("received message", data);
      context.commit("commReceiveMessage", data);
      switch (data.route) {
        case "error":
          // context.commit("notificationsCreate", {
          //   type: "error",
          //   text: data.data.Message
          // });
          break;
        case "player/login":
          context.commit("userLogged", data.data);
          context.commit("masterInit", data.data);
          context.commit("lifecycleUpdateCurrentTurn", data.data);
          context.commit("mapInit", data.data);
          context.commit("turnsUpdate", data.data);
          context.commit("userUpdate", data.data);
          break;
        case "player/init":
          context.commit("masterInit", data.data);
          context.commit("lifecycleUpdateCurrentTurn", data.data);
          context.commit("mapInit", data.data);
          context.commit("turnsUpdate", data.data);
          context.commit("userUpdate", data.data);
          break;
        case "player/setTurnClosed":
          context.commit("lifecycleToggleClose", data.data);
          break;
      }
    },
    // event, callback
    commConnectionOpened: (context, payload) => {
      context.state.isOpened = true;
      console.log("Connected", payload.event);

      if (payload.callback)
        payload.callback();
    },
    // event
    commConnectionClosed: (context, payload) => {
      context.state.isOpened = false;
      console.log("Disconnected", payload.event);
    },
    // event
    commOnError: (context, payload) => {
      console.log("Error", payload);
    },
  },
}
