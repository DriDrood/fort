const mutations = {
  generateGuid: () => `${mutations.s4()}${mutations.s4()}-${mutations.s4()}-4${mutations.s4().substr(0, 3)}-${mutations.s4()}-${mutations.s4()}${mutations.s4()}${mutations.s4()}`.toLowerCase(),
  s4: () => (((1 + Math.random()) * 0x10000) | 0).toString(16).substring(1)
};
export default mutations;