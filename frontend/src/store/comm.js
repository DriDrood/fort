import Vue from 'vue';
import master from "./master";

export default {
  state: () => ({
    connection: null,
    isOpened: false,
    requests: {},
    receivers: {
      // 'user/login': { userInit: "mutation", turnsInit: "action" },
    },
  }),
  getters: {
    isLoading: state => Object.keys(state.requests).length != 0,
  },
  mutations: {
    // messageId, callback = { "route": "mutation/action" }
    commSendMessage: (state, payload) => {
      Vue.set(state.requests, payload.messageId, payload.callback || null);
    },
    // messageId
    commReceiveMessage: (state, payload) => {
      Vue.delete(state.requests, payload.messageId);
    },
    // route, callback, callbackType
    commRegisterReceiver: (state, payload) => {
      const type = payload.callbackType || "mutation";

      if (!state.receivers[payload.route])
        Vue.set(state.receivers, payload.route, {});

      state.receivers[payload.route][payload.callback] = type;
    },
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
    // route, data, callback
    commSend: (context, payload) => {
      // open connection if not
      if (!context.state.isOpened) {
        context.dispatch("commInit", { callback: () => context.dispatch("commSend", payload) });
        return;
      }

      // prepare message
      payload.messageId = master.getters.generateGuid();
      payload.jwtToken = context.rootState.user.login.jwtToken;
      const payloadString = JSON.stringify(payload);
      console.log("Sending message", payload);

      // send
      context.commit("commSendMessage", payload);
      context.state.connection.send(payloadString);
    },
    
    // event
    commMessageReceive: (context, payload) => {
      // get data
      const data = JSON.parse(payload.event.data);
      console.log("Received message", data);

      // get callbacks
      var callbacks = context.state.requests[data.messageId] || context.state.receivers[data.route];

      // remove loading status
      context.commit("commReceiveMessage", data);

      // no receiver
      if (!callbacks)
        return;

      // run all callbacks
      Object.keys(callbacks).forEach(route => {
        switch (callbacks[route]) {
          case "mutation":
            context.commit(route, data.data);
            break;
          case "action":
            context.dispatch(route, data.data);
            break;
          default:
            console.log("error - unknown callback type", route, callbacks[route]);
        }
      });
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
