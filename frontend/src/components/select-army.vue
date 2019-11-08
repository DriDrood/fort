<template>
  <div class="modalContainer" @click="depClose">
    <div class="armySelect">
      <button class="close" @click="close">
        <i class="fa fa-times"></i>
      </button>
      <h2>Armáda</h2>
      <div class="count">
        <div class="amount">12</div>
        <button class="incr">
          <i class="fa fa-caret-up"></i>
        </button>
        <button class="decr">
          <i class="fa fa-caret-down"></i>
        </button>
      </div>
      <div class="selectRange">
        <div class="selectSlider"></div>
      </div>
      <button class="ok" @click="accept">OK</button>
    </div>
  </div>
</template>

<script>
export default {
  name: "selectArmy",
  props: {
    sourceId: {},
    targetId: {}
  },
  methods: {
    accept() {
      this.$store.commit('order', {
        sourceId: this.sourceId,
        targetId: this.targetId,
        amount: 8
      });
      this.$emit("close");
    },
    close() {
      this.$emit("close");
    },
    depClose(e) {
      if (!e.target.classList.contains('modalContainer')) return;
      this.close();
    }
  }
};
</script>

<style lang="sass">

.modalContainer
  position: absolute
  display: grid
  top: 0
  width: 100%
  height: 100%

  background: #00000088
  z-index: 2

  justify-items: center
  align-items: center

  .armySelect
    display: grid
    width: 80%
    height: 30%
    min-width: 20rem
    min-height: 10rem
    padding: 2rem

    border-radius: 2rem
    background: linear-gradient(160deg, #6f99acff 0%, #6f99ac60 100%)
    color: #fff

    grid-template-areas: ". close" "label count" "slider slider" "ok ok"
    grid-row-gap: 10px
    align-items: center
    justify-items: center

    .close
      grid-area: close
      justify-self: end
      align-self: start
    h2
      grid-area: label
      font-size: 1.5rem
    .count
      grid-area: count
      display: grid

      grid-template-areas: "amount incr""amount decr"
      justify-content: center
      align-items: center
      grid-column-gap: 5px
      .amount
        grid-area: amount
        font-size: 1.2rem
      .incr
        grid-area: incr
      .decr
        grid-area: decr
    .selectRange
      grid-area: slider
      position: relative
      width: 100%
      height: 4px
      margin: 20px 0
      background: linear-gradient(90deg, #ffffff 0%, #959da1 100%)
      .selectSlider
        position: absolute
        top: -8px
        left: 22%
        width: 10px
        height: 20px

        border-radius: 1rem
        background: #bfc7cb
    .ok
      grid-area: ok
</style>