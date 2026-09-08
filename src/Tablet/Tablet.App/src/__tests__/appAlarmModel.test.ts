import {AlarmBarState} from '../components/AlarmBar';
import {buildAlarmBarModel} from '../App';
import type {ManualRuntime} from '../hooks/useManualRuntime';
import type {LineConfig} from '../types/plc';

const line: LineConfig = {
  id: 'line1',
  name: 'Line 1',
  host: '127.0.0.1',
  port: 503,
  slaveId: 1,
  pollIntervalMs: 100,
  connectionType: 'TcpAS',
};

const runtime = (overrides: Partial<ManualRuntime>): ManualRuntime => ({
  selectedLine: line,
  lines: [line],
  snapshot: {},
  isConnected: false,
  lastUpdatedText: 'Last updated 10:00:00',
  errorText: '',
  activeJogTag: null,
  controller: {} as ManualRuntime['controller'],
  selectLine: jest.fn(),
  refreshNow: jest.fn(async () => undefined),
  ...overrides,
});

describe('app alarm bar model', () => {
  it('shows only the real PLC connection status when connected', () => {
    expect(
      buildAlarmBarModel(runtime({isConnected: true, errorText: 'socket noise', activeJogTag: 'manual.move_x_forward'})),
    ).toEqual({
      state: AlarmBarState.Normal,
      text: 'Đã kết nối PLC thực tế',
    });
  });

  it('shows only the real PLC connection status when disconnected', () => {
    expect(buildAlarmBarModel(runtime({isConnected: false, errorText: 'ECONNREFUSED'}))).toEqual({
      state: AlarmBarState.Error,
      text: 'Chưa kết nối PLC thực tế',
    });
  });
});
