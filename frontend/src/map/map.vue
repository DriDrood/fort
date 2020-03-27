<template>
  <div class="mapContainer">
    <div class="map">
      <svg viewBox="0 0 1920 1024" preserveAspectRatio="none">
        <defs>
          <linearGradient
            v-for="(team, teamId) in teams"
            :key="`team-${teamId}`"
            :id="`team-${teamId}`"
          >
            <stop :style="{ 'stop-color': team.color }" offset="0" />
            <stop :style="{ 'stop-color': team.light }" offset="1" />
          </linearGradient>
          <pattern v-for="userSize in userAvatarSizes" :key="userSize.key" :id="userSize.key" width="1" height="1">
            <image v-bind:[`xlink:href`]="`/users/${userSize.playerId}.jpg`" x="0" y="0" :width="userSize.size * 2" :height="userSize.size * 2" />
          </pattern>
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
        <armyMove v-for="(armyMove, index) in turnRun.armies" :key="`armyMove-${index}`" :armyMove="armyMove" />
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
import Vue from "vue";
import { mapState, mapGetters } from "vuex";
import city from "./city";
import road from "./road";
import order from "../turns/order";
import armyMove from "../turns/armyMove";
import selectArmy from "../turns/select-army";

export default {
  name: "worldMap",
  components: {
    city,
    road,
    order,
    armyMove,
    selectArmy
  },
  data: () => ({
    selected: null,
    targetId: null,
    showModal: false
  }),
  computed: {
    ...mapState(["cities", "roads", "teams", "turnRun"]),
    ...mapGetters(["isTurnCurrent", "activeTurn"]),
    distinctRoads() {
      let result = [];
      Object.keys(this.roads).forEach(id => {
        const sourceId = parseInt(id);
        const targetIds = this.roads[sourceId];
        targetIds.forEach(targetId => {
          if (sourceId < targetId)
            result.push({
              source: this.cities[sourceId],
              target: this.cities[targetId]
            });
        });
      });
      return result;
    },
    availableRoads() {
      if (!this.selected) return [];
      return this.roads[this.selected].map(r =>
        r < this.selected ? `${r}-${this.selected}` : `${this.selected}-${r}`
      );
    },
    availableCities() {
      if (!this.selected) return [];
      return this.roads[this.selected].concat(this.selected);
    },
    orders() {
      return this.activeTurn.orders;
    },
    userAvatarSizes() {
      let result = {};
      Object.values(this.activeTurn.cityOccupation).forEach(c => {
        const key = `U_${c.playerId}_${c.size}`;
        if (result[key] === undefined)
          Vue.set(result, key, {
            playerId: c.playerId,
            size: c.size,
            key: key
          });
      });
      return Object.values(result);
    }
  },
  methods: {
    select(cityId) {
      // I'm in history
      if (!this.isTurnCurrent) {
        this.$store.commit("notify", {
          text: "Jste v minulosti",
          level: "warning"
        });
        return;
      }

      // selected again same city
      if (!cityId || cityId == this.selected) this.selected = null;
      // selected 2nd available city
      else if (this.selected && this.roads[this.selected].includes(cityId)) {
        this.targetId = cityId;
        this.showModal = true;
      }

      // select 1st
      else if (this.activeTurn.cityOccupation[cityId].playerId == this.$store.state.login.id)
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