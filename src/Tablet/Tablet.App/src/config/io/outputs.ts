import type {IoPointConfig} from './index';
import {createIoDisplayAddresses, defaultIoTagName} from './addressing';

export const ioOutputs: IoPointConfig[] = createIoDisplayAddresses('Y').map(displayAddress => ({
  displayAddress,
  tagName: defaultIoTagName('output', displayAddress),
}));
