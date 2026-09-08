export const createIoDisplayAddresses = (prefix: 'X' | 'Y'): string[] => {
  return Array.from({length: 48}, (_, point) => `${prefix}${point.toString().padStart(2, '0')}`);
};

export const defaultIoTagName = (channel: 'input' | 'output', displayAddress: string): string =>
  `${channel}.${displayAddress.toLowerCase()}`;

export const plcAddressForIoDisplayAddress = (displayAddress: string): string => {
  const point = Number(displayAddress.slice(1));
  const area = displayAddress.startsWith('X') ? 'M' : 'Y';
  return `${area}${point}`;
};
