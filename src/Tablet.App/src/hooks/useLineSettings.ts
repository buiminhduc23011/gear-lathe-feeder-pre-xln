import {useCallback, useEffect, useState} from 'react';
import {tabletAppConfig} from '../config/manualConfig';
import {loadStoredLines, saveStoredLines} from '../settings/lineSettingsStorage';
import type {LineConfig} from '../types/plc';

const minPollIntervalMs = 50;
const maxPollIntervalMs = 5000;

const clamp = (value: number, min: number, max: number) => Math.min(Math.max(value, min), max);

export const normalizeLine = (line: LineConfig): LineConfig => ({
  id: line.id.trim() || 'line1',
  name: line.name.trim() || 'Line 1',
  host: line.host.trim() || '127.0.0.1',
  port: clamp(Math.trunc(line.port) || 503, 1, 65535),
  slaveId: clamp(Math.trunc(line.slaveId) || 1, 1, 247),
  pollIntervalMs: clamp(Math.trunc(line.pollIntervalMs) || 100, minPollIntervalMs, maxPollIntervalMs),
  connectionType: line.connectionType === 'TcpDVP' ? 'TcpDVP' : 'TcpAS',
});

const normalizeLines = (lines: LineConfig[]) => {
  const normalized = lines.map(normalizeLine);
  return normalized.length > 0 ? normalized : tabletAppConfig.lines;
};

const nextLineNumber = (lines: LineConfig[]) => {
  const maxNumber = lines.reduce((max, line) => {
    const match = line.id.match(/^line(\d+)$/);
    return match ? Math.max(max, Number(match[1])) : max;
  }, 0);
  return maxNumber + 1;
};

export const createNextLine = (lines: LineConfig[]): LineConfig => {
  const lineNumber = nextLineNumber(lines);
  return {
    id: `line${lineNumber}`,
    name: `Line ${lineNumber}`,
    host: '127.0.0.1',
    port: 502 + lineNumber,
    slaveId: lineNumber,
    pollIntervalMs: 100,
    connectionType: 'TcpAS',
  };
};

export const useLineSettings = () => {
  const [lines, setLines] = useState<LineConfig[]>(tabletAppConfig.lines);

  const commitLines = useCallback((nextLines: LineConfig[]) => {
    const normalized = normalizeLines(nextLines);
    setLines(normalized);
    saveStoredLines(normalized).catch(() => undefined);
    return normalized;
  }, []);

  useEffect(() => {
    let disposed = false;
    loadStoredLines()
      .then(storedLines => {
        if (!disposed && storedLines) {
          commitLines(storedLines);
        }
      })
      .catch(() => undefined);

    return () => {
      disposed = true;
    };
  }, [commitLines]);

  const addLine = useCallback(() => {
    const nextLine = createNextLine(lines);
    commitLines([...lines, nextLine]);
    return nextLine;
  }, [commitLines, lines]);

  const saveLine = useCallback((line: LineConfig) => {
    commitLines(lines.map(current => (current.id === line.id ? line : current)));
  }, [commitLines, lines]);

  const deleteLine = useCallback((lineId: string) => {
    if (lines.length <= 1) {
      return lines;
    }
    return commitLines(lines.filter(line => line.id !== lineId));
  }, [commitLines, lines]);

  return {
    lines,
    addLine,
    saveLine,
    deleteLine,
  };
};
