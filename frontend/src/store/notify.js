import Vue from "vue";
import master from "./master";

export default {
  state: () => ({
    data: {
      // '167b0591-937c-46a2-a967-f90c525c2134': {
      //   id: '167b0591-937c-46a2-a967-f90c525c2134',
      //   text: 'Hello',
      //   level: 'info', // warning, error
      //   permanent: true
      // },
    }
  }),
  mutations: {
    // id, notification
    notifyCreate: (state, payload) => {
      Vue.set(state.data, payload.id, payload.notification);
    },
    // id
    notifyClose: (state, payload) => {
      if (!state.notifications[payload.id]) return;
  
      Vue.delete(state.notifications, payload.id);
    },
  },
  actions: {
    notifyInit: (context) => {
      context.commit("commRegisterReceiver", { route: "error", callback: "notifyError", callbackType: "action" });
    },
    // level, text, permanent
    notifyCreate: (context, payload) => {
      // create
      const guid = master.getters.generateGuid();
      const newNotification = {
        id: guid,
        text: payload.text,
        level: payload.level,
        permanent: payload.permanent
      };
  
      // set
      context.commit("notifyCreate", { id: guid, notification: newNotification });
  
      // time destroy
      if (!payload.permanent) {
        setTimeout(() => context.commit("notifyClose", { id: guid }), context.rootState.master.config.notificationDuration * 1000);
      }
    },
    // id
    notifyClose: (context, payload) => { 
      context.commit("notifyClose", payload);
    },
    // message
    notifyError: (context, payload) => {
      context.dispatch("notifyCreate", {
        text: payload.message,
        level: "warning",
        permanent: false,
      });
    },
  }
};
