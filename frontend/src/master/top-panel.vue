<template>
  <nav class="top">
    <img :src="`/users/${login.id}.jpg`" :alt="login.name" :title="login.name" />
    <div v-if="loading" class="loading">
      <i class="fas fa-spin fa-circle-notch"></i>
    </div>
    <button v-if="activeTurnId > 0" class="prev" title="Vrátit o kolo" @click="prevTurn">
      <i class="fa fa-step-backward"></i>
    </button>
    <div class="turn">{{ activeTurnId + 1 }}</div>
    <button v-if="!isTurnCurrent" class="next" title="Vpřed o kolo" @click="nextTurn">
      <i class="fa fa-step-forward"></i>
    </button>
    <button class="done" :class="{active: currentTurn.done}" title="Hotovo" @click="toggleDone">
      <i class="fa fa-check"></i>
    </button>
    <div class="time">{{ currentTurn.remains }}</div>
  </nav>
</template>

<script>
import { mapState, mapGetters } from "vuex";

export default {
  name: "topPanel",
  computed: {
    ...mapState(["activeTurnId", "currentTurn", "login"]),
    ...mapGetters(["isTurnCurrent", "loading"])
  },
  methods: {
    toggleDone() {
      this.$store.dispatch("toggleDone");
    },
    prevTurn() {
      this.$store.dispatch("prevTurn");
    },
    nextTurn() {
      this.$store.dispatch("nextTurn");
    }
  }
};
</script>

<style lang="sass">
@import ../_sass/_constants

nav.top
  position: fixed
  z-index: 3
  display: grid
  width: 100%
  height: $topPanel-height

  background: linear-gradient(170deg, #6f99acff 0%, #6f99ac60 100%)
  font-size: 2rem
  font-weight: bold

  grid-template-areas: "prev turn next avatar done time"
  grid-template-columns: 2fr 2fr 2fr 5fr 2fr 4fr
  justify-items: center
  align-items: center

  img
    grid-area: avatar
    height: 7rem
    width: 7rem
    margin: -20% 0
    border: 5px solid #241f16
    border-radius: 50%
  .loading
    grid-area: avatar
    height: 7rem
    width: 7rem
    margin: -20% 0
    background-color: #00000060
    border-radius: 50%
    z-index: 3

    display: grid
    justify-items: center
    align-items: center
    color: #fff
  .prev
    grid-area: prev
  .next
    grid-area: next
  .turn
    grid-area: turn
  .done
    grid-area: done
    &.active
      color: #fff
  .time
    grid-area: time
</style>