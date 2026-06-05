/// <reference lib="dom" />

import './web.css';
import {AppRegistry} from 'react-native';
import {createRoot} from 'react-dom/client';
import type {ComponentType, ReactElement} from 'react';
import App from './App';

interface WebAppRegistry {
  registerComponent(appKey: string, getComponentFunc: () => ComponentType): string;
  getApplication(appKey: string): {element: ReactElement};
}

const rootTag = document.getElementById('root');

if (!rootTag) {
  throw new Error('Web preview root element was not found.');
}

const appRegistry = AppRegistry as unknown as WebAppRegistry;
appRegistry.registerComponent('GearLineTablet', () => App);
createRoot(rootTag).render(appRegistry.getApplication('GearLineTablet').element);
