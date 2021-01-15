<template>
  <g :id="`order-${orderId}`" class="order">
    <circle :cx="coords.x" :cy="coords.y" :r="size" />
    <text :x="coords.x" :y="coords.y + 5" text-anchor="middle">{{ order.amount }}</text>
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
  }
};
</script>

<style lang="sass">
.order
  circle
    fill: #ccc
</style>