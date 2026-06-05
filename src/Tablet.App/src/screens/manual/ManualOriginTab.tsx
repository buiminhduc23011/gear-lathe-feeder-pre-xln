import React from 'react';
import {ScrollView, Text, View} from 'react-native';
import {HoldToRunButton} from '../../components/HoldToRunButton';
import type {ManualRuntime} from '../../hooks/useManualRuntime';
import type {ManualOriginActionState} from '../../manual/manualSelectors';
import {manualStyles, StatusReadout} from './ManualShared';
import type {CompactProps} from './types';

interface Props extends CompactProps {
  actions: ManualOriginActionState[];
  isConnected: boolean;
  controller: ManualRuntime['controller'];
}

export const ManualOriginTab = ({actions, isConnected, controller, compact}: Props) => (
  <ScrollView showsVerticalScrollIndicator={false} contentContainerStyle={[manualStyles.grid, compact && manualStyles.gridCompact]}>
    {actions.map(action => (
      <View key={action.config.commandTag} testID="manual-origin-card" style={[manualStyles.homeCard, compact && manualStyles.homeCardCompact]}>
        <View style={[manualStyles.cardHeader, compact && manualStyles.cardHeaderCompact]}>
          <View style={manualStyles.titleBlock}>
            <Text adjustsFontSizeToFit minimumFontScale={0.72} style={[manualStyles.cardTitle, compact && manualStyles.cardTitleCompact]} numberOfLines={1}>
              {action.config.title}
            </Text>
          </View>
          <StatusReadout
            compact={compact}
            label={action.isActive ? 'Homing' : action.isDone ? 'Home' : 'Lệch'}
            tone={action.isActive ? 'warning' : action.isDone ? 'ok' : 'idle'}
          />
        </View>
        <HoldToRunButton
          compact={compact}
          size="hero"
          label="CHẠY HOME"
          disabled={!isConnected}
          active={action.isActive || action.isCommandActive}
          onStart={() => controller.pressCommand(action.config.commandTag)}
          onStop={() => controller.releaseCommand(action.config.commandTag)}
        />
      </View>
    ))}
  </ScrollView>
);
