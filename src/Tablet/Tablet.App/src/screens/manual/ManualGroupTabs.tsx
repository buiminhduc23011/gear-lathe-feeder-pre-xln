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
          <Text
            style={[styles.tabText, compact && styles.tabTextCompact, selected && styles.tabTextSelected]}
            numberOfLines={1}>
            {group.label}
          </Text>
        </Pressable>
      );
    })}
    <View style={styles.tabRemainder} />
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
    borderRightWidth: 1,
    borderRightColor: '#263B4A',
    borderBottomWidth: 3,
    borderBottomColor: 'transparent',
    backgroundColor: 'rgba(255, 255, 255, 0.018)',
    paddingHorizontal: 14,
  },
  tabCompact: {
    width: 136,
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
});
