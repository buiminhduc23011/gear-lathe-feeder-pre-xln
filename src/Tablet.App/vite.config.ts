import {defineConfig} from 'vite';
import react from '@vitejs/plugin-react';
import path from 'path';

export default defineConfig({
  plugins: [
    react({
      babel: {
        plugins: ['react-native-web'],
      },
    }),
  ],
  resolve: {
    alias: [
      {
        find: /^react-native$/,
        replacement: 'react-native-web',
      },
      {
        find: /^@sti\/delta-plc$/,
        replacement: path.resolve(__dirname, '../Delta.Plc/src'),
      },
      {
        find: /^buffer$/,
        replacement: path.resolve(__dirname, 'node_modules/buffer'),
      },
    ],
    extensions: [
      '.web.tsx',
      '.web.ts',
      '.web.jsx',
      '.web.js',
      '.tsx',
      '.ts',
      '.jsx',
      '.js',
      '.json',
    ],
  },
  define: {
    __DEV__: JSON.stringify(process.env.NODE_ENV !== 'production'),
    global: 'globalThis',
  },
  server: {
    host: '0.0.0.0',
    port: 5173,
  },
});
