import Vue from 'vue';
import masterMutations from '../master/mutations';

const actions = {
  post(path, data, context, callback) {
    var requestId = masterMutations.generateGuid();
    context.commit('addSpinner', requestId);
    Vue.http.post(`${helpers.getUrl()}/api/${path}`, data, { headers: { Authorization: "Bearer " + context.state.login.jwtToken } })
      .finally(() => context.commit('removeSpinner', requestId ))
      .then(resp => callback(resp.body))
      .catch((err) => helpers.error(context, err));
  },
  get(path, context, callback) {
    var requestId = masterMutations.generateGuid();
    context.commit('addSpinner', requestId);
    Vue.http.get(helpers.getUrl() + path, { headers: { Authorization: "Bearer " + context.state.login.jwtToken } })
      .finally(() => context.commit('removeSpinner', requestId))
      .then(resp => callback(resp.body))
      .catch((err) => {
        if (err.status == 401)
          context.commit("logout", true);
        else
          helpers.error(context, err);
      });
  }
};
export default actions;

const helpers = {
  error(context, err) {
    console.log(err);
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