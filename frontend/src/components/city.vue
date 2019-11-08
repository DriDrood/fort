<template>
  <g :id="`city-${city.id}`" class="city">
    <circle class="army" :cx="city.x - size" :cy="city.y - size" r="12" />
    <text :x="city.x - size" :y="city.y - size + 5" text-anchor="middle">{{ city.army }}</text>
    <circle
      class="fort"
      :style="{fill: color}"
      :class="{selected: isSelected, available: isAvailable}"
      :cx="city.x"
      :cy="city.y"
      :r="size"
      @click="select"
    />
  </g>
</template>

<script>
export default {
  name: "city",
  props: {
    city: {},
    selected: { default: null }
  },
  computed: {
    size() {
      return this.city.army;
    },
    isSelected() {
      return this.selected == this.city.id;
    },
    isAvailable() {
      return this.$store.state.roads[this.city.id].includes(this.selected);
    },
    color() {
      return this.$store.state.teams[this.$store.state.players[this.city.owner].teamId].color;
    }
  },
  methods: {
    select() {
      this.$emit('select');
    }
  }
};
</script>

<style lang="sass">
svg .city
  .army
    fill: #fff
  .fort
    // fill: #757575
    stroke: url(#city-default)
    stroke-width: 5px
    &.selected
      stroke: url(#city-selected)
    &.available
      stroke: url(#city-available)
</style>