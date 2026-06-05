import React from 'react';
import {fireEvent, render} from '@testing-library/react-native';
import {appPages} from '../navigation/appPages';
import {NavigationPage} from '../screens/NavigationPage';

describe('navigation screen', () => {
  it('renders one tile for each app page and navigates from a tile', () => {
    const onNavigate = jest.fn();
    const screen = render(<NavigationPage onNavigate={onNavigate} />);

    const tiles = screen.getAllByTestId('navigation-tile');
    expect(tiles).toHaveLength(appPages.length);

    fireEvent.press(tiles[1]);
    expect(onNavigate).toHaveBeenCalledWith(appPages[1].key);
  });
});
