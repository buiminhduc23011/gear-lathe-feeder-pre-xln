import React, {useState} from 'react';
import {fireEvent, render} from '@testing-library/react-native';
import {SettingsPage} from '../screens/SettingsPage';
import type {LineConfig} from '../types/plc';

const line1: LineConfig = {id: 'line1', name: 'Line 1', host: '127.0.0.1', port: 503, slaveId: 1, pollIntervalMs: 100};
const line2: LineConfig = {id: 'line2', name: 'Line 2', host: '192.168.1.20', port: 504, slaveId: 2, pollIntervalMs: 100};
const line3: LineConfig = {id: 'line3', name: 'Line 3', host: '192.168.1.30', port: 505, slaveId: 3, pollIntervalMs: 100};

describe('settings line page', () => {
  it('saves edited line connection values', () => {
    const onSaveLine = jest.fn();
    const screen = render(
      <SettingsPage
        lines={[line1]}
        onAddLine={() => line2}
        onDeleteLine={() => [line1]}
        onSaveLine={onSaveLine}
        selectedLineId="line1"
      />,
    );

    fireEvent.changeText(screen.getByDisplayValue('127.0.0.1'), '192.168.1.10');
    fireEvent.changeText(screen.getByDisplayValue('503'), '1503');
    fireEvent.press(screen.getByText('Lưu line'));

    expect(onSaveLine).toHaveBeenCalledWith({
      ...line1,
      host: '192.168.1.10',
      port: 1503,
    });
  });

  it('adds and deletes lines through callbacks', () => {
    const onAddLine = jest.fn(() => line3);
    const onDeleteLine = jest.fn((lineId: string) => [line1].filter(line => line.id !== lineId));

    const Wrapper = () => {
      const [lines, setLines] = useState([line1, line2]);
      return (
        <SettingsPage
          lines={lines}
          onAddLine={() => {
            const nextLine = onAddLine();
            setLines(current => [...current, nextLine]);
            return nextLine;
          }}
          onDeleteLine={lineId => {
            onDeleteLine(lineId);
            const nextLines = lines.filter(line => line.id !== lineId);
            setLines(nextLines);
            return nextLines;
          }}
          onSaveLine={jest.fn()}
          selectedLineId="line2"
        />
      );
    };

    const screen = render(<Wrapper />);

    fireEvent.press(screen.getByText('Thêm line'));
    expect(onAddLine).toHaveBeenCalledTimes(1);

    fireEvent.press(screen.getByText('Xóa line'));
    expect(onDeleteLine).toHaveBeenCalledWith('line3');
  });

  it('selects the active line from the settings line list', () => {
    const onSelectLine = jest.fn();
    const screen = render(
      <SettingsPage
        lines={[line1, line2]}
        onAddLine={() => line3}
        onDeleteLine={() => [line1]}
        onSaveLine={jest.fn()}
        onSelectLine={onSelectLine}
        selectedLineId="line2"
      />,
    );

    fireEvent.press(screen.getByText('Line 1'));
    expect(onSelectLine).toHaveBeenCalledWith('line1');
  });
});
