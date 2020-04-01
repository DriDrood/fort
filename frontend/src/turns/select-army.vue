<template>
  <div class="modalContainer" @click="depClose">
    <div class="armySelect">
      <button class="close" @click="close">
        <i class="fa fa-times"></i>
      </button>
      <h2>Armáda</h2>
      <input class="amount" v-model="value" />
      <button class="incr" @click="incr">
        <i class="fa fa-caret-up"></i>
      </button>
      <button class="decr" @click="decr">
        <i class="fa fa-caret-down"></i>
      </button>
      <div class="selectRange" @click="set">
        <div class="selectSlider" :style="{left: `${ratio}%`}"></div>
      </div>
      <button class="ok" @click="accept">OK</button>
    </div>
  </div>
</template>

<script>
import { mapGetters } from "vuex";

export default {
  name: "selectArmy",
  props: {
    sourceId: {},
    targetId: {}
  },
  data: () => ({
    value: null
  }),
  computed: {
    ...mapGetters(["activeTurn"]),
    max() {
      const source = this.activeTurn.cityOccupations[this.sourceId];
      return (source.availableArmy != null ? source.availableArmy : source.army) + (this.prevOrderArmy || 0);
    },
    ratio() {
      return Math.floor((this.value / this.max) * 100);
    },
    prevOrderArmy() {
      const prevOrder = this.activeTurn.orders[`${this.sourceId}>>${this.targetId}`]
      return (prevOrder && prevOrder.amount) || 0;
    }
  },
  methods: {
    incr() {
      if (this.value >= this.max) return;

      this.value -= -1;
    },
    decr() {
      if (this.value <= 0) return;

      this.value -= 1;
    },
    set(e) {
      this.value = Math.round((e.offsetX / e.target.offsetWidth) * this.max);
    },
    accept() {
      this.$store.commit('order', {
        sourceId: this.sourceId,
        targetId: this.targetId,
        amount: this.value,
        max: this.max
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
  },
  mounted() {
    if (this.prevOrderArmy)
      this.value = this.prevOrderArmy;
    else
      this.value = this.max;
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

    grid-template-areas: ". . close" "label amount incr" "label amount decr" "slider slider slider" "ok ok ok"
    grid-column: 1fr 1fr 1fr
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
    .amount
      grid-area: amount
      font-size: 1.2rem
      width: 4rem
    .incr
      grid-area: incr
      font-size: 2rem
    .decr
      grid-area: decr
      font-size: 2rem
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
        // left: 22%
        width: 10px
        height: 20px

        border-radius: 1rem
        background: #bfc7cb
    .ok
      grid-area: ok
</style>