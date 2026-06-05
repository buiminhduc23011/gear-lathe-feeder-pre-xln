import React, {useState} from 'react';
import {AppShell} from './components/AppShell';
import {AlarmBarState} from './components/AlarmBar';
import {useLineSettings} from './hooks/useLineSettings';
import {useManualRuntime, type ManualRuntime} from './hooks/useManualRuntime';
import type {AppPage} from './navigation/appPages';
import {ManualPage} from './screens/ManualPage';
import {NavigationPage} from './screens/NavigationPage';
import {PlaceholderPage} from './screens/PlaceholderPage';
import {SettingsPage} from './screens/SettingsPage';

type AppScreen = AppPage | 'navigation';

// Simplified screen metadata mapping for HMI header
const screenMetadata: Record<AppScreen, {eyebrow: string; title: string}> = {
  navigation: {eyebrow: 'GEAR LINE', title: 'Bảng điều khiển'},
  auto: {eyebrow: 'GEAR LINE', title: 'Tự động'},
  manual: {eyebrow: 'GEAR LINE', title: 'Bằng tay'},
  io: {eyebrow: 'GEAR LINE', title: 'Giám sát IO'},
  history: {eyebrow: 'GEAR LINE', title: 'Lịch sử'},
  settings: {eyebrow: 'GEAR LINE', title: 'Cài đặt'},
};

const compactPlcError = (errorText: string): string => {
  const normalizedText = errorText.replace(/\s+/g, ' ').trim();
  if (/connect|ECONNREFUSED|ENETUNREACH|EHOSTUNREACH|timed out/i.test(normalizedText)) {
    return 'Mất kết nối PLC';
  }

  return normalizedText || 'Lỗi PLC';
};

const buildAlarmBarModel = (runtime: ManualRuntime): {text: string; state: AlarmBarState} => {
  if (runtime.errorText) {
    return {
      state: AlarmBarState.Error,
      text: `PLC ERROR | ${compactPlcError(runtime.errorText)}`,
    };
  }

  if (runtime.activeJogTag) {
    return {
      state: AlarmBarState.Warning,
      text: `JOG ACTIVE | ${runtime.activeJogTag}`,
    };
  }

  if (!runtime.isConnected) {
    return {
      state: AlarmBarState.Error,
      text: `PLC OFFLINE | Active ${runtime.selectedLine.name} | Waiting for PLC data`,
    };
  }

  return {
    state: AlarmBarState.Normal,
    text: `${runtime.lastUpdatedText && /preview/i.test(runtime.lastUpdatedText) ? 'preview data' : runtime.lastUpdatedText || 'preview data'} | SYSTEM READY | AC`,
  };
};

const App = () => {
  const [currentPage, setCurrentPage] = useState<AppScreen>('navigation');
  const lineSettings = useLineSettings();
  const runtime = useManualRuntime(lineSettings.lines);
  const screenMeta = screenMetadata[currentPage];
  const alarmBar = buildAlarmBarModel(runtime);

  const content = (() => {
    switch (currentPage) {
      case 'navigation':
        return <NavigationPage onNavigate={setCurrentPage} />;
      case 'manual':
        return <ManualPage runtime={runtime} />;
      case 'auto':
        return <PlaceholderPage title="Tự động" />;
      case 'io':
        return <PlaceholderPage title="Giám sát IO" />;
      case 'history':
        return <PlaceholderPage title="Lịch sử" />;
      case 'settings':
        return (
          <SettingsPage
            lines={lineSettings.lines}
            onAddLine={lineSettings.addLine}
            onDeleteLine={lineSettings.deleteLine}
            onSelectLine={runtime.selectLine}
            onSaveLine={lineSettings.saveLine}
            selectedLineId={runtime.selectedLine.id}
          />
        );
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
      onHome={() => setCurrentPage('navigation')}
      currentPage={currentPage}
      onNavigate={setCurrentPage}>
      {content}
    </AppShell>
  );
};

export default App;
