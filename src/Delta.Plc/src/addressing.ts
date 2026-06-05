import type {DeltaConnectionType, PlcTag} from './types';
import {defaultDeltaConnectionType, getDeltaModbusAddress, type DeltaMemoryArea} from './deltaAddressMap';

export type PlcArea = 'coil' | 'discrete' | 'holding';

export interface PlcTagBinding {
  tag: PlcTag;
  area: PlcArea;
  startAddress: number;
  span: number;
  bitIndex?: number;
}

const numeric = (value: string, address: string): number => {
  if (!/^\d+$/.test(value)) {
    throw new Error(`PLC address '${address}' has an invalid numeric value.`);
  }
  return Number.parseInt(value, 10);
};

const wordSpan = (tag: PlcTag): number => {
  if (tag.dataType === 'Int32' || tag.dataType === 'Float') {
    return 2;
  }
  if (tag.dataType === 'String') {
    if (!tag.length || tag.length <= 0) {
      throw new Error(`String tag '${tag.name}' is missing a positive length.`);
    }
    return Math.ceil(tag.length / 2);
  }
  return 1;
};

const parseAddress = (address: string): {area: DeltaMemoryArea; index: number; bitIndex?: number} => {
  const match = /^([A-Z]+)(\d+)(?:\.(\d+))?$/.exec(address);
  if (!match) {
    throw new Error(`PLC address '${address}' is not supported.`);
  }

  const area = match[1] as DeltaMemoryArea;
  const index = numeric(match[2], address);
  const bitIndex = match[3] === undefined ? undefined : numeric(match[3], address);
  return {area, index, bitIndex};
};

export const createBinding = (
  tag: PlcTag,
  connectionType: DeltaConnectionType = defaultDeltaConnectionType,
): PlcTagBinding => {
  const address = tag.address.trim().toUpperCase();
  const parsed = parseAddress(address);

  if (parsed.bitIndex !== undefined) {
    if (parsed.area !== 'D') {
      throw new Error(`PLC bit address '${tag.address}' is only supported for D words.`);
    }
    if (tag.dataType !== 'Bool') {
      throw new Error(`D-bit address '${tag.address}' must use Bool data type.`);
    }
    if (parsed.bitIndex < 0 || parsed.bitIndex > 15) {
      throw new Error(`PLC bit address '${tag.address}' is outside the supported D-word bit range.`);
    }
    return {
      tag,
      area: 'holding',
      startAddress: getDeltaModbusAddress('D', parsed.index, connectionType),
      span: 1,
      bitIndex: parsed.bitIndex,
    };
  }

  switch (parsed.area) {
    case 'D':
    case 'SR':
    case 'HC':
    case 'E':
      return {
        tag,
        area: 'holding',
        startAddress: getDeltaModbusAddress(parsed.area, parsed.index, connectionType),
        span: wordSpan(tag),
      };
    case 'M':
    case 'Y':
    case 'S':
      if (tag.dataType !== 'Bool') {
        throw new Error(`Coil address '${tag.address}' must use Bool data type.`);
      }
      return {
        tag,
        area: 'coil',
        startAddress: getDeltaModbusAddress(parsed.area, parsed.index, connectionType),
        span: 1,
      };
    case 'X':
      if (tag.dataType !== 'Bool') {
        throw new Error(`Discrete input address '${tag.address}' must use Bool data type.`);
      }
      return {
        tag,
        area: 'discrete',
        startAddress: getDeltaModbusAddress(parsed.area, parsed.index, connectionType),
        span: 1,
      };
    case 'C':
    case 'T':
      if (tag.dataType === 'Bool') {
        return {
          tag,
          area: 'coil',
          startAddress: getDeltaModbusAddress(parsed.area, parsed.index, connectionType),
          span: 1,
        };
      }
      return {
        tag,
        area: 'holding',
        startAddress: getDeltaModbusAddress(parsed.area, parsed.index, connectionType),
        span: wordSpan(tag),
      };
    default:
      throw new Error(`PLC address '${tag.address}' is not supported for tag '${tag.name}'.`);
  }
};

export const createBindingLookup = (
  tags: PlcTag[],
  connectionType: DeltaConnectionType = defaultDeltaConnectionType,
): Map<string, PlcTagBinding> =>
  new Map(tags.map(tag => [tag.name.toLowerCase(), createBinding(tag, connectionType)]));

export const getBinding = (lookup: Map<string, PlcTagBinding>, tagName: string): PlcTagBinding => {
  const binding = lookup.get(tagName.toLowerCase());
  if (!binding) {
    throw new Error(`Unknown PLC tag: ${tagName}`);
  }
  return binding;
};
