import {useCallback, useEffect, useMemo, useRef, useState} from 'react';
import {AppState} from 'react-native';
import {tabletAppConfig} from '../config/manualConfig';
import {ManualController} from '../manual/manualController';
import type {LineConfig, PlcSnapshot} from '../types/plc';
import {PlcClient, type PlcTransportFactory} from '../plc/PlcClient';

export interface ManualRuntime {
  selectedLine: LineConfig;
  lines: LineConfig[];
  snapshot: PlcSnapshot;
  isConnected: boolean;
  lastUpdatedText: string;
  errorText: string;
  activeJogTag: string | null;
  controller: ManualController;
  selectLine(lineId: string): void;
  refreshNow(): Promise<void>;
}

export const useManualRuntime = (
  configuredLines: LineConfig[] = tabletAppConfig.lines,
  transportFactory?: PlcTransportFactory,
): ManualRuntime => {
  const lines = configuredLines.length > 0 ? configuredLines : tabletAppConfig.lines;
  const [selectedLineId, setSelectedLineId] = useState(lines[0].id);
  const selectedLine = lines.find(line => line.id === selectedLineId) ?? lines[0];
  const [snapshot, setSnapshot] = useState<PlcSnapshot>({});
  const [isConnected, setIsConnected] = useState(false);
  const [lastUpdatedText, setLastUpdatedText] = useState('Waiting for PLC data');
  const [errorText, setErrorText] = useState('');
  const [, forceRender] = useState(0);
  const clientRef = useRef<PlcClient | null>(null);
  const refreshInFlightRef = useRef<Promise<void> | null>(null);
  const nextPollAtRef = useRef(0);
  const snapshotRef = useRef<PlcSnapshot>({});

  const updateSnapshot = useCallback((nextSnapshot: PlcSnapshot) => {
    snapshotRef.current = nextSnapshot;
    setSnapshot(nextSnapshot);
  }, []);

  const controller = useMemo(() => {
    const plcPort = {
      get isConnected() {
        return clientRef.current?.isConnected ?? false;
      },
      snapshot: () => clientRef.current?.snapshot() ?? snapshotRef.current,
      write: async (tagName: string, value: unknown) => {
        await clientRef.current?.write(tagName, value);
        updateSnapshot(clientRef.current?.snapshot() ?? {});
      },
    };
    return new ManualController(tabletAppConfig.manualScreen, plcPort);
  }, [updateSnapshot]);

  const refreshNow = useCallback(async () => {
    if (refreshInFlightRef.current) {
      return refreshInFlightRef.current;
    }

    const refreshPromise = (async () => {
      if (!clientRef.current) {
        return;
      }
      try {
        const nextSnapshot = await clientRef.current.readAll();
        updateSnapshot(nextSnapshot);
        setIsConnected(clientRef.current.isConnected);
        setLastUpdatedText(`Last updated ${new Date().toLocaleTimeString()}`);
        setErrorText('');
        nextPollAtRef.current = 0;
        await controller.clearStaleJogTags();
      } catch (error) {
        setIsConnected(false);
        setErrorText(error instanceof Error ? error.message : String(error));
        nextPollAtRef.current = Date.now() + Math.max(selectedLine.pollIntervalMs, 1000);
      }
    })();

    refreshInFlightRef.current = refreshPromise;
    try {
      await refreshPromise;
    } finally {
      if (refreshInFlightRef.current === refreshPromise) {
        refreshInFlightRef.current = null;
      }
    }
  }, [controller, selectedLine.pollIntervalMs, updateSnapshot]);

  useEffect(() => {
    if (!lines.some(line => line.id === selectedLineId)) {
      setSelectedLineId(lines[0].id);
    }
  }, [lines, selectedLineId]);

  useEffect(() => {
    const client = new PlcClient(selectedLine, tabletAppConfig.tags, transportFactory);
    clientRef.current = client;
    updateSnapshot(client.snapshot());
    setIsConnected(false);
    setLastUpdatedText('Waiting for PLC data');
    setErrorText('');

    let disposed = false;
    const tick = async () => {
      if (!disposed && Date.now() >= nextPollAtRef.current) {
        await refreshNow();
      }
    };

    tick();
    const interval = setInterval(tick, selectedLine.pollIntervalMs);

    return () => {
      disposed = true;
      clearInterval(interval);
      refreshInFlightRef.current = null;
      nextPollAtRef.current = 0;
      controller.stopJog().catch(() => undefined);
      client.disconnect();
      clientRef.current = null;
    };
  }, [controller, refreshNow, selectedLine, transportFactory, updateSnapshot]);

  useEffect(() => {
    const subscription = AppState.addEventListener('change', state => {
      if (state !== 'active') {
        controller.stopJog().catch(() => undefined);
        forceRender(value => value + 1);
      }
    });
    return () => subscription.remove();
  }, [controller]);

  const selectLine = useCallback((lineId: string) => {
    controller.stopJog().catch(() => undefined);
    setSelectedLineId(lineId);
  }, [controller]);

  return {
    selectedLine,
    lines,
    snapshot,
    isConnected,
    lastUpdatedText,
    errorText,
    activeJogTag: controller.activeJogTag,
    controller,
    selectLine,
    refreshNow,
  };
};
