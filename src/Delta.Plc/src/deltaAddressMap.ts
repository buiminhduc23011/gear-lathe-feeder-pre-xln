import type {DeltaConnectionType} from './types';

export type DeltaMemoryArea = 'D' | 'M' | 'X' | 'Y' | 'S' | 'T' | 'C' | 'SR' | 'HC' | 'E';

interface DeltaAddressRule {
  base: number;
  max: number;
  label: string;
}

const dvpRules: Partial<Record<DeltaMemoryArea, DeltaAddressRule>> = {
  D: {base: 0x1000, max: 4999, label: 'DVP Series D register range: D0-D4999'},
  M: {base: 0x0800, max: 4095, label: 'DVP Series M bit range: M0-M4095'},
  X: {base: 0x0400, max: 255, label: 'DVP Series X bit range: X0-X377 octal'},
  Y: {base: 0x0500, max: 255, label: 'DVP Series Y bit range: Y0-Y377 octal'},
  S: {base: 0x0000, max: 999, label: 'DVP Series S bit range: S0-S999'},
  T: {base: 0x0600, max: 255, label: 'DVP Series T range: T0-T255'},
  C: {base: 0x0e00, max: 255, label: 'DVP Series C range: C0-C255'},
};

const asRules: Record<DeltaMemoryArea, DeltaAddressRule> = {
  D: {base: 0x0000, max: 29999, label: 'AS Series D register range: D0-D29999'},
  M: {base: 0x0000, max: 8191, label: 'AS Series M bit range: M0-M8191'},
  X: {base: 0x6000, max: 1023, label: 'AS Series X bit range: X0-X1023'},
  Y: {base: 0xa000, max: 1023, label: 'AS Series Y bit range: Y0-Y1023'},
  S: {base: 0x5000, max: 1023, label: 'AS Series S bit range: S0-S1023'},
  T: {base: 0xe000, max: 2047, label: 'AS Series T range: T0-T2047'},
  C: {base: 0xf000, max: 1023, label: 'AS Series C range: C0-C1023'},
  SR: {base: 0xc000, max: 511, label: 'AS Series SR range: SR0-SR511'},
  HC: {base: 0xfc00, max: 255, label: 'AS Series HC range: HC0-HC255'},
  E: {base: 0xfe00, max: 255, label: 'AS Series E range: E0-E255'},
};

export const defaultDeltaConnectionType: DeltaConnectionType = 'TcpAS';

export const getDeltaModbusAddress = (
  area: DeltaMemoryArea,
  index: number,
  connectionType: DeltaConnectionType = defaultDeltaConnectionType,
): number => {
  if (!Number.isInteger(index) || index < 0) {
    throw new Error(`Delta PLC address ${area}${index} has an invalid index.`);
  }

  const rules = connectionType === 'TcpAS' ? asRules : dvpRules;
  const rule = rules[area];
  if (!rule) {
    throw new Error(`Delta ${connectionType} does not support area ${area}.`);
  }
  if (index > rule.max) {
    throw new Error(`${rule.label}; received ${area}${index}.`);
  }

  return rule.base + index;
};
