import Vue from 'vue'
import App from './App.vue'
import store from './store'

Vue.config.productionTip = false

new Vue({
  store,
  render: h => h(App),
  created() {
    this.$store.dispatch('lifecycleInit');
    this.$store.dispatch('mapInit');
    this.$store.dispatch('notifyInit');
    this.$store.dispatch('turnsInit');
    this.$store.dispatch('userInit');

    this.$store.dispatch('masterInit');
  }
}).$mount('#app')
