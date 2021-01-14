<template>
  <div class="notifications">
    <div
      v-for="notification in notifications"
      :key="notification.id"
      class="notification"
      :class="notification.level"
    >
      {{ notification.text }}
      <i class="fa fa-times" @click="destroy(notification.id)"></i>
    </div>
  </div>
</template>

<script>
import { mapState } from "vuex";

export default {
  name: "notification",
  computed: {
    ...mapState(["notifications"])
  },
  methods: {
    destroy(id) {
      this.$store.commit('unNotify', {
        id
      });
    }
  }
};
</script>

<style lang="sass">
@import ../_sass/_constants

.notifications
  position: fixed
  top: $topPanel-height
  right: 0
  border-radius: 0 0 0 1rem
  overflow: hidden
  .notification
    padding: 1rem
    color: #fff
    font-weight: bold
    &.info
      background-color: #6f99ac60
    &.warning
      background-color: #bdc14e60
    &.error
      background-color: #772a2a60
    i.fa
      float: right
      margin-left: 1rem
      cursor: pointer
</style>