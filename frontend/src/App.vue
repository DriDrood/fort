<template>
  <div id="app">
    <login v-if="!login" />
    <template v-else>
      <topPanel />
      <worldMap />
    </template>
  </div>
</template>

<script>
import login from "./master/login";
import topPanel from "./master/top-panel";
import worldMap from "./map/map";
import { mapState } from 'vuex';

export default {
  name: "app",
  components: {
    login,
    topPanel,
    worldMap
  },
  computed: {
    ...mapState(['login'])
  },
  mounted() {
    this.$store.commit('countDown');
  }
};
</script>

<style lang="sass">
// reset
*
  margin: 0
  padding: 0

  border: none
  border-radius: 0
  outline: none
  color: inherit
  background: none

  text-decoration: none
  font-family: 'Nunito', sans-serif
  font-weight: inherit
  font-size: inherit
  line-height: 1
input::placeholder
  text-align: center
h1, h2, h3
  font-weight: bold
h1
  font-size: 2rem
h2
  font-size: 1.5rem
h3
  font-size: 1.2rem
// base
html
  width: 100%
  height: 100%
body
  width: 100%
  height: 100%

  color: #241f16
  background-color: #000
  font-weight: 300
#app
  width: 100%
  height: 100%
</style>