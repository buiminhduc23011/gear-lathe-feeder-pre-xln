import React from 'react';
import {fireEvent, render} from '@testing-library/react-native';
import {tabletAppConfig} from '../config/manualConfig';
import {ManualController} from '../manual/manualController';
import {ManualPage} from '../screens/ManualPage';
import type {ManualRuntime} from '../hooks/useManualRuntime';
import type {PlcSnapshot} from '../types/plc';

const createRuntime = (): ManualRuntime => {
  const snapshot = tabletAppConfig.tags.reduce<PlcSnapshot>((acc, tag) => {
    acc[tag.name] = tag.dataType === 'Bool' ? false : 0;
    return acc;
  }, {});
  const port = {
    isConnected: true,
    snapshot: () => snapshot,
    write: async (tagName: string, value: unknown) => {
      snapshot[tagName] = value;
    },
  };

  return {
    selectedLine: tabletAppConfig.lines[0],
    lines: tabletAppConfig.lines,
    snapshot,
    isConnected: true,
    lastUpdatedText: 'Last updated 10:00:00',
    errorText: '',
    activeJogTag: null,
    controller: new ManualController(tabletAppConfig.manualScreen, port),
    selectLine: jest.fn(),
    refreshNow: jest.fn(),
  };
};

describe('manual screen seed config', () => {
  it('keeps the desktop manual group counts', () => {
    expect(tabletAppConfig.manualScreen.originActions).toHaveLength(9);
    expect(tabletAppConfig.manualScreen.axes).toHaveLength(3);
    expect(tabletAppConfig.manualScreen.cylinders).toHaveLength(6);
  });

  it('renders origin, axis, and cylinder cards from config', () => {
    const runtime = createRuntime();
    const screen = render(<ManualPage runtime={runtime} />);

    expect(screen.getAllByTestId('manual-origin-card')).toHaveLength(9);

    fireEvent.press(screen.getByText('3 Trục'));
    expect(screen.getAllByTestId('manual-axis-card')).toHaveLength(3);

    fireEvent.press(screen.getByText('Xy Lanh'));
    expect(screen.getAllByTestId('manual-cylinder-card')).toHaveLength(6);
  });
});
