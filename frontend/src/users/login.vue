<template>
  <div class="login">
    <input type="text" placeholder="Jméno" class="username" v-model="username">
    <input type="password" placeholder="Heslo" class="password" v-model="password">
    <button @click="login">
      <i v-if="loading" class="fas fa-spin fa-circle-notch"></i>
      <i v-else class="fas fa-sign-in-alt" title="login"></i>
    </button>
  </div>
</template>

<script>
import { mapGetters } from 'vuex';

export default {
  name: 'login',
  data: () => ({
    username: null,
    password: null
  }),
  computed: {
    ...mapGetters(["loading"])
  },
  methods: {
    login() {
      this.$store.dispatch('login', {
        username: this.username,
        password: this.password
      })
    }
  }
}
</script>

<style lang="sass">
.login
  display: grid
  width: 100%
  height: 100%
  grid-template-areas: "username login" "password login"
  grid-row: 1fr auto auto 1fr

  overflow: hidden
  background:
    image: url('../../public/world.jpg')
    size: cover
  input
    height: 1rem
    width: 10rem
    padding: .5rem .7rem
    background-color: white
    &.username
      grid-area: username
      justify-self: end
      align-self: end
      border-radius: 1rem 0 0 0
    &.password
      grid-area: password
      justify-self: end
      align-self: start
      border-radius: 0 0 0 1rem
  button
    grid-area: login
    justify-self: start
    align-self: center
    width: 5rem
    padding: 1.5rem
    background: linear-gradient(150deg, #1f6384 0%, #6093abb5 100%)
    border-radius: 0 1rem 1rem 0
</style>