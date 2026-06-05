jest.mock('react-native-tcp-socket', () => ({
  createConnection: jest.fn(),
}));
