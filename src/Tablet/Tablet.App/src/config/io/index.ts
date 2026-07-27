import {ioInputs} from './inputs';
import {ioOutputs} from './outputs';

export {ioInputs, ioOutputs};

export type IoChannelKey = 'input' | 'output';

export interface IoPointConfig {
  displayAddress: string;
  tagName: string;
}

export interface IoChannelConfig {
  key: IoChannelKey;
  prefix: 'X' | 'Y';
  label: string;
  points: IoPointConfig[];
}

export const ioChannels: IoChannelConfig[] = [
  {key: 'input', prefix: 'X', label: 'Input', points: ioInputs},
  {key: 'output', prefix: 'Y', label: 'Output', points: ioOutputs},
];
