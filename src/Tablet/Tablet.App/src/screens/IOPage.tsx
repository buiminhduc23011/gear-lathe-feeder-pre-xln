import React, {useCallback, useMemo, useState} from 'react';
import {Pressable, ScrollView, StyleSheet, Text, useWindowDimensions, View} from 'react-native';
import {ioChannels, type IoChannelKey} from '../config/io';
import {plcTagByName} from '../config/plcTags';
import {colors, radius, spacing, typography} from '../styles/theme';
import type {PlcSnapshot} from '../types/plc';

interface Props {
  snapshot: PlcSnapshot;
}

interface IoCell {
  address: string;
  subtitle: string;
  active: boolean;
  mapped: boolean;
}

interface IoCellDescriptor {
  address: string;
  subtitle: string;
  mapped: boolean;
  tagName?: string;
}

const boolValue = (value: unknown): boolean => value === true || value === 1 || value === '1' || value === 'true';

const resolveIoChannel = (channel: IoChannelKey) => ioChannels.find(item => item.key === channel) ?? ioChannels[0];

const buildIoCellDescriptors = (channel: IoChannelKey): IoCellDescriptor[] => {
  const config = resolveIoChannel(channel);
  return config.points.map(point => {
    const tag = plcTagByName.get(point.tagName);
    return {
      address: point.displayAddress,
      subtitle: tag?.description ?? 'UNMAPPED',
      mapped: Boolean(tag),
      tagName: tag?.name,
    };
  });
};

const ioCellDescriptorsByChannel: Record<IoChannelKey, IoCellDescriptor[]> = {
  input: buildIoCellDescriptors('input'),
  output: buildIoCellDescriptors('output'),
};

export const buildIoCells = (channel: IoChannelKey, snapshot: PlcSnapshot): IoCell[] => {
  return ioCellDescriptorsByChannel[channel].map(cell => ({
    address: cell.address,
    subtitle: cell.subtitle,
    active: cell.tagName ? boolValue(snapshot[cell.tagName]) : false,
    mapped: cell.mapped,
  }));
};

interface IoCellTileProps extends IoCell {
  compact: boolean;
}

const IoCellTile = React.memo(({active, address, compact, mapped, subtitle}: IoCellTileProps) => (
  <View
    accessibilityLabel={`${address} ${subtitle} ${active ? 'ON' : 'OFF'}`}
    style={[styles.cell, compact && styles.cellCompact, active && styles.cellActive, !mapped && styles.cellUnmapped]}
    testID="io-cell">
    <Text style={[styles.cellAddress, active && styles.cellAddressActive]}>{address}</Text>
  </View>
));

export const IOPage = React.memo(({snapshot}: Props) => {
  const [channel, setChannel] = useState<IoChannelKey>('input');
  const {height, width} = useWindowDimensions();
  const compact = width <= 900 || height <= 520;
  const inputCells = useMemo(() => buildIoCells('input', snapshot), [snapshot]);
  const outputCells = useMemo(() => buildIoCells('output', snapshot), [snapshot]);
  const cells = channel === 'input' ? inputCells : outputCells;
  const selectChannel = useCallback((nextChannel: IoChannelKey) => {
    setChannel(currentChannel => currentChannel === nextChannel ? currentChannel : nextChannel);
  }, []);

  return (
    <View style={styles.root}>
      <View style={[styles.tabs, compact && styles.tabsCompact]}>
        {ioChannels.map(item => {
          const active = item.key === channel;
          return (
            <Pressable
              key={item.key}
              accessibilityRole="tab"
              accessibilityState={{selected: active}}
              onPress={() => selectChannel(item.key)}
              testID={`io-tab-${item.key}`}
              style={({pressed}) => [
                styles.tab,
                compact && styles.tabCompact,
                active && styles.tabSelected,
                pressed && styles.tabPressed,
              ]}>
              <Text style={[styles.tabLabel, compact && styles.tabLabelCompact, active && styles.tabTextSelected]}>{item.label}</Text>
            </Pressable>
          );
        })}
        <View style={styles.tabRemainder} />
      </View>

      <View style={styles.board}>
        <ScrollView removeClippedSubviews style={styles.gridScroll} contentContainerStyle={styles.gridContent}>
          <View style={styles.grid} testID="io-grid">
            {cells.map((cell, index) => (
              <IoCellTile
                key={index}
                active={cell.active}
                address={cell.address}
                compact={compact}
                mapped={cell.mapped}
                subtitle={cell.subtitle}
              />
            ))}
          </View>
        </ScrollView>
      </View>
    </View>
  );
});

const styles = StyleSheet.create({
  root: {
    flex: 1,
    minHeight: 0,
    gap: spacing.lg,
  },
  tabs: {
    minHeight: 54,
    flexDirection: 'row',
    borderWidth: 1,
    borderColor: '#263B4A',
    backgroundColor: 'rgba(12, 24, 33, 0.95)',
  },
  tabsCompact: {
    minHeight: 44,
  },
  tab: {
    width: 166,
    minWidth: 0,
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
    borderBottomColor: colors.running,
    backgroundColor: 'rgba(23, 240, 129, 0.10)',
  },
  tabPressed: {
    backgroundColor: 'rgba(23, 240, 129, 0.16)',
  },
  tabLabel: {
    color: colors.textSecondary,
    fontFamily: typography.fontFamily,
    fontSize: 16,
    fontWeight: typography.weights.semibold,
    letterSpacing: 0,
    textTransform: 'uppercase',
  },
  tabLabelCompact: {
    fontSize: 14,
  },
  tabTextSelected: {
    color: colors.text,
  },
  tabRemainder: {
    flex: 1,
    minWidth: 0,
  },
  board: {
    flex: 1,
    minHeight: 0,
    borderWidth: 1,
    borderColor: colors.borderStrong,
    borderRadius: radius.panel,
    overflow: 'hidden',
    backgroundColor: colors.recessed,
  },
  gridScroll: {
    flex: 1,
  },
  gridContent: {
    flexGrow: 1,
  },
  grid: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    backgroundColor: colors.divider,
  },
  cell: {
    width: '12.5%',
    minHeight: 58,
    alignItems: 'flex-start',
    justifyContent: 'space-between',
    borderRightWidth: 1,
    borderBottomWidth: 1,
    borderColor: colors.divider,
    backgroundColor: 'rgba(12, 27, 45, 0.94)',
    paddingHorizontal: spacing.sm,
    paddingVertical: 9,
  },
  cellCompact: {
    minHeight: 48,
    paddingHorizontal: 7,
    paddingVertical: 6,
  },
  cellActive: {
    backgroundColor: '#087F6E',
  },
  cellUnmapped: {
    opacity: 0.62,
  },
  cellAddress: {
    minWidth: 0,
    color: colors.text,
    fontFamily: typography.fontFamily,
    fontSize: 12,
    fontWeight: typography.weights.bold,
    letterSpacing: 0,
  },
  cellAddressActive: {
    color: colors.text,
  },
  cellSubtitle: {
    minWidth: 0,
    color: colors.textMuted,
    fontFamily: typography.fontFamily,
    fontSize: 10,
    fontWeight: typography.weights.medium,
    letterSpacing: 0,
    lineHeight: 12,
  },
  cellSubtitleActive: {
    color: 'rgba(255, 255, 255, 0.82)',
  },
});
