import {useCallback, useMemo, useRef, useState} from 'react';
import {tabletAppConfig} from '../config/manualConfig';
import {ManualController} from '../manual/manualController';
import type {LineConfig, PlcSnapshot, PlcTag} from '../types/plc';

export interface ManualRuntime {
  selectedLine: LineConfig;
  lines: LineConfig[];
  snapshot: PlcSnapshot;
  isConnected: boolean;
  lastUpdatedText: string;
  errorText: string;
  activeJogTag: string | null;
  controller: ManualController;
  selectLine(lineId: string): void;
  refreshNow(): Promise<void>;
}

export const useManualRuntime = (
  configuredLines: LineConfig[] = tabletAppConfig.lines,
): ManualRuntime => {
  const lines = configuredLines.length > 0 ? configuredLines : tabletAppConfig.lines;
  const [selectedLineId, setSelectedLineId] = useState(lines[0].id);
  const selectedLine = lines.find(line => line.id === selectedLineId) ?? lines[0];
  const [snapshot, setSnapshot] = useState<PlcSnapshot>(() => createPreviewSnapshot());
  const snapshotRef = useRef(snapshot);

  const updateSnapshot = useCallback((nextSnapshot: PlcSnapshot) => {
    snapshotRef.current = nextSnapshot;
    setSnapshot(nextSnapshot);
  }, []);

  const controller = useMemo(() => {
    const plcPort = {
      get isConnected() {
        return true;
      },
      snapshot: () => snapshotRef.current,
      write: async (tagName: string, value: unknown) => {
        updateSnapshot({
          ...snapshotRef.current,
          [tagName]: value,
        });
      },
    };

    return new ManualController(tabletAppConfig.manualScreen, plcPort);
  }, [updateSnapshot]);

  const selectLine = useCallback((lineId: string) => {
    controller.stopJog().catch(() => undefined);
    setSelectedLineId(lineId);
  }, [controller]);

  const refreshNow = useCallback(async () => {
    updateSnapshot({
      ...snapshotRef.current,
      'manual.current_position_x': Number(snapshotRef.current['manual.current_position_x'] ?? 0) + 0.1,
      'manual.current_position_y': Number(snapshotRef.current['manual.current_position_y'] ?? 0) + 0.05,
    });
  }, [updateSnapshot]);

  return {
    selectedLine,
    lines,
    snapshot,
    isConnected: true,
    lastUpdatedText: 'Web preview data',
    errorText: '',
    activeJogTag: controller.activeJogTag,
    controller,
    selectLine,
    refreshNow,
  };
};

const createPreviewSnapshot = (): PlcSnapshot => {
  const snapshot: PlcSnapshot = {};

  for (const tag of tabletAppConfig.tags) {
    snapshot[tag.name] = previewDefaultValue(tag);
  }

  return {
    ...snapshot,
    'input.x0_09': true,
    'output.y0_06': true,
    'output.y0_07': true,
    'manual.is_homed_x': true,
    'manual.is_homed_y': true,
    'manual.is_homed_z': true,
    'manual.home_cart_1_clamp_done': true,
    'manual.home_cart_2_clamp_done': true,
    'manual.home_rotate_cylinder_done': true,
    'manual.home_tool_clamp_done': true,
    'manual.home_tool_change_done': true,
    'manual.cart_1_closed_signal': true,
    'manual.cart_2_opened_signal': true,
    'manual.small_part_opened_signal': true,
    'manual.large_part_opened_signal': true,
    'manual.rotated_to_0_signal': true,
    'manual.changed_tool_to_0_signal': true,
    'manual.manual_speed_x': 80,
    'manual.manual_speed_y': 70,
    'manual.manual_speed_z': 35,
    'manual.move_point_x': 240,
    'manual.move_point_y': 160,
    'manual.move_point_z': 42,
    'manual.current_position_x': 126.4,
    'manual.current_position_y': 84.2,
    'manual.current_position_z': 31.8,
    'data.max_speed_x': 260,
    'data.max_speed_y': 220,
    'data.max_speed_z': 90,
    'data.limit_x_positive': 520,
    'data.limit_x_negative': -20,
    'data.limit_y_positive': 360,
    'data.limit_y_negative': -15,
    'data.limit_z_positive': 120,
    'data.limit_z_negative': 0,
  };
};

const previewDefaultValue = (tag: PlcTag): unknown => {
  switch (tag.dataType) {
    case 'Bool':
      return false;
    case 'Int16':
    case 'Int32':
    case 'Float':
      return 0;
    case 'String':
      return '';
  }
};
