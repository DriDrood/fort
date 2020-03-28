export default {
  activeTurnId: 1,
  turns: [
    {
      cityOccupation: {
        '1': {
          playerId: '5',
          size: 50
        },
        '2': {
          playerId: '4',
          size: 15
        },
        '3': {
          playerId: '4',
          size: 12
        },
        '4': {
          playerId: '4',
          size: 12
        }
      },
      orders: {
        '4>>3': {
          playerId: '4',
          amount: 10,
          size: 8
        },
        '3>>4': {
          playerId: '5',
          amount: 5,
          size: 7
        }
      }
    },
    {
      cityOccupation: {
        '1': {
          playerId: '3',
          size: 15
        },
        '2': {
          playerId: '4',
          size: 15
        },
        '3': {
          playerId: '5',
          size: 12,
          army: 20
        },
        '4': {
          playerId: '4',
          size: 24
        }
      },
      orders: {}
    }
  ],
  turnRun: {
    armies: [],
    armiesPosition: 0
  }
}
