/// <reference lib="dom" />

import type {LineConfig} from '../types/plc';

const storageKey = 'gear-line-tablet.lines';

export const loadStoredLines = async (): Promise<LineConfig[] | null> => {
  const raw = globalThis.localStorage?.getItem(storageKey);
  if (!raw) {
    return null;
  }

  const parsed = JSON.parse(raw);
  return Array.isArray(parsed) ? parsed : null;
};

export const saveStoredLines = async (lines: LineConfig[]): Promise<void> => {
  globalThis.localStorage?.setItem(storageKey, JSON.stringify(lines));
};
