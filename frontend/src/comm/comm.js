import Vue from 'vue';
import masterMutations from '../master/mutations';

const actions = {
  post(path, data, context, callback) {
    var requestId = masterMutations.generateGuid();

    helpers.addSpinner(context.state, requestId);
    Vue.http.post(`${helpers.getUrl()}/api/${path}`, data, { headers: { Authorization: "Bearer " + context.state.login.jwtToken } })
      .finally(() => helpers.removeSpinner(context.state, requestId))
      .then(resp => callback(resp.body))
      .catch((err) => {
        switch (err.status) {
          // not modified - ignore
          case 304:
            break;
          // not authorized - logout
          case 401:
            context.commit("logout", true);
            break;
          // other error - log, show
          default:
            helpers.error(context, err);
            break;
        }
      });
  },
  get(path, context, callback) {
    var requestId = masterMutations.generateGuid();

    helpers.addSpinner(context.state, requestId);
    Vue.http.get(helpers.getUrl() + path, { headers: { Authorization: "Bearer " + context.state.login.jwtToken } })
      .finally(() => helpers.removeSpinner(context.state, requestId))
      .then(resp => callback(resp.body))
      .catch((err) => {
        switch (err.status) {
          // not modified - ignore
          case 304:
            break;
          // not authorized - logout
          case 401:
            context.commit("logout", true);
            break;
          // other error - log, show
          default:
            helpers.error(context, err);
            break;
        }
      });
  }
};
export default actions;

const helpers = {
  addSpinner(state, requestId) {
    Vue.set(state.requests, requestId, true);
  },
  removeSpinner(state, requestId) {
    Vue.delete(state.requests, requestId);
  },
  error(context, err) {
    // console.log(err);
    let text = err;
    if (err.body != null)
      text = err.body
    if (err.status == 0)
      text = "Nepodařilo se připojit k serveru. Jste připojen k Internetu?"
    if (err.status == 401)
      text = "Neoprávněný přístup. Zkuste se odhlásit a znovu přihlásit."
    context.commit("notify", {
      text: text,
      level: "error"
    });
  },
  getUrl: () => window.location.href.split('/').slice(0, 3).join('/')
}