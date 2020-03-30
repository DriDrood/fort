export default {
  loading: (state) => Object.values(state.requests).filter(r => r).length > 0
}