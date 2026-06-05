import React, {useEffect, useRef, useState} from 'react';
import {Animated, Easing, StyleSheet, Text, View} from 'react-native';
import {colors, radius, spacing} from '../styles/theme';

export type AlarmTone = 'ok' | 'fault' | 'warning' | 'info' | 'idle';

interface Props {
  text: string;
  tone?: AlarmTone;
}

const tonePalette: Record<AlarmTone, {background: string; border: string; text: string; label: string}> = {
  ok: {background: colors.runningSurface, border: colors.running, text: colors.running, label: 'STATUS'},
  fault: {background: colors.faultSurface, border: colors.fault, text: colors.fault, label: 'ALARM'},
  warning: {background: colors.warningSurface, border: colors.warning, text: colors.warning, label: 'STATUS'},
  info: {background: colors.primarySurface, border: colors.primary, text: colors.primaryStrong, label: 'STATUS'},
  idle: {background: colors.idleSurface, border: colors.idle, text: colors.textSecondary, label: 'STATUS'},
};

export const AlarmTicker = ({text, tone = 'idle'}: Props) => {
  const palette = tonePalette[tone];
  const translateX = useRef(new Animated.Value(0)).current;
  const [contentWidth, setContentWidth] = useState(0);

  useEffect(() => {
    if (contentWidth <= 0) {
      return undefined;
    }

    translateX.setValue(0);
    const animation = Animated.loop(
      Animated.timing(translateX, {
        toValue: -contentWidth,
        duration: Math.max(8000, contentWidth * 34),
        easing: Easing.linear,
        useNativeDriver: true,
      }),
      {resetBeforeIteration: true},
    );

    animation.start();
    return () => animation.stop();
  }, [contentWidth, text, translateX]);

  return (
    <View
      accessibilityLabel={`${palette.label}: ${text}`}
      style={[styles.shell, {backgroundColor: palette.background, borderColor: palette.border}]}>
      <View style={[styles.fixedLabel, palette.label === 'STATUS' && styles.fixedLabelStatus]}>
        <View style={[styles.dot, {backgroundColor: palette.text}]} />
        {palette.label !== 'STATUS' ? (
          <Text style={[styles.labelText, {color: palette.text}]}>{palette.label}</Text>
        ) : null}
      </View>
      <View style={styles.viewport}>
        <Animated.View
          style={[styles.track, {transform: [{translateX}]}]}>
          <View onLayout={event => setContentWidth(event.nativeEvent.layout.width)} style={styles.messageBlock}>
            <Text style={[styles.tickerText, {color: palette.text}]} numberOfLines={1}>
              {text}
            </Text>
          </View>
          <View style={styles.messageBlock} aria-hidden>
            <Text style={[styles.tickerText, {color: palette.text}]} numberOfLines={1}>
              {text}
            </Text>
          </View>
        </Animated.View>
      </View>
    </View>
  );
};

const styles = StyleSheet.create({
  shell: {
    minWidth: 260,
    minHeight: 40,
    flex: 1,
    flexDirection: 'row',
    alignItems: 'center',
    overflow: 'hidden',
    borderWidth: 1,
    borderRadius: radius.button,
  },
  fixedLabel: {
    height: '100%',
    minWidth: 84,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    borderRightWidth: 1,
    borderRightColor: colors.divider,
    gap: spacing.xs,
    paddingHorizontal: spacing.sm,
  },
  fixedLabelStatus: {
    minWidth: 28,
  },
  dot: {
    width: 7,
    height: 7,
    borderRadius: 4,
  },
  labelText: {
    fontSize: 11,
    fontWeight: '600',
    letterSpacing: 0,
  },
  viewport: {
    flex: 1,
    overflow: 'hidden',
  },
  track: {
    flexDirection: 'row',
    alignSelf: 'flex-start',
  },
  messageBlock: {
    paddingLeft: spacing.md,
    paddingRight: spacing.xl,
  },
  tickerText: {
    fontFamily: 'monospace',
    fontSize: 14,
    fontWeight: '600',
    letterSpacing: 0,
  },
});
