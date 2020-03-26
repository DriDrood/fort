<template>
  <g :id="`city-${city.id}`" class="city">
    <template v-if="occupation.army != null">
      <circle class="cityArmy" :cx="city.x - occupation.size" :cy="city.y - occupation.size" r="12" />
      <text :x="city.x - occupation.size" :y="city.y - occupation.size + 5" text-anchor="middle">{{ occupation.army }}</text>
    </template>
    <circle
      class="fort"
      :style="{stroke: `url(#team-${teamId})`}"
      :class="{selected: isSelected, available: isAvailable}"
      :cx="city.x"
      :cy="city.y"
      :r="occupation.size"
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
    occupation() {
      return this.$store.state.turns[this.$store.state.currentTurn.activeId].cityOccupation[this.city.id];
    },
    isSelected() {
      return this.selected == this.city.id;
    },
    isAvailable() {
      return this.$store.state.staticData.roads[this.city.id].includes(this.selected);
    },
    teamId() {
      return this.$store.state.staticData.players[this.occupation.playerId].teamId;
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
  .cityArmy
    fill: #fff
  .fort
    fill: #757575
    stroke-width: 5px
    &.selected
      stroke-width: 0px
</style>