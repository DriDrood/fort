<template>
  <circle
    class="army"
    :style="{ cx: x, cy: y, r: size, fill: `url(#team-${teamId})` , transition: `cx ${staticData.config.armyRunDuration}s linear, cy ${staticData.config.armyRunDuration}s linear` }"
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
    ...mapState(["moveRun", "staticData"]),
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
      return this.animation.x[this.moveRun.armiesPosition];
    },
    y() {
      return this.animation.y[this.moveRun.armiesPosition];
    },
    size() {
      return this.animation.size[this.moveRun.armiesPosition];
    },
    teamId() {
      return this.$store.state.staticData.players[this.army.playerId].teamId;
    }
  }
};
</script>

<style lang="sass">
.army
  fill: #fff
</style>