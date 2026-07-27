module.exports = {
  preset: 'react-native',
  moduleNameMapper: {
    '^@sti/delta-plc$': '<rootDir>/../Delta.Plc/src',
    '^@babel/runtime/(.*)$': '<rootDir>/node_modules/@babel/runtime/$1',
    '^buffer$': '<rootDir>/node_modules/buffer',
  },
  transformIgnorePatterns: [
    'node_modules/(?!((jest-)?react-native|@react-native|@react-native-community|react-native-tcp-socket)/)',
  ],
  setupFilesAfterEnv: ['<rootDir>/jest.setup.js'],
  testMatch: ['**/__tests__/**/*.test.ts', '**/__tests__/**/*.test.tsx'],
};
