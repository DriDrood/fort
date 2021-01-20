<template>
  <div class="controls">
    <button v-if="stateKey == 'Ready'" @click="start" title="Game start"><i class="fas fa-play"></i></button>
    <button v-if="stateKey == 'Running'" @click="pause" title="Pause"><i class="fas fa-pause"></i></button>
    <button v-if="stateKey == 'Paused'" @click="resume" title="Resume"><i class="fas fa-play"></i></button>
    <button v-if="stateKey == 'Running'" @click="end" title="End turn"><i class="fas fa-stop"></i></button>
    <button @click="resetGame" title="Reset game"><i class="fas fa-redo"></i></button>
  </div>
</template>

<script>
export default {
  name: "Controls",
  computed: {
    stateKey() {
      return this.$store.state.lifecycle.state.key;
    }
  },
  methods: {
    start() {
      this.$store.dispatch("commSend", { route: "admin/startGame" });
    },
    pause() {
      this.$store.dispatch("commSend", { route: "admin/pauseTurn" });
    },
    resume() {
      this.$store.dispatch("commSend", { route: "admin/resumeTurn" });
    },
    end() {
      this.$store.dispatch("commSend", { route: "admin/endTurn" });
    },
    resetGame() {
      if (!confirm("You are going to reset whole game. Are you sure?"))
        return;

      this.$store.dispatch("commSend", { route: "admin/resetGame" });
    },
  },
};
</script>

<style lang="sass">
@import ../_sass/_constants

.controls
  position: fixed
  top: $topPanel-height
  left: 0
  background: linear-gradient(170deg, #6f99acff 0%, #6f99ac60 100%)
  button
    margin: 5px 10px
</style>
