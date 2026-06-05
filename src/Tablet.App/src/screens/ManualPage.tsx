import React, {useMemo, useState} from 'react';
import {StyleSheet, useWindowDimensions, View} from 'react-native';
import {tabletAppConfig} from '../config/manualConfig';
import type {ManualRuntime} from '../hooks/useManualRuntime';
import {getAxisStates, getCylinderStates, getOriginActionStates} from '../manual/manualSelectors';
import {colors} from '../styles/theme';
import {ManualAxisTab} from './manual/ManualAxisTab';
import {ManualCylinderTab} from './manual/ManualCylinderTab';
import {ManualGroupTabs} from './manual/ManualGroupTabs';
import {ManualOriginTab} from './manual/ManualOriginTab';
import type {ManualGroup} from './manual/types';

interface Props {
  runtime: ManualRuntime;
}

export const ManualPage = ({runtime}: Props) => {
  const [selectedGroup, setSelectedGroup] = useState<ManualGroup>('origin');
  const {height, width} = useWindowDimensions();
  const compact = width <= 900 || height <= 520;
  const config = tabletAppConfig.manualScreen;
  const axes = useMemo(() => getAxisStates(config, runtime.snapshot), [config, runtime.snapshot]);
  const cylinders = useMemo(() => getCylinderStates(config, runtime.snapshot), [config, runtime.snapshot]);
  const origins = useMemo(() => getOriginActionStates(config, runtime.snapshot), [config, runtime.snapshot]);

  return (
    <View style={styles.root}>
      <ManualGroupTabs compact={compact} selectedGroup={selectedGroup} onSelect={setSelectedGroup} />
      <View style={styles.panel}>
        {selectedGroup === 'origin' ? (
          <ManualOriginTab actions={origins} compact={compact} isConnected={runtime.isConnected} controller={runtime.controller} />
        ) : null}
        {selectedGroup === 'axis' ? (
          <ManualAxisTab axes={axes} compact={compact} isConnected={runtime.isConnected} controller={runtime.controller} />
        ) : null}
        {selectedGroup === 'cylinder' ? (
          <ManualCylinderTab cylinders={cylinders} compact={compact} isConnected={runtime.isConnected} controller={runtime.controller} />
        ) : null}
      </View>
    </View>
  );
};

const styles = StyleSheet.create({
  root: {
    flex: 1,
    minHeight: 0,
    borderWidth: 1,
    borderColor: '#263B4A',
    backgroundColor: 'rgba(4, 12, 18, 0.92)',
  },
  panel: {
    flex: 1,
    minHeight: 0,
  },
});
