import React from 'react';
import {ScrollView, Text, View} from 'react-native';
import {HoldToRunButton} from '../../components/HoldToRunButton';
import type {ManualRuntime} from '../../hooks/useManualRuntime';
import {formatManualNumber, type ManualAxisState} from '../../manual/manualSelectors';
import {manualStyles, MetricCell, StatusReadout, type Tone} from './ManualShared';
import type {CompactProps} from './types';

interface Props extends CompactProps {
  axes: ManualAxisState[];
  isConnected: boolean;
  controller: ManualRuntime['controller'];
}

export const ManualAxisTab = ({axes, isConnected, controller, compact}: Props) => (
  <ScrollView showsVerticalScrollIndicator={false} contentContainerStyle={[manualStyles.grid, compact && manualStyles.gridCompact]}>
    {axes.map(axis => (
      <AxisCard key={axis.config.key} axis={axis} compact={compact} isConnected={isConnected} controller={controller} />
    ))}
  </ScrollView>
);

const AxisCard = ({
  axis,
  isConnected,
  controller,
  compact,
}: {
  axis: ManualAxisState;
  isConnected: boolean;
  controller: ManualRuntime['controller'];
} & CompactProps) => {
  const limitText = axis.isNegativeLimitActive ? 'LIMIT -' : axis.isPositiveLimitActive ? 'LIMIT +' : 'OK';
  const statusLabel = axis.isHomed ? 'Home' : axis.isHoming ? 'Homing' : 'Chưa';
  const tone: Tone = axis.isHomed ? 'ok' : axis.isHoming ? 'warning' : 'idle';

  return (
    <View testID="manual-axis-card" style={[manualStyles.axisCard, compact && manualStyles.axisCardCompact]}>
      <View style={[manualStyles.cardHeader, compact && manualStyles.cardHeaderCompact]}>
        <View style={manualStyles.titleBlock}>
          <Text adjustsFontSizeToFit minimumFontScale={0.72} style={[manualStyles.cardTitle, compact && manualStyles.cardTitleCompact]} numberOfLines={1}>
            {axis.config.displayName}
          </Text>
          <View style={manualStyles.positionRow}>
            <Text style={[manualStyles.positionValue, compact && manualStyles.positionValueCompact]} numberOfLines={1}>
              {formatManualNumber(axis.currentPosition)}
            </Text>
            <Text style={manualStyles.unit}>mm</Text>
          </View>
        </View>
        <StatusReadout compact={compact} label={statusLabel} tone={tone} />
      </View>

      <View style={manualStyles.metricRow}>
        <MetricCell label="SERVO" value={axis.isServoOn ? 'ON' : 'OFF'} compact={compact} />
        <MetricCell label="LIMIT" value={limitText} compact={compact} />
      </View>

      <View style={manualStyles.jogRow}>
        <HoldToRunButton
          compact={compact}
          label={axis.config.negativeLabel}
          disabled={!isConnected || !axis.canJogNegative}
          active={axis.isNegativeJogActive}
          onStart={() => controller.pressCommand(axis.config.negativeJogTag)}
          onStop={() => controller.releaseCommand(axis.config.negativeJogTag)}
        />
        <HoldToRunButton
          compact={compact}
          label="HOME"
          disabled={!isConnected}
          active={axis.isHomeCommandActive || axis.isHoming}
          onStart={() => controller.pressCommand(axis.config.homeTag)}
          onStop={() => controller.releaseCommand(axis.config.homeTag)}
        />
        <HoldToRunButton
          compact={compact}
          label={axis.config.positiveLabel}
          disabled={!isConnected || !axis.canJogPositive}
          active={axis.isPositiveJogActive}
          onStart={() => controller.pressCommand(axis.config.positiveJogTag)}
          onStop={() => controller.releaseCommand(axis.config.positiveJogTag)}
        />
      </View>
    </View>
  );
};
