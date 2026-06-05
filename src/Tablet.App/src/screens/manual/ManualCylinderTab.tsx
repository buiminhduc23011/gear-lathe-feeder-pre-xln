import React from 'react';
import {ScrollView, Text, View} from 'react-native';
import {HoldToRunButton} from '../../components/HoldToRunButton';
import type {ManualRuntime} from '../../hooks/useManualRuntime';
import type {ManualCylinderState} from '../../manual/manualSelectors';
import {manualStyles} from './ManualShared';
import type {CompactProps} from './types';

interface Props extends CompactProps {
  cylinders: ManualCylinderState[];
  isConnected: boolean;
  controller: ManualRuntime['controller'];
}

export const ManualCylinderTab = ({cylinders, isConnected, controller, compact}: Props) => (
  <ScrollView showsVerticalScrollIndicator={false} contentContainerStyle={[manualStyles.grid, compact && manualStyles.gridCompact]}>
    {cylinders.map(cylinder => (
      <View key={cylinder.config.key} testID="manual-cylinder-card" style={[manualStyles.cylinderCard, compact && manualStyles.cylinderCardCompact]}>
        <View style={[manualStyles.cardHeader, compact && manualStyles.cardHeaderCompact]}>
          <View style={manualStyles.titleBlock}>
            <View style={manualStyles.titleInlineRow}>
              <Text adjustsFontSizeToFit minimumFontScale={0.72} style={[manualStyles.cardTitle, manualStyles.inlineTitle, compact && manualStyles.cardTitleCompact]} numberOfLines={1}>
                {cylinder.config.title}
              </Text>
              <Text style={[manualStyles.inlineSubtitle, compact && manualStyles.inlineSubtitleCompact]} numberOfLines={1}>
                {getCylinderFeedbackText(cylinder)}
              </Text>
            </View>
          </View>
        </View>
        <View style={manualStyles.twoButtonRow}>
          <HoldToRunButton
            compact={compact}
            label={cylinder.config.primaryActionLabel}
            disabled={!isConnected}
            active={cylinder.isPrimaryCommandActive}
            onStart={() => controller.pressCommand(cylinder.config.primaryCommandTag)}
            onStop={() => controller.releaseCommand(cylinder.config.primaryCommandTag)}
          />
          <HoldToRunButton
            compact={compact}
            label={cylinder.config.secondaryActionLabel}
            disabled={!isConnected}
            active={cylinder.isSecondaryCommandActive}
            onStart={() => controller.pressCommand(cylinder.config.secondaryCommandTag)}
            onStop={() => controller.releaseCommand(cylinder.config.secondaryCommandTag)}
          />
        </View>
      </View>
    ))}
  </ScrollView>
);

const getCylinderFeedbackText = (cylinder: ManualCylinderState): string => {
  if (cylinder.isPrimaryFeedbackActive) {
    return cylinder.config.primaryFeedbackLabel;
  }

  if (cylinder.isSecondaryFeedbackActive) {
    return cylinder.config.secondaryFeedbackLabel;
  }

  return 'Chưa có tín hiệu';
};
