import Vue from 'vue';
import masterState from '../master/state';
import masterMutations from '../master/mutations';

const mutations = {
  notify: (state, payload) => { // level, text, permanent
    // create
    const guid = masterMutations.generateGuid();
    const newNotification = {
      id: guid,
      text: payload.text,
      level: payload.level,
      permanent: payload.permanent
    };

    // set
    Vue.set(state.notifications, guid, newNotification);

    // time destroy
    if (!payload.permanent) {
      setTimeout(() => mutations.unNotify(state, { id: guid }), masterState.config.notificationDuration * 1000);
    }
  },
  unNotify: (state, payload) => { // id
    if (!state.notifications[payload.id]) return;

    Vue.delete(state.notifications, payload.id);
  }
};
export default mutations;
