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
      :fill="isOwnerVisible ? `url(#U_${occupation.playerId}_${occupation.size}` : '#757575'"
      @click="select"
    />
  </g>
</template>

<script>
import { mapGetters } from 'vuex';

export default {
  name: "city",
  props: {
    city: {},
    selected: { default: null }
  },
  computed: {
    ...mapGetters(["activeTurn"]),
    occupation() {
      return this.activeTurn.cityOccupations[this.city.id];
    },
    isSelected() {
      return this.selected == this.city.id;
    },
    isAvailable() {
      return this.$store.getters.cityRoads[this.city.id].includes(this.selected);
    },
    isOwnerVisible() {
      // neutral city
      if (this.occupation.playerId == null)
        return false;

      // same player
      if (this.occupation.playerId == this.$store.state.login.id)
        return true;

      // same team
      var currentPlayerTeamId = this.$store.state.players[this.$store.state.login.id].teamId;
      if (this.$store.state.players[this.occupation.playerId].teamId == currentPlayerTeamId)
        return true;

      // next to my
      if (this.$store.getters.cityRoads[this.city.id].some(neighbourId =>
          this.$store.state.players[this.activeTurn.cityOccupations[neighbourId].playerId].teamId == currentPlayerTeamId))
        return true;

      // else
      return false;
    },
    teamId() {
      if (this.occupation.playerId == null)
        return 'neutral';
        
      return this.$store.state.players[this.occupation.playerId].teamId;
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
    stroke-width: 5px
    &.selected
      stroke-width: 0px
</style>