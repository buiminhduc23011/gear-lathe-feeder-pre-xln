import React, {useState} from 'react';
import {AppShell} from './components/AppShell';
import type {AlarmTone} from './components/AlarmTicker';
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

const buildAlarmState = (runtime: ManualRuntime): {text: string; tone: AlarmTone} => {
  if (runtime.errorText) {
    return {
      tone: 'fault',
      text: `PLC ERROR | ${compactPlcError(runtime.errorText)}`,
    };
  }

  if (runtime.activeJogTag) {
    return {
      tone: 'warning',
      text: `JOG ACTIVE | ${runtime.activeJogTag}`,
    };
  }

  if (!runtime.isConnected) {
    return {
      tone: 'fault',
      text: `PLC OFFLINE | Active ${runtime.selectedLine.name} | Waiting for PLC data`,
    };
  }

  return {
    tone: 'ok',
    text: `SYSTEM READY | Active ${runtime.selectedLine.name} | ${runtime.lastUpdatedText || 'PLC online'}`,
  };
};

const App = () => {
  const [currentPage, setCurrentPage] = useState<AppScreen>('navigation');
  const lineSettings = useLineSettings();
  const runtime = useManualRuntime(lineSettings.lines);
  const screenMeta = screenMetadata[currentPage];
  const alarmState = buildAlarmState(runtime);

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
      alarmText={alarmState.text}
      alarmTone={alarmState.tone}
      onHome={() => setCurrentPage('navigation')}
      currentPage={currentPage}
      onNavigate={setCurrentPage}>
      {content}
    </AppShell>
  );
};

export default App;
