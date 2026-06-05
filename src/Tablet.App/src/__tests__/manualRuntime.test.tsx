import React, {useCallback, useMemo} from 'react';
import {act, render} from '@testing-library/react-native';
import {Text} from 'react-native';
import {useManualRuntime} from '../hooks/useManualRuntime';
import type {LineConfig} from '../types/plc';

const line: LineConfig = {
  id: 'line1',
  name: 'Line 1',
  host: '127.0.0.1',
  port: 503,
  slaveId: 1,
  pollIntervalMs: 100,
};

const Harness = ({transport}: {transport: {isConnected: boolean; connect: jest.Mock; disconnect: jest.Mock; request: jest.Mock}}) => {
  const lines = useMemo(() => [line], []);
  const transportFactory = useCallback(() => transport, [transport]);
  const runtime = useManualRuntime(lines, transportFactory);
  return <Text>{runtime.errorText}</Text>;
};

describe('useManualRuntime polling', () => {
  afterEach(() => {
    jest.useRealTimers();
  });

  it('does not start overlapping PLC reads while one is still in flight', async () => {
    jest.useFakeTimers();
    let rejectRequest: ((error: Error) => void) | undefined;
    const transport = {
      isConnected: false,
      connect: jest.fn(),
      disconnect: jest.fn(),
      request: jest.fn(() => new Promise((_, reject) => {
        rejectRequest = reject;
      })),
    };

    const screen = render(<Harness transport={transport} />);
    await act(async () => undefined);

    expect(transport.request).toHaveBeenCalledTimes(1);

    act(() => {
      jest.advanceTimersByTime(500);
    });

    expect(transport.request).toHaveBeenCalledTimes(1);

    await act(async () => {
      rejectRequest?.(new Error('offline'));
      await Promise.resolve();
    });
    screen.unmount();
  });
});
