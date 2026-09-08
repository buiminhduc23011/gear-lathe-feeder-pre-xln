import {Buffer} from 'buffer';

export const toSignedInt16 = (word: number): number => {
  const normalized = word & 0xffff;
  return normalized >= 0x8000 ? normalized - 0x10000 : normalized;
};

export const normalizeWord = (value: number): number => value & 0xffff;

export const wordsToInt32 = (lowWord: number, highWord: number): number => {
  const bytes = Buffer.alloc(4);
  bytes.writeUInt16LE(normalizeWord(lowWord), 0);
  bytes.writeUInt16LE(normalizeWord(highWord), 2);
  return bytes.readInt32LE(0);
};

export const int32ToWords = (value: number): number[] => {
  const bytes = Buffer.alloc(4);
  bytes.writeInt32LE(value, 0);
  return [bytes.readUInt16LE(0), bytes.readUInt16LE(2)];
};

export const wordsToFloat = (lowWord: number, highWord: number): number => {
  const bytes = Buffer.alloc(4);
  bytes.writeUInt16LE(normalizeWord(lowWord), 0);
  bytes.writeUInt16LE(normalizeWord(highWord), 2);
  return bytes.readFloatLE(0);
};

export const floatToWords = (value: number): number[] => {
  const bytes = Buffer.alloc(4);
  bytes.writeFloatLE(value, 0);
  return [bytes.readUInt16LE(0), bytes.readUInt16LE(2)];
};

export const wordsToString = (words: number[], maxLength: number): string => {
  const chars: string[] = [];
  for (const word of words) {
    if (chars.length < maxLength) {
      chars.push(String.fromCharCode(word & 0x00ff));
    }
    if (chars.length < maxLength) {
      chars.push(String.fromCharCode((word >> 8) & 0x00ff));
    }
  }
  return chars.join('').replace(/[\0 ]+$/g, '');
};

export const stringToWords = (value: string, maxLength: number): number[] => {
  const normalized = value.slice(0, maxLength).padEnd(maxLength, ' ');
  const words: number[] = [];
  for (let index = 0; index < maxLength; index += 2) {
    const low = normalized.charCodeAt(index) & 0xff;
    const high = index + 1 < normalized.length ? normalized.charCodeAt(index + 1) & 0xff : 0x20;
    words.push(low | (high << 8));
  }
  return words;
};
