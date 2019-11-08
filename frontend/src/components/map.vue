<template>
  <div class="mapContainer">
    <div class="map">
      <svg viewBox="0 0 1920 1024" preserveAspectRatio="none">
        <defs>
          <linearGradient id="city-selected">
            <stop style="stop-color:#83824b" offset="0" />
            <stop style="stop-color:#c4c498" offset="1" />
          </linearGradient>
          <linearGradient id="city-available">
            <stop style="stop-color:#52834b" offset="0" />
            <stop style="stop-color:#9dc498" offset="1" />
          </linearGradient>
          <linearGradient id="city-default">
            <stop style="stop-color:#4b7183" offset="0" />
            <stop style="stop-color:#98b6c4" offset="1" />
          </linearGradient>
        </defs>
        <road v-for="(road, index) in distinctRoads" :key="index" :road="road" />
        <order v-for="(order, orderId) in orders" :key="orderId" :order="order" :orderId="orderId" />
        <city
          v-for="city in cities"
          :key="`city-${city.id}`"
          :city="city"
          :selected="selected"
          @select="select(city.id)"
        />
        <rect
          v-if="selected"
          class="darkness"
          x="0"
          y="0"
          width="1920"
          height="1024"
          @click="select(null)"
        />
        <use
          v-for="roadId in availableRoads"
          :key="`reuse-${roadId}`"
          v-bind:[`xlink:href`]="`#road-${roadId}`"
        />
        <use
          v-for="cityId in availableCities"
          :key="`reuse-${cityId}`"
          v-bind:[`xlink:href`]="`#city-${cityId}`"
        />
      </svg>
    </div>
    <selectArmy v-if="showModal" @close="closeModal" :sourceId="selected" :targetId="targetId" />
  </div>
</template>

<script>
import { mapState, mapGetters } from "vuex";
import city from "./city";
import road from "./road";
import order from "./order";
import selectArmy from "./select-army";

export default {
  name: "worldMap",
  components: {
    city,
    road,
    order,
    selectArmy
  },
  data: () => ({
    selected: null,
    targetId: null,
    showModal: false
  }),
  computed: {
    ...mapState(["cities"]),
    ...mapGetters(["distinctRoads"]),
    availableRoads() {
      if (!this.selected) return [];
      return this.$store.state.roads[this.selected].map(r =>
        r < this.selected ? `${r}-${this.selected}` : `${this.selected}-${r}`
      );
    },
    availableCities() {
      if (!this.selected) return [];
      return this.$store.state.roads[this.selected].concat(this.selected);
    },
    orders() {
      return this.$store.state.prevTurns[this.$store.state.turn.activeId];
    }
  },
  methods: {
    select(cityId) {
      // selected again same city
      if (!cityId || cityId == this.selected) this.selected = null;
      // selected 2nd available city
      else if (
        this.selected &&
        this.$store.state.roads[this.selected].includes(cityId)
      ) {
        this.targetId = cityId;
        this.showModal = true;
      }
      // select 1st
      else if (this.cities[cityId].owner == this.$store.state.login.id)
        this.selected = cityId;
    },
    closeModal() {
      this.selected = null;
      this.showModal = false;
    }
  }
};
</script>

<style lang="sass">
.mapContainer
  width: 100%
  height: 100%
  overflow: auto
  background-color: #000
  .map
    margin-top: 5rem
    width: 1920px
    height: 1024px
    background: url('/world.jpg') no-repeat
    background-size: 100% 100%
    svg
      width: 100%
      height: 100%
      .darkness
        fill: #000
        fill-opacity: 0.8
</style>