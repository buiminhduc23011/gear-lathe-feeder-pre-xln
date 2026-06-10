import React from 'react';
import {StyleSheet, Text, View} from 'react-native';
import {colors, spacing, typography} from '../../styles/theme';
import type {CompactProps} from './types';

export type Tone = 'ok' | 'warning' | 'idle';

export const StatusReadout = ({label, tone: _tone, compact}: {label: string; tone: Tone} & CompactProps) => (
  <View style={manualStyles.statusReadout}>
    <Text style={[manualStyles.cardStatusText, compact && manualStyles.cardStatusTextCompact]} numberOfLines={1}>
      {label}
    </Text>
  </View>
);

export const MetricCell = ({label, value, compact}: {label: string; value: string} & CompactProps) => (
  <View style={[manualStyles.metricCell, compact && manualStyles.metricCellCompact]}>
    <Text style={manualStyles.metricLabel}>{label}</Text>
    <Text style={manualStyles.metricValue} numberOfLines={1}>
      {value}
    </Text>
  </View>
);

export const manualStyles = StyleSheet.create({
  grid: {
    flexGrow: 1,
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: 12,
    padding: 16,
  },
  gridCompact: {
    gap: 8,
    padding: 8,
  },
  homeCard: {
    flexGrow: 1,
    flexBasis: '32%',
    minWidth: 210,
    minHeight: 156,
    justifyContent: 'space-between',
    gap: 16,
    borderWidth: 1,
    borderColor: '#334D60',
    borderRadius: 4,
    backgroundColor: 'rgba(17, 31, 40, 0.94)',
    padding: 18,
  },
  homeCardCompact: {
    minWidth: 0,
    minHeight: 94,
    gap: 6,
    padding: 8,
  },
  axisCard: {
    flexGrow: 1,
    flexBasis: '32%',
    minWidth: 230,
    minHeight: 184,
    gap: 14,
    borderWidth: 1,
    borderColor: '#334D60',
    borderRadius: 4,
    backgroundColor: 'rgba(17, 31, 40, 0.94)',
    padding: 18,
  },
  axisCardCompact: {
    minWidth: 0,
    minHeight: 136,
    gap: 8,
    padding: 10,
  },
  cylinderCard: {
    flexGrow: 1,
    flexBasis: '32%',
    minWidth: 230,
    minHeight: 150,
    justifyContent: 'space-between',
    gap: 14,
    borderWidth: 1,
    borderColor: '#334D60',
    borderRadius: 4,
    backgroundColor: 'rgba(17, 31, 40, 0.94)',
    padding: 18,
  },
  cylinderCardCompact: {
    minWidth: 0,
    minHeight: 112,
    gap: 8,
    padding: 10,
  },
  cardHeader: {
    minHeight: 50,
    flexDirection: 'row',
    alignItems: 'center',
    gap: 16,
  },
  cardHeaderCompact: {
    minHeight: 34,
    gap: 8,
  },
  titleBlock: {
    flex: 1,
    minWidth: 0,
    gap: 2,
  },
  titleInlineRow: {
    minWidth: 0,
    flexDirection: 'row',
    alignItems: 'baseline',
    gap: 8,
  },
  inlineTitle: {
    flexShrink: 1,
    minWidth: 0,
  },
  cardTitle: {
    color: colors.text,
    fontFamily: typography.fontFamily,
    fontSize: 27,
    fontWeight: typography.weights.semibold,
    letterSpacing: 0,
  },
  cardTitleCompact: {
    fontSize: 17,
  },
  statusReadout: {
    flexShrink: 0,
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
  },
  cardStatusText: {
    color: colors.textSecondary,
    fontFamily: typography.fontFamily,
    fontSize: 20,
    fontWeight: typography.weights.medium,
    letterSpacing: 0,
  },
  inlineSubtitle: {
    flexShrink: 0,
    color: colors.textSecondary,
    fontFamily: typography.fontFamily,
    fontSize: 13,
    fontWeight: typography.weights.medium,
    letterSpacing: 0,
  },
  inlineSubtitleCompact: {
    fontSize: 11,
  },
  cardStatusTextCompact: {
    fontSize: 14,
  },
  positionRow: {
    flexDirection: 'row',
    alignItems: 'flex-end',
    gap: 6,
  },
  positionValue: {
    color: colors.primaryStrong,
    fontSize: 26,
    fontWeight: typography.weights.semibold,
    fontFamily: typography.fontFamily,
    letterSpacing: 0,
  },
  positionValueCompact: {
    fontSize: 18,
  },
  unit: {
    color: colors.textSecondary,
    fontFamily: typography.fontFamily,
    fontSize: 12,
    paddingBottom: 4,
  },
  metricRow: {
    flexDirection: 'row',
    gap: 8,
  },
  metricCell: {
    flex: 1,
    minHeight: 42,
    justifyContent: 'center',
    borderWidth: 1,
    borderColor: '#263B4A',
    backgroundColor: 'rgba(4, 12, 18, 0.58)',
    paddingHorizontal: 10,
  },
  metricCellCompact: {
    minHeight: 34,
    paddingHorizontal: 8,
  },
  metricLabel: {
    color: colors.textMuted,
    fontFamily: typography.fontFamily,
    fontSize: 10,
    fontWeight: typography.weights.semibold,
    letterSpacing: 0,
  },
  metricValue: {
    color: colors.text,
    fontFamily: typography.fontFamily,
    fontSize: 14,
    fontWeight: typography.weights.semibold,
    letterSpacing: 0,
  },
  jogRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: spacing.sm,
  },
  twoButtonRow: {
    flexDirection: 'row',
    gap: spacing.sm,
  },
});
