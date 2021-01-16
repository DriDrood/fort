<template>
  <g :id="`order-${orderId}`" class="order">
    <circle :cx="coords.x" :cy="coords.y" :r="size" :style="{ fill: team.color, transition: `cx ${animationDuration}s linear, cy ${animationDuration}s linear` }" />
    <text v-if="showText" :x="coords.x" :y="coords.y + 5" text-anchor="middle" :style="`fill: ${team.colorLight}`">{{ order.startAmount }}</text>
  </g>
</template>

<script>
export default {
  name: "order",
  props: {
    orderId: {},
    order: {}
  },
  computed: {
    coords() {
      const [sourceId, targetId] = this.orderId.split(">>");
      const source = this.$store.state.map.cities[sourceId];
      const target = this.$store.state.map.cities[targetId];

      const armiesPosition = this.$store.state.turns.moveProgress == 0
        ? 0.2
        : this.$store.state.turns.moveProgress == 1
          ? 0.5
          : 1;

      const x = source.x + (target.x - source.x) * armiesPosition;
      const y = source.y + (target.y - source.y) * armiesPosition;

      return { x, y };
    },
    size() {
      if (this.$store.state.turns.moveProgress == 0)
        return 12;
        
      if (this.$store.state.turns.moveProgress == 1)
        return this.order.startSize;

      return this.order.endSize;
    },
    showText() {
      // unknown amount
      if (!this.order.startAmount)
        return false;

      return this.$store.state.turns.moveProgress == 0;
    },
    team() {
      const player = this.$store.state.user.players[this.order.playerId];
      const team = this.$store.state.user.teams[player.teamId];
      return team;
    },
    animationDuration() {
      return this.$store.state.master.config.armyMoveDuration;
    }
  }
};
</script>
