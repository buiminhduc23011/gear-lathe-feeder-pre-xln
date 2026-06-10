import React, {useCallback, useMemo, useRef, useState} from 'react';
import {AppShell} from './components/AppShell';
import {ioChannels} from './config/io';
import {AlarmBarState} from './components/AlarmBar';
import {useLineSettings} from './hooks/useLineSettings';
import {useManualRuntime, type ManualRuntime} from './hooks/useManualRuntime';
import type {AppPage} from './navigation/appPages';
import {IOPage} from './screens/IOPage';
import {ManualPage} from './screens/ManualPage';
import {NavigationPage} from './screens/NavigationPage';
import {PlaceholderPage} from './screens/PlaceholderPage';
import {SettingsPage} from './screens/SettingsPage';
import type {PlcSnapshot} from './types/plc';

type AppScreen = AppPage | 'navigation';

const ioSnapshotTagNames = ioChannels.flatMap(channel => channel.points.map(point => point.tagName));

const useStableSnapshotSelection = (source: PlcSnapshot, tagNames: readonly string[]): PlcSnapshot => {
  const selectedRef = useRef<PlcSnapshot>({});
  const previous = selectedRef.current;
  let changed = Object.keys(previous).length !== tagNames.length;
  const next: PlcSnapshot = {};

  for (const tagName of tagNames) {
    const value = source[tagName];
    next[tagName] = value;
    if (!Object.prototype.hasOwnProperty.call(previous, tagName) || !Object.is(previous[tagName], value)) {
      changed = true;
    }
  }

  if (!changed) {
    return previous;
  }

  selectedRef.current = next;
  return next;
};

// Simplified screen metadata mapping for HMI header
const screenMetadata: Record<AppScreen, {eyebrow: string; title: string}> = {
  navigation: {eyebrow: 'GEAR LINE', title: 'Bảng điều khiển'},
  auto: {eyebrow: 'GEAR LINE', title: 'Tự động'},
  manual: {eyebrow: 'GEAR LINE', title: 'Bằng tay'},
  io: {eyebrow: 'GEAR LINE', title: 'Giám sát IO'},
  history: {eyebrow: 'GEAR LINE', title: 'Lịch sử'},
  settings: {eyebrow: 'GEAR LINE', title: 'Cài đặt'},
};

export const buildAlarmBarModel = (runtime: ManualRuntime): {text: string; state: AlarmBarState} => {
  if (runtime.isConnected) {
    return {
      state: AlarmBarState.Normal,
      text: 'Đã kết nối PLC thực tế',
    };
  }

  return {
    state: AlarmBarState.Error,
    text: 'Chưa kết nối PLC thực tế',
  };
};

const App = () => {
  const [currentPage, setCurrentPage] = useState<AppScreen>('navigation');
  const lineSettings = useLineSettings();
  const runtime = useManualRuntime(lineSettings.lines);
  const screenMeta = screenMetadata[currentPage];
  const alarmBar = buildAlarmBarModel(runtime);
  const ioSnapshot = useStableSnapshotSelection(runtime.snapshot, ioSnapshotTagNames);
  const handleHome = useCallback(() => setCurrentPage('navigation'), []);
  const navigationContent = useMemo(() => <NavigationPage onNavigate={setCurrentPage} />, []);
  const autoContent = useMemo(() => <PlaceholderPage />, []);
  const historyContent = useMemo(() => <PlaceholderPage />, []);
  const manualContent = useMemo(() => <ManualPage runtime={runtime} />, [runtime]);
  const ioContent = useMemo(() => <IOPage snapshot={ioSnapshot} />, [ioSnapshot]);
  const settingsContent = useMemo(() => (
    <SettingsPage
      lines={lineSettings.lines}
      onAddLine={lineSettings.addLine}
      onDeleteLine={lineSettings.deleteLine}
      onSelectLine={runtime.selectLine}
      onSaveLine={lineSettings.saveLine}
      selectedLineId={runtime.selectedLine.id}
    />
  ), [
    lineSettings.addLine,
    lineSettings.deleteLine,
    lineSettings.lines,
    lineSettings.saveLine,
    runtime.selectLine,
    runtime.selectedLine.id,
  ]);

  const content = (() => {
    switch (currentPage) {
      case 'navigation':
        return navigationContent;
      case 'manual':
        return manualContent;
      case 'auto':
        return autoContent;
      case 'io':
        return ioContent;
      case 'history':
        return historyContent;
      case 'settings':
        return settingsContent;
    }
  })();

  return (
    <AppShell
      isConnected={runtime.isConnected}
      showHomeButton={currentPage !== 'navigation'}
      screenEyebrow={screenMeta.eyebrow}
      screenTitle={screenMeta.title}
      alarmText={alarmBar.text}
      alarmState={alarmBar.state}
      onHome={handleHome}
      currentPage={currentPage}
      onNavigate={setCurrentPage}>
      {content}
    </AppShell>
  );
};

export default App;
