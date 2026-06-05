import React from 'react';
import {Pressable, StyleSheet, Text, View} from 'react-native';
import {colors, typography} from '../../styles/theme';
import type {CompactProps, ManualGroup} from './types';

const groups: Array<{key: ManualGroup; label: string}> = [
  {key: 'origin', label: 'Origin'},
  {key: 'axis', label: '3 Trục'},
  {key: 'cylinder', label: 'Xy Lanh'},
];

interface Props extends CompactProps {
  selectedGroup: ManualGroup;
  onSelect(group: ManualGroup): void;
}

export const ManualGroupTabs = ({selectedGroup, onSelect, compact}: Props) => (
  <View style={[styles.tabs, compact && styles.tabsCompact]}>
    {groups.map(group => {
      const selected = group.key === selectedGroup;
      return (
        <Pressable
          accessibilityRole="tab"
          accessibilityState={{selected}}
          key={group.key}
          onPress={() => onSelect(group.key)}
          style={({pressed}) => [
            styles.tab,
            compact && styles.tabCompact,
            selected && styles.tabSelected,
            pressed && styles.tabPressed,
          ]}>
          <GroupGlyph group={group.key} color={selected ? colors.primaryStrong : colors.textSecondary} size={compact ? 24 : 30} />
          <Text style={[styles.tabText, compact && styles.tabTextCompact, selected && styles.tabTextSelected]} numberOfLines={1}>
            {group.label}
          </Text>
        </Pressable>
      );
    })}
    <View style={styles.tabRemainder} />
  </View>
);

const GroupGlyph = ({group, color, size}: {group: ManualGroup; color: string; size: number}) => {
  if (group === 'origin') {
    return <OriginCrosshair color={color} size={size} />;
  }

  if (group === 'axis') {
    return <AxisTriad color={color} size={size} />;
  }

  return <CylinderGlyph color={color} size={size} />;
};

const OriginCrosshair = ({color, size}: {color: string; size: number}) => (
  <View style={{width: size, height: size}}>
    <View style={[styles.crosshairRing, {borderColor: color, width: size * 0.68, height: size * 0.68, borderRadius: size * 0.34, left: size * 0.16, top: size * 0.16}]} />
    <View style={[styles.crosshairLine, {backgroundColor: color, width: size, height: 2, top: size * 0.5 - 1}]} />
    <View style={[styles.crosshairLine, {backgroundColor: color, width: 2, height: size, left: size * 0.5 - 1}]} />
    <View style={[styles.crosshairDot, {backgroundColor: color, width: size * 0.13, height: size * 0.13, borderRadius: size * 0.065, left: size * 0.435, top: size * 0.435}]} />
  </View>
);

const AxisTriad = ({color, size}: {color: string; size: number}) => {
  const stroke = Math.max(2, size * 0.085);
  const dot = Math.max(3, size * 0.13);
  const hub = Math.max(3.2, size * 0.14);
  const centerX = size * 0.5;
  const centerY = size * 0.54;
  const armLength = size * 0.42;

  return (
    <View style={{width: size, height: size}}>
      <View
        style={[
          styles.axisTriadSegment,
          {
            backgroundColor: color,
            width: stroke,
            height: size * 0.42,
            left: centerX - stroke / 2,
            top: size * 0.14,
          },
        ]}
      />
      <View
        style={[
          styles.axisTriadSegment,
          {
            backgroundColor: color,
            width: armLength,
            height: stroke,
            left: centerX - armLength * 0.94,
            top: centerY + size * 0.18,
            transform: [{rotate: '-35deg'}],
          },
        ]}
      />
      <View
        style={[
          styles.axisTriadSegment,
          {
            backgroundColor: color,
            width: armLength,
            height: stroke,
            left: centerX - armLength * 0.06,
            top: centerY + size * 0.18,
            transform: [{rotate: '35deg'}],
          },
        ]}
      />
      <View style={[styles.axisTriadDot, {backgroundColor: color, width: hub, height: hub, borderRadius: hub / 2, left: centerX - hub / 2, top: centerY - hub / 2}]} />
      <View style={[styles.axisTriadDot, {backgroundColor: color, width: dot, height: dot, borderRadius: dot / 2, left: centerX - dot / 2, top: size * 0.07}]} />
      <View style={[styles.axisTriadDot, {backgroundColor: color, width: dot, height: dot, borderRadius: dot / 2, left: size * 0.12, top: size * 0.79}]} />
      <View style={[styles.axisTriadDot, {backgroundColor: color, width: dot, height: dot, borderRadius: dot / 2, right: size * 0.12, top: size * 0.79}]} />
    </View>
  );
};

const CylinderGlyph = ({color, size}: {color: string; size: number}) => (
  <View style={{width: size, height: size, justifyContent: 'center', alignItems: 'center'}}>
    <View style={[styles.cylinderShell, {width: size * 0.72, height: size * 0.24, borderColor: color, borderRadius: size * 0.12, transform: [{rotate: '-45deg'}]}]}>
      <View style={[styles.cylinderCap, {borderLeftColor: color, left: size * 0.18}]} />
      <View style={[styles.cylinderCap, {borderLeftColor: color, right: size * 0.12}]} />
    </View>
  </View>
);

const styles = StyleSheet.create({
  tabs: {
    minHeight: 60,
    flexDirection: 'row',
    borderBottomWidth: 1,
    borderBottomColor: '#263B4A',
    backgroundColor: 'rgba(12, 24, 33, 0.95)',
  },
  tabsCompact: {
    minHeight: 46,
  },
  tab: {
    width: 166,
    minWidth: 0,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    gap: 14,
    borderRightWidth: 1,
    borderRightColor: '#263B4A',
    borderBottomWidth: 3,
    borderBottomColor: 'transparent',
    backgroundColor: 'rgba(255, 255, 255, 0.018)',
    paddingHorizontal: 14,
  },
  tabCompact: {
    width: 136,
    gap: 8,
    paddingHorizontal: 10,
  },
  tabSelected: {
    borderBottomColor: colors.primary,
    backgroundColor: 'rgba(18, 164, 255, 0.08)',
  },
  tabPressed: {
    backgroundColor: 'rgba(18, 164, 255, 0.14)',
  },
  tabText: {
    color: colors.textSecondary,
    fontFamily: typography.fontFamily,
    fontSize: 20,
    fontWeight: typography.weights.semibold,
    letterSpacing: 0,
    textShadowColor: 'rgba(255, 255, 255, 0.12)',
    textShadowOffset: {width: 0, height: 0},
    textShadowRadius: 3,
  },
  tabTextCompact: {
    fontSize: 16,
  },
  tabTextSelected: {
    color: colors.text,
  },
  tabRemainder: {
    flex: 1,
    minWidth: 0,
  },
  crosshairRing: {
    position: 'absolute',
    borderWidth: 2,
  },
  crosshairLine: {
    position: 'absolute',
  },
  crosshairDot: {
    position: 'absolute',
  },
  axisTriadSegment: {
    position: 'absolute',
    borderRadius: 2,
  },
  axisTriadDot: {
    position: 'absolute',
  },
  cylinderShell: {
    position: 'absolute',
    borderWidth: 2,
  },
  cylinderCap: {
    position: 'absolute',
    top: 3,
    bottom: 3,
    borderLeftWidth: 2,
  },
});
