<template>
  <div class="mapContainer">
    <div class="map">
      <svg viewBox="0 0 1920 1024" preserveAspectRatio="none">
        <defs>
          <linearGradient
            v-for="(team, teamId) in user.teams"
            :key="`team-${teamId}`"
            :id="`team-${teamId}`"
          >
            <stop :style="{ 'stop-color': team.color }" offset="0" />
            <stop :style="{ 'stop-color': team.colorLight }" offset="1" />
          </linearGradient>
          <pattern v-for="userSize in userAvatarSizes" :key="userSize.key" :id="userSize.key" width="1" height="1">
            <image v-bind:[`xlink:href`]="`/users/${userSize.playerId}.jpg`" x="0" y="0" :width="userSize.size * 2" :height="userSize.size * 2" />
          </pattern>
        </defs>
        <road v-for="(road, index) in map.roads" :key="index" :road="road" />
        <order v-for="(order, orderId) in orders" :key="orderId" :order="order" :orderId="orderId" />
        <city
          v-for="city in map.cities"
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
          v-for="orderId in availableOrders"
          :key="`reuse-${orderId}`"
          v-bind:[`xlink:href`]="`#order-${orderId}`"
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
    ...mapState(["map", "user", "turns"]),
    ...mapGetters(["isTurnCurrent", "activeTurn"]),
    availableRoads() {
      if (!this.selected) return [];
      return this.$store.getters.cityRoads[this.selected].map(r =>
        r < this.selected ? `${r}__${this.selected}` : `${this.selected}__${r}`
      );
    },
    availableOrders() {
      if (!this.selected) return [];
      return Object.keys(this.activeTurn.orders).filter(id => id.split('>>')[0] == this.selected);
    },
    availableCities() {
      if (!this.selected) return [];
      return this.$store.getters.cityRoads[this.selected].concat(this.selected);
    },
    orders() {
      return (this.activeTurn && this.activeTurn.orders) || {};
    },
    userAvatarSizes() {
      let result = {};
      const occupations = (this.activeTurn && this.activeTurn.cityOccupations) || {};
      Object.values(occupations).forEach(c => {
        if (c.playerId == null)
          return;
          
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
      // selected again same city
      if (!cityId || cityId == this.selected) {
        this.selected = null;
        return;
      }

      if (this.$store.state.lifecycle.state.key != 'Running' && this.$store.state.lifecycle.state.key != "Paused")
      {
        this.$store.dispatch("notifyCreate", {
          text: "Kolo neběží",
          level: "warning"
        });
        this.selected = null;
        return;
      }

      // I'm in history
      if (!this.isTurnCurrent) {
        this.$store.dispatch("notifyCreate", {
          text: "Jste v minulosti",
          level: "warning"
        });
        this.selected = null;
        return;
      }

      // select 1st
      if (!this.selected) {
        // my city
        if (this.activeTurn.cityOccupations[cityId].playerId == this.$store.state.user.login.id) {
          this.selected = cityId;
          return;
        }

        // foreign city
        this.$store.dispatch("notifyCreate", {
          text: "Toto není vaše město",
          level: "warning"
        });
        return;
      }

      // selected 2nd available city
      if (this.$store.getters.cityRoads[this.selected].includes(cityId)) {
        this.targetId = cityId;
        this.showModal = true;
      }
    },
    closeModal() {
      this.selected = null;
      this.showModal = false;
    }
  }
};
</script>

<style lang="sass">
@import ../_sass/_constants

.mapContainer
  width: 100%
  height: 100%
  overflow: auto
  background-color: #000
  .map
    margin-top: $topPanel-height
    width: $map-width
    height: $map-height
    background: url('/world.jpg') no-repeat
    background-size: 100% 100%
    svg
      width: 100%
      height: 100%
      .darkness
        width: $map-width
        height: $map-height
        fill: #000
        fill-opacity: 0.8
</style>