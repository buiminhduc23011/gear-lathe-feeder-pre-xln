import {tabletAppConfig} from '../config/manualConfig';
import {ManualController, type ManualPlcPort} from '../manual/manualController';
import type {PlcSnapshot} from '../types/plc';

class FakePlc implements ManualPlcPort {
  isConnected = true;
  writes: Array<{tagName: string; value: unknown}> = [];
  values: PlcSnapshot = {};

  constructor(initial: PlcSnapshot = {}) {
    this.values = {...initial};
  }

  snapshot(): PlcSnapshot {
    return this.values;
  }

  async write(tagName: string, value: unknown): Promise<void> {
    this.writes.push({tagName, value});
    this.values[tagName] = value;
  }
}

describe('ManualController', () => {
  it('writes one-shot command true without resetting it', async () => {
    const plc = new FakePlc();
    const controller = new ManualController(tabletAppConfig.manualScreen, plc);

    await controller.runOneShot('manual.home_x');

    expect(plc.writes).toEqual([{tagName: 'manual.home_x', value: true}]);
  });

  it('clears the opposite cylinder command before activating a cylinder', async () => {
    const plc = new FakePlc({'manual.change_tool_to_180': true});
    const controller = new ManualController(tabletAppConfig.manualScreen, plc);

    await controller.runOneShot('manual.change_tool_to_0');

    expect(plc.writes).toEqual([
      {tagName: 'manual.change_tool_to_180', value: false},
      {tagName: 'manual.change_tool_to_0', value: true},
    ]);
  });

  it('holds jog true on press and false on release', async () => {
    const plc = new FakePlc();
    const controller = new ManualController(tabletAppConfig.manualScreen, plc);

    await controller.startJog('manual.move_x_forward');
    await controller.stopJog('manual.move_x_forward');

    expect(plc.writes).toEqual([
      {tagName: 'manual.move_x_forward', value: true},
      {tagName: 'manual.move_x_forward', value: false},
    ]);
  });

  it('disables commands when PLC is offline or interlock is active', async () => {
    const plc = new FakePlc({'alarm.estop': true});
    const controller = new ManualController(tabletAppConfig.manualScreen, plc);

    await controller.runOneShot('manual.home_x');
    expect(plc.writes).toHaveLength(0);

    plc.values = {};
    plc.isConnected = false;
    await controller.runOneShot('manual.home_x');
    expect(plc.writes).toHaveLength(0);
  });

  it('clears stale jog tags that are not actively held', async () => {
    const plc = new FakePlc({'manual.move_x_forward': true});
    const controller = new ManualController(tabletAppConfig.manualScreen, plc);

    await controller.clearStaleJogTags();

    expect(plc.writes).toEqual([{tagName: 'manual.move_x_forward', value: false}]);
  });
});
