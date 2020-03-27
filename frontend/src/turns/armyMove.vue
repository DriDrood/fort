<template>
  <circle
    class="armyMove"
    :style="{ cx: x, cy: y, r: size, fill: `url(#team-${teamId})` , transition: `cx ${config.armyRunDuration}s linear, cy ${config.armyRunDuration}s linear` }"
  />
</template>

<script>
import { mapState } from "vuex";

export default {
  name: "armyMove",
  props: {
    armyMove: {}
  },
  computed: {
    ...mapState(["config", "turnRun"]),
    animation() {
      const middleX = (this.armyMove.endX - this.armyMove.startX) / 2 + this.armyMove.startX;
      const middleY = (this.armyMove.endY - this.armyMove.startY) / 2 + this.armyMove.startY;

      return {
        size: [this.armyMove.size1, this.armyMove.size1, this.armyMove.size2],
        x: [this.armyMove.startX, middleX, this.armyMove.endX],
        y: [this.armyMove.startY, middleY, this.armyMove.endY]
      };
    },
    x() {
      return this.animation.x[this.turnRun.armiesPosition];
    },
    y() {
      return this.animation.y[this.turnRun.armiesPosition];
    },
    size() {
      return this.animation.size[this.turnRun.armiesPosition];
    },
    teamId() {
      return this.$store.state.players[this.armyMove.playerId].teamId;
    }
  }
};
</script>

<style lang="sass">
.armyMove
  fill: #fff
</style>