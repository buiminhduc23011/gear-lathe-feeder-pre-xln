import React from 'react';
import {fireEvent, render} from '@testing-library/react-native';
import {Text} from 'react-native';
import {AlarmBarState} from '../components/AlarmBar';
import {AppShell} from '../components/AppShell';

describe('app shell header', () => {
  it('uses the logo as home and renders static alarm content', () => {
    const onHome = jest.fn();
    const screen = render(
      <AppShell
        alarmText="PLC ERROR | Door guard open"
        alarmState={AlarmBarState.Error}
        isConnected={false}
        onHome={onHome}
        showHomeButton>
        <Text>Body</Text>
      </AppShell>,
    );

    fireEvent.press(screen.getByLabelText('Home'));
    expect(onHome).toHaveBeenCalledTimes(1);

    expect(screen.queryByText('LINE-HMI')).toBeNull();
    expect(screen.queryByText('ENDPOINT')).toBeNull();
    expect(screen.queryByText('LINE')).toBeNull();
    expect(screen.getAllByText('ALARM')).toHaveLength(1);
    expect(screen.getByTestId('alarm-content')).toBeTruthy();
    expect(screen.getByLabelText('ALARM: PLC ERROR | Door guard open')).toBeTruthy();
  });
});
