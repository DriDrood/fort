<template>
  <circle
    class="army"
    :style="{ cx: x, cy: y, r: size, transition: `cx ${move.duration}s linear, cy ${move.duration}s linear` }"
  />
</template>

<script>
import { mapState } from "vuex";

export default {
  name: "army",
  props: {
    army: {}
  },
  computed: {
    ...mapState(["move"]),
    animation() {
      const middleX = (this.army.endX - this.army.startX) / 2 + this.army.startX;
      const middleY = (this.army.endY - this.army.startY) / 2 + this.army.startY;

      return {
        size: [this.army.size1, this.army.size1, this.army.size2],
        x: [this.army.startX, middleX, this.army.endX],
        y: [this.army.startY, middleY, this.army.endY]
      };
    },
    x() {
      return this.animation.x[this.move.armiesPosition];
    },
    y() {
      return this.animation.y[this.move.armiesPosition];
    },
    size() {
      return this.animation.size[this.move.armiesPosition];
    }
  }
};
</script>

<style lang="sass">
.army
  fill: #fff
</style>