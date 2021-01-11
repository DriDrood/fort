import master from "./master";

export default {
  state: () => ({
    data: {
      // '167b0591-937c-46a2-a967-f90c525c2134': {
      //   id: '167b0591-937c-46a2-a967-f90c525c2134',
      //   text: 'Hello',
      //   level: 'info',
      //   permanent: true
      // },
      // 'bccdc555-1156-4055-8ba7-3fd32cef4a6c': {
      //   id: 'bccdc555-1156-4055-8ba7-3fd32cef4a6c',
      //   text: 'tohle ti nemůžu dovolit',
      //   level: 'warning',
      //   permanent: true
      // },
      // '74b84570-d89f-4ab1-a9cd-faf127b029de': {
      //   id: '74b84570-d89f-4ab1-a9cd-faf127b029de',
      //   text: 'Spojení se serverem nenavázáno',
      //   level: 'error',
      //   permanent: true
      // }
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
    }
  },
  actions: {
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
    }
  }
};
