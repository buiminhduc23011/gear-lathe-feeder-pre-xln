import React from 'react';
import {fireEvent, render} from '@testing-library/react-native';
import {Text} from 'react-native';
import {AppShell} from '../components/AppShell';

describe('app shell header', () => {
  it('uses an icon home button and keeps line controls out of the header', () => {
    const onHome = jest.fn();
    const screen = render(
      <AppShell
        alarmText="PLC ERROR | Door guard open"
        alarmTone="fault"
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
    expect(screen.getByText('ALARM')).toBeTruthy();
    expect(screen.getByLabelText('ALARM: PLC ERROR | Door guard open')).toBeTruthy();
  });
});
