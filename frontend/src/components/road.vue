<template>
  <line
    class="road"
    :id="`road-${road}`"
    :x1="source.x"
    :y1="source.y"
    :x2="target.x"
    :y2="target.y"
  />
</template>

<script>
export default {
  name: "road",
  props: ["road"],
  computed: {
    source() {
      return this.$store.state.map.cities[this.road.split("__")[0]];
    },
    target() {
      return this.$store.state.map.cities[this.road.split("__")[1]];
    }
  }
};
</script>

<style lang="sass">
.road
  stroke: #ccc
  stroke-width: 5px
</style>