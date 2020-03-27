import Vue from 'vue';
import masterState from '../master/state';
import masterMutations from '../master/mutations';

const mutations = {
  notify: (state, payload) => { // level, text, timeDestroy
    // create
    const guid = masterMutations.generateGuid();
    const newNotification = {
      id: guid,
      text: payload.text,
      level: payload.level,
      timeDestroy: payload.timeDestroy
    };

    // set
    Vue.set(state.notifications, guid, newNotification);

    // time destroy
    if (payload.timeDestroy) {
      setTimeout(() => mutations.unNotify(state, { id: guid }), masterState.config.notificationDuration * 1000);
    }
  },
  unNotify: (state, payload) => { // id
    if (!state.notifications[payload.id]) return;

    Vue.delete(state.notifications, payload.id);
  }
};
export default mutations;
