import React from 'react';
import {fireEvent, render} from '@testing-library/react-native';
import {ioChannels, ioInputs, ioOutputs} from '../config/io';
import {plcTagByName} from '../config/plcTags';
import {buildIoCells, IOPage} from '../screens/IOPage';

describe('IOPage', () => {
  it('builds X/Y monitor cells from declared IO tag references', () => {
    const inputCells = buildIoCells('input', {'input.x00': true, 'input.x09': true, 'input.x14': true});
    const outputCells = buildIoCells('output', {'output.y06': true, 'output.y07': true});

    expect(inputCells).toHaveLength(ioInputs.length);
    expect(inputCells).toHaveLength(48);
    expect(inputCells[0].address).toBe('X00');
    expect(inputCells[inputCells.length - 1].address).toBe('X47');
    expect(inputCells.map(cell => cell.address)).toContain('X09');
    expect(inputCells.find(cell => cell.address === 'X00')?.active).toBe(true);
    expect(inputCells.find(cell => cell.address === 'X14')?.active).toBe(true);
    expect(inputCells.find(cell => cell.address === 'X14')?.subtitle).toBe('Input X14');
    expect(outputCells).toHaveLength(ioOutputs.length);
    expect(outputCells).toHaveLength(48);
    expect(outputCells[0].address).toBe('Y00');
    expect(outputCells[outputCells.length - 1].address).toBe('Y47');
    expect(outputCells.map(cell => cell.address)).toContain('Y09');
    expect(outputCells.filter(cell => cell.active).map(cell => cell.address)).toEqual(['Y06', 'Y07']);
  });

  it('switches between input and output tabs', () => {
    const screen = render(<IOPage snapshot={{'output.y06': true, 'output.y07': true}} />);

    expect(screen.getByText('Input')).toBeTruthy();
    expect(screen.getByText('Output')).toBeTruthy();
    expect(screen.queryByText(/40 TAGS/)).toBeNull();
    expect(screen.getAllByTestId('io-cell')).toHaveLength(ioInputs.length);
    expect(screen.getAllByTestId('io-cell')).toHaveLength(48);
    expect(screen.getByText('X00')).toBeTruthy();
    expect(screen.getByText('X47')).toBeTruthy();
    expect(screen.getByTestId('io-tab-input').props.accessibilityState).toEqual({selected: true});

    fireEvent.press(screen.getByTestId('io-tab-output'));

    expect(screen.getByTestId('io-tab-output').props.accessibilityState).toEqual({selected: true});
    expect(screen.getAllByTestId('io-cell')).toHaveLength(ioOutputs.length);
    expect(screen.getAllByTestId('io-cell')).toHaveLength(48);
    expect(screen.queryByText('X00')).toBeNull();
    expect(screen.getByText('Y00')).toBeTruthy();
    expect(screen.getByText('Y07')).toBeTruthy();
    expect(screen.getByText('Y47')).toBeTruthy();
    expect(screen.queryByText('2/40')).toBeNull();
  });

  it('keeps IO point tag references mapped to PLC tags', () => {
    for (const channel of ioChannels) {
      for (const point of channel.points) {
        const tag = plcTagByName.get(point.tagName);
        const pointNumber = Number(point.displayAddress.slice(1));
        const expectedAddress = channel.key === 'input' ? `M${pointNumber}` : `Y${pointNumber}`;

        expect(tag).toBeDefined();
        expect(tag?.address).toBe(expectedAddress);
        expect(tag?.writable).not.toBe(true);
      }
    }
  });
});
