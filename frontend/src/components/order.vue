<template>
  <g class="order">
    <circle :cx="coords.x" :cy="coords.y" r="12" />
    <text :x="coords.x" :y="coords.y + 5" text-anchor="middle">{{ amount }}</text>
  </g>
</template>

<script>
export default {
  name: "order",
  props: {
    order: {},
    amount: {}
  },
  computed: {
    coords() {
      const [sourceId, targetId] = this.order.split("-");
      const source = this.$store.state.cities[sourceId];
      const target = this.$store.state.cities[targetId];

      const x = source.x + (target.x - source.x) / 4;
      const y = source.y + (target.y - source.y) / 4;

      return { x, y };
    }
  }
};
</script>

<style lang="sass">
.order
  circle
    fill: #ccc
</style>