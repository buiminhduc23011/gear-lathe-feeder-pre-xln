import React from 'react';
import {StyleSheet, Text, View} from 'react-native';
import {colors, radius} from '../styles/theme';

type BadgeTone = 'ok' | 'fault' | 'warning' | 'info' | 'idle';

interface Props {
  label: string;
  tone?: BadgeTone;
}

const toneColors: Record<BadgeTone, {background: string; border: string; text: string}> = {
  ok: {background: colors.runningSurface, border: colors.running, text: colors.running},
  fault: {background: colors.faultSurface, border: colors.fault, text: colors.fault},
  warning: {background: colors.warningSurface, border: colors.warning, text: colors.warning},
  info: {background: colors.primarySurface, border: colors.primary, text: colors.primary},
  idle: {background: colors.idleSurface, border: colors.idle, text: colors.idle},
};

export const StatusBadge = ({label, tone = 'idle'}: Props) => {
  const palette = toneColors[tone];
  return (
    <View style={[styles.badge, {backgroundColor: palette.background, borderColor: palette.border}]}>
      <View style={[styles.indicatorDot, {backgroundColor: palette.text}]} />
      <Text style={[styles.text, {color: palette.text}]} numberOfLines={1}>
        {label}
      </Text>
    </View>
  );
};

const styles = StyleSheet.create({
  badge: {
    minHeight: 30,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    borderWidth: 1,
    borderRadius: radius.badge,
    paddingHorizontal: 10,
    paddingVertical: 4,
    gap: 6,
  },
  indicatorDot: {
    width: 6,
    height: 6,
    borderRadius: 3,
  },
  text: {
    fontSize: 12,
    fontWeight: '600',
    textTransform: 'uppercase',
    letterSpacing: 0,
  },
});
