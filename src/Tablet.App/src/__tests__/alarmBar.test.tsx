import React from 'react';
import {render} from '@testing-library/react-native';
import {AlarmBar, AlarmBarState} from '../components/AlarmBar';

describe('alarm bar', () => {
  it.each([
    [AlarmBarState.Normal, 'STATUS'],
    [AlarmBarState.Warning, 'WARNING'],
    [AlarmBarState.Error, 'ALARM'],
  ])('renders %s state with the expected label', (state, label) => {
    const screen = render(<AlarmBar state={state} text="Machine message" />);

    expect(screen.getAllByText(label)).toHaveLength(2);
    expect(screen.getByLabelText(`${label}: Machine message`)).toBeTruthy();
  });
});
