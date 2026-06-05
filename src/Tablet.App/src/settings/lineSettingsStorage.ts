import {NativeModules} from 'react-native';
import type {LineConfig} from '../types/plc';

interface NativeLineSettingsStorage {
  loadLines(): Promise<string | null>;
  saveLines(linesJson: string): Promise<boolean>;
}

const nativeStorage = NativeModules.LineSettingsStorage as NativeLineSettingsStorage | undefined;

export const loadStoredLines = async (): Promise<LineConfig[] | null> => {
  if (!nativeStorage?.loadLines) {
    return null;
  }

  const raw = await nativeStorage.loadLines();
  if (!raw) {
    return null;
  }

  const parsed = JSON.parse(raw);
  return Array.isArray(parsed) ? parsed : null;
};

export const saveStoredLines = async (lines: LineConfig[]): Promise<void> => {
  if (!nativeStorage?.saveLines) {
    return;
  }

  await nativeStorage.saveLines(JSON.stringify(lines));
};
