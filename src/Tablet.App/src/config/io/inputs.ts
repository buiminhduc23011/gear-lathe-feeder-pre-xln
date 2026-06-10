import type {IoPointConfig} from './index';
import {createIoDisplayAddresses, defaultIoTagName} from './addressing';

export const ioInputs: IoPointConfig[] = createIoDisplayAddresses('X').map(displayAddress => ({
  displayAddress,
  tagName: defaultIoTagName('input', displayAddress),
}));
