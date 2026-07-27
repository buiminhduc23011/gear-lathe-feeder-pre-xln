import React from 'react';
import {StyleSheet, Text, useWindowDimensions, View} from 'react-native';
import {colors, spacing, typography} from '../styles/theme';

export enum AlarmBarState {
  Normal = 'normal',
  Warning = 'warning',
  Error = 'error',
}

export interface AlarmBarProps {
  text: string;
  state: AlarmBarState;
}

const statePalette: Record<AlarmBarState, {background: string; border: string; label: string}> = {
  [AlarmBarState.Normal]: {
    background: colors.runningSurface,
    border: colors.running,
    label: 'STATUS',
  },
  [AlarmBarState.Warning]: {
    background: colors.warningSurface,
    border: colors.warning,
    label: 'WARNING',
  },
  [AlarmBarState.Error]: {
    background: colors.faultSurface,
    border: colors.fault,
    label: 'ALARM',
  },
};

export const AlarmBar = React.memo(({text, state}: AlarmBarProps) => {
  const {height, width} = useWindowDimensions();
  const palette = statePalette[state];
  const segments = text.split('|').map(segment => segment.trim()).filter(Boolean);
  const isNormalStatus = state === AlarmBarState.Normal && segments.length >= 3;
  const isCompact = width <= 900 || height <= 520;

  return (
    <View
      accessibilityLabel={`${palette.label}: ${text}`}
      style={[styles.shell, isCompact && styles.shellCompact, {backgroundColor: palette.background, borderColor: palette.border}]}
      testID="alarm-bar">
      <View style={[styles.content, isCompact && styles.contentCompact]} testID="alarm-content">
        {isNormalStatus ? (
          <View style={styles.segmentRow}>
            {segments.slice(0, 3).map((segment, index) => (
              <React.Fragment key={`${segment}-${index}`}>
                <View style={[styles.segment, isCompact && styles.segmentCompact]}>
                  <Text
                    ellipsizeMode="tail"
                    numberOfLines={1}
                    style={[
                      styles.tickerText,
                      isCompact && styles.tickerTextCompact,
                      index === 1 && styles.primarySegment,
                      index === 1 && isCompact && styles.primarySegmentCompact,
                      styles.whiteText,
                    ]}>
                    {segment}
                  </Text>
                </View>
                {index < 2 ? <View style={[styles.separator, isCompact && styles.separatorCompact]} /> : null}
              </React.Fragment>
            ))}
          </View>
        ) : (
          <View style={styles.alertContent}>
            <Text style={[styles.labelText, styles.whiteText]}>{palette.label}</Text>
            <Text
              ellipsizeMode="tail"
              numberOfLines={1}
              style={[styles.tickerText, isCompact && styles.tickerTextCompact, styles.whiteText]}>
              {text}
            </Text>
          </View>
        )}
      </View>
    </View>
  );
});

const styles = StyleSheet.create({
  shell: {
    minWidth: 430,
    minHeight: 62,
    flex: 1,
    flexDirection: 'row',
    alignItems: 'center',
    overflow: 'hidden',
    borderWidth: 1.5,
    borderRadius: 4,
  },
  shellCompact: {
    minWidth: 320,
    minHeight: 44,
    borderRadius: 4,
  },
  labelText: {
    fontFamily: typography.fontFamily,
    fontSize: 13,
    fontWeight: typography.weights.semibold,
    letterSpacing: 0,
  },
  whiteText: {
    color: colors.text,
  },
  content: {
    flex: 1,
    minWidth: 0,
    minHeight: 59,
    flexDirection: 'row',
    alignItems: 'center',
    paddingHorizontal: spacing.lg,
  },
  contentCompact: {
    minHeight: 41,
    paddingHorizontal: spacing.md,
  },
  segmentRow: {
    flexDirection: 'row',
    alignItems: 'center',
  },
  segment: {
    minWidth: 0,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    gap: spacing.md,
    paddingHorizontal: spacing.sm,
  },
  segmentCompact: {
    gap: spacing.sm,
    paddingHorizontal: spacing.xs,
  },
  separator: {
    width: 1,
    height: 28,
    backgroundColor: 'rgba(32, 232, 244, 0.14)',
  },
  separatorCompact: {
    height: 20,
  },
  alertContent: {
    flex: 1,
    minWidth: 0,
    flexDirection: 'row',
    alignItems: 'center',
    gap: spacing.md,
  },
  tickerText: {
    fontFamily: typography.fontFamily,
    fontSize: 18,
    fontWeight: typography.weights.semibold,
    letterSpacing: 0,
  },
  tickerTextCompact: {
    fontSize: 13,
  },
  primarySegment: {
    fontSize: 19,
  },
  primarySegmentCompact: {
    fontSize: 13,
  },
});
