<template>
  <div class="mapContainer">
    <div class="map">
      <svg viewBox="0 0 1920 1024" preserveAspectRatio="none">
        <defs>
          <linearGradient
            v-for="(team, teamId) in staticData.teams"
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
          v-for="city in staticData.cities"
          :key="`city-${city.id}`"
          :city="city"
          :selected="selected"
          @select="select(city.id)"
        />
        <army v-for="(army, index) in moveRun.armies" :key="`army-${index}`" :army="army" />
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
import Vue from 'vue';
import { mapState, mapGetters } from "vuex";
import city from "./city";
import road from "./road";
import order from "./order";
import army from "./army";
import selectArmy from "./select-army";

export default {
  name: "worldMap",
  components: {
    city,
    road,
    order,
    army,
    selectArmy
  },
  data: () => ({
    selected: null,
    targetId: null,
    showModal: false
  }),
  computed: {
    ...mapState(["staticData", "moveRun"]),
    ...mapGetters(["distinctRoads", "isTurnCurrent", "currentTurn"]),
    availableRoads() {
      if (!this.selected) return [];
      return this.staticData.roads[this.selected].map(r =>
        r < this.selected ? `${r}-${this.selected}` : `${this.selected}-${r}`
      );
    },
    availableCities() {
      if (!this.selected) return [];
      return this.staticData.roads[this.selected].concat(this.selected);
    },
    orders() {
      return this.currentTurn.orders;
    },
    userAvatarSizes() {
      let result = {};
      Object.values(this.currentTurn.cityOccupation).forEach(c => {
        const key = `U_${c.playerId}_${c.size}`;
        if (result[key] === undefined)
          Vue.set(result, key, { playerId: c.playerId, size: c.size, key: key });
      });
      return Object.values(result);
    }
  },
  methods: {
    select(cityId) {
      // I'm in history
      if (!this.isTurnCurrent) return;

      // selected again same city
      if (!cityId || cityId == this.selected) this.selected = null;

      // selected 2nd available city
      else if (
        this.selected &&
        this.staticData.roads[this.selected].includes(cityId)
      ) {
        this.targetId = cityId;
        this.showModal = true;
      }

      // select 1st
      else if (this.currentTurn.cityOccupation[cityId].playerId == this.$store.state.login.id)
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