const {getDefaultConfig, mergeConfig} = require('@react-native/metro-config');
const path = require('path');

const deltaPlcRoot = path.resolve(__dirname, '../Delta.Plc');

module.exports = mergeConfig(getDefaultConfig(__dirname), {
  watchFolders: [deltaPlcRoot],
  resolver: {
    extraNodeModules: {
      '@sti/delta-plc': path.join(deltaPlcRoot, 'src'),
      '@babel/runtime': path.resolve(__dirname, 'node_modules/@babel/runtime'),
      buffer: path.resolve(__dirname, 'node_modules/buffer'),
    },
  },
});
