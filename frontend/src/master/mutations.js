import lifecycleMutations from '../lifecycle/mutations';
import mapMutations from '../map/mutations';
import turnMutations from '../turns/mutations';
import userMutations from '../users/mutations';

const mutations = {
  countDownPeriodicaly: (state) => {
    setTimeout(() =>
      setInterval(() => {
        if (state.currentTurn.endsAt) {
          const remainsDate = new Date(state.currentTurn.endsAt - new Date());
          state.currentTurn.remains = `${remainsDate.getMinutes()}:${remainsDate.getSeconds().toString().padStart(2, '0')}`;
        }
        else {
          state.currentTurn.remains = '-:--';
        }
      }, 1000), new Date(state.currentTurn.endsAt - new Date()).getMilliseconds());
  },
  updateInit(state, payload) { //
    state.config = payload.config;
    mapMutations.updateMap(state, payload);
    userMutations.updateUsers(state, payload);
    lifecycleMutations.updateCurrentTurn(state, payload);
    turnMutations.updateTurn(state, payload);
  },
  updateDone: (state, payload) => state.currentTurn.done = payload.done,
  generateGuid: () => `${helpers.s4()}${helpers.s4()}-${helpers.s4()}-4${helpers.s4().substr(0, 3)}-${helpers.s4()}-${helpers.s4()}${helpers.s4()}${helpers.s4()}`.toLowerCase()
};
export default mutations;

const helpers = {
  s4: () => (((1 + Math.random()) * 0x10000) | 0).toString(16).substring(1)
}