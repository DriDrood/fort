<template>
  <nav class="top">
    <img src="av.jpg" alt="avatar" title="Samuel" />
    <button class="prev" title="Vrátit o kolo"><i class="fa fa-step-backward"></i></button>
    <button class="next" title="Vpřed o kolo"><i class="fa fa-step-forward"></i></button>
    <button class="done" title="Hotovo"><i class="fa fa-check"></i></button>
    <div class="time">5:37</div>
  </nav>
</template>

<script>
export default {
  name: "topPanel"
}
</script>

<style lang="sass">
nav.top
  position: fixed
  display: grid
  width: 100%
  height: 5rem

  background: linear-gradient(170deg, #6f99acff 0%, #6f99ac60 100%)
  font-size: 2rem

  grid-template-areas: "prev next avatar done time"
  grid-template-columns: 2fr 2fr 3fr 2fr 2fr
  justify-items: center
  align-items: center

  z-index: 3

  img
    grid-area: avatar
    height: 140%
    margin: -20% 0
    border: 5px solid #241f16
    border-radius: 50%
  .prev
    grid-area: prev
  .next
    grid-area: next
  .done
    grid-area: done
  .time
    grid-area: time
    font-weight: bold
</style>