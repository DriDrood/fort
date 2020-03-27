<template>
  <line
    class="road"
    :id="`road-${road.source.id}-${road.target.id}`"
    :x1="road.source.x"
    :y1="road.source.y"
    :x2="road.target.x"
    :y2="road.target.y"
  />
</template>

<script>
export default {
  name: "road",
  props: ["road"]
};
</script>

<style lang="sass">
.road
  stroke: #ccc
  stroke-width: 5px
</style>