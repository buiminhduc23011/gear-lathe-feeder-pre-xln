import React from 'react';
import {act, fireEvent, render} from '@testing-library/react-native';
import {appPages} from '../navigation/appPages';
import {formatCurrentDateTime, NavigationPage} from '../screens/NavigationPage';

describe('navigation screen', () => {
  afterEach(() => {
    jest.useRealTimers();
  });

  it('renders one tile for each app page and navigates from a tile', () => {
    const onNavigate = jest.fn();
    const screen = render(<NavigationPage onNavigate={onNavigate} />);

    const tiles = screen.getAllByTestId('navigation-tile');
    expect(tiles).toHaveLength(appPages.length);

    fireEvent.press(tiles[1]);
    expect(onNavigate).toHaveBeenCalledWith(appPages[1].key);
  });

  it('shows the current device date and time and updates every second', () => {
    jest.useFakeTimers();
    jest.setSystemTime(new Date(2026, 5, 5, 14, 30, 45));

    const screen = render(<NavigationPage onNavigate={jest.fn()} />);
    expect(screen.getByText('Thứ Sáu, 05/06/2026 • 14:30:45')).toBeTruthy();

    act(() => {
      jest.advanceTimersByTime(1000);
    });

    expect(screen.getByText('Thứ Sáu, 05/06/2026 • 14:30:46')).toBeTruthy();
  });

  it('formats single-digit date and time parts with leading zeroes', () => {
    expect(formatCurrentDateTime(new Date(2026, 0, 4, 3, 2, 1))).toBe(
      'Chủ Nhật, 04/01/2026 • 03:02:01',
    );
  });
});
