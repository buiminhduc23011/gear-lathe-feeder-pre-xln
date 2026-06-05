import React, {useMemo, useState} from 'react';
import {Pressable, ScrollView, StyleSheet, Text, useWindowDimensions, View} from 'react-native';
import {HoldToRunButton} from '../components/HoldToRunButton';
import {tabletAppConfig} from '../config/manualConfig';
import type {ManualRuntime} from '../hooks/useManualRuntime';
import {
  formatManualNumber,
  getAxisStates,
  getCylinderStates,
  getOriginActionStates,
  type ManualAxisState,
  type ManualCylinderState,
  type ManualOriginActionState,
} from '../manual/manualSelectors';
import {colors, spacing, typography} from '../styles/theme';

type ManualGroup = 'origin' | 'axis' | 'cylinder';
type Tone = 'ok' | 'warning' | 'idle';

interface Props {
  runtime: ManualRuntime;
}

interface CompactProps {
  compact: boolean;
}

export const ManualPage = ({runtime}: Props) => {
  const [selectedGroup, setSelectedGroup] = useState<ManualGroup>('origin');
  const {height, width} = useWindowDimensions();
  const compact = width <= 900 || height <= 520;
  const config = tabletAppConfig.manualScreen;
  const axes = useMemo(() => getAxisStates(config, runtime.snapshot), [config, runtime.snapshot]);
  const cylinders = useMemo(() => getCylinderStates(config, runtime.snapshot), [config, runtime.snapshot]);
  const origins = useMemo(() => getOriginActionStates(config, runtime.snapshot), [config, runtime.snapshot]);
  const isConnected = runtime.isConnected;

  return (
    <View style={styles.root}>
      <ManualGroupTabs compact={compact} selectedGroup={selectedGroup} onSelect={setSelectedGroup} />
      <View style={styles.panel}>
        {selectedGroup === 'origin' ? (
          <OriginGrid actions={origins} compact={compact} isConnected={isConnected} controller={runtime.controller} />
        ) : null}
        {selectedGroup === 'axis' ? (
          <AxisGrid axes={axes} compact={compact} isConnected={isConnected} controller={runtime.controller} />
        ) : null}
        {selectedGroup === 'cylinder' ? (
          <CylinderGrid cylinders={cylinders} compact={compact} isConnected={isConnected} controller={runtime.controller} />
        ) : null}
      </View>
    </View>
  );
};

const ManualGroupTabs = ({
  selectedGroup,
  onSelect,
  compact,
}: {
  selectedGroup: ManualGroup;
  onSelect(group: ManualGroup): void;
} & CompactProps) => {
  const groups: Array<{key: ManualGroup; label: string}> = [
    {key: 'origin', label: 'Origin'},
    {key: 'axis', label: '3 Trục'},
    {key: 'cylinder', label: 'Xy Lanh'},
  ];

  return (
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
};

const OriginGrid = ({
  actions,
  isConnected,
  controller,
  compact,
}: {
  actions: ManualOriginActionState[];
  isConnected: boolean;
  controller: ManualRuntime['controller'];
} & CompactProps) => (
  <ScrollView showsVerticalScrollIndicator={false} contentContainerStyle={[styles.grid, compact && styles.gridCompact]}>
    {actions.map(action => (
      <View key={action.config.commandTag} testID="manual-origin-card" style={[styles.homeCard, compact && styles.homeCardCompact]}>
        <View style={[styles.cardHeader, compact && styles.cardHeaderCompact]}>
          <View style={styles.titleBlock}>
            <Text adjustsFontSizeToFit minimumFontScale={0.72} style={[styles.cardTitle, compact && styles.cardTitleCompact]} numberOfLines={1}>
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

const AxisGrid = ({
  axes,
  isConnected,
  controller,
  compact,
}: {
  axes: ManualAxisState[];
  isConnected: boolean;
  controller: ManualRuntime['controller'];
} & CompactProps) => (
  <ScrollView showsVerticalScrollIndicator={false} contentContainerStyle={[styles.grid, compact && styles.gridCompact]}>
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
  const axisLetter = axis.config.key.slice(-1).toUpperCase();
  const limitText = axis.isNegativeLimitActive ? 'LIMIT -' : axis.isPositiveLimitActive ? 'LIMIT +' : 'OK';
  const statusLabel = axis.isHomed ? 'Home' : axis.isHoming ? 'Homing' : 'Chưa';
  const tone: Tone = axis.isHomed ? 'ok' : axis.isHoming ? 'warning' : 'idle';

  return (
    <View testID="manual-axis-card" style={[styles.axisCard, compact && styles.axisCardCompact]}>
      <View style={[styles.cardHeader, compact && styles.cardHeaderCompact]}>
        <View style={[styles.symbolFrame, compact && styles.symbolFrameCompact]}>
          <LetterSymbol letter={axisLetter} color={colors.text} size={compact ? 28 : 38} />
        </View>
        <View style={styles.titleBlock}>
          <Text adjustsFontSizeToFit minimumFontScale={0.72} style={[styles.cardTitle, compact && styles.cardTitleCompact]} numberOfLines={1}>
            {axis.config.displayName}
          </Text>
          <View style={styles.positionRow}>
            <Text style={[styles.positionValue, compact && styles.positionValueCompact]} numberOfLines={1}>
              {formatManualNumber(axis.currentPosition)}
            </Text>
            <Text style={styles.unit}>mm</Text>
          </View>
        </View>
        <StatusReadout compact={compact} label={statusLabel} tone={tone} />
      </View>

      <View style={styles.metricRow}>
        <MetricCell label="SERVO" value={axis.isServoOn ? 'ON' : 'OFF'} compact={compact} />
        <MetricCell label="LIMIT" value={limitText} compact={compact} />
      </View>

      <View style={styles.jogRow}>
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

const CylinderGrid = ({
  cylinders,
  isConnected,
  controller,
  compact,
}: {
  cylinders: ManualCylinderState[];
  isConnected: boolean;
  controller: ManualRuntime['controller'];
} & CompactProps) => (
  <ScrollView showsVerticalScrollIndicator={false} contentContainerStyle={[styles.grid, compact && styles.gridCompact]}>
    {cylinders.map(cylinder => (
      <View key={cylinder.config.key} testID="manual-cylinder-card" style={[styles.cylinderCard, compact && styles.cylinderCardCompact]}>
        <View style={[styles.cardHeader, compact && styles.cardHeaderCompact]}>
          <View style={[styles.symbolFrame, compact && styles.symbolFrameCompact]}>
            <ClampSymbol color={colors.text} size={compact ? 28 : 38} />
          </View>
          <View style={styles.titleBlock}>
            <Text adjustsFontSizeToFit minimumFontScale={0.72} style={[styles.cardTitle, compact && styles.cardTitleCompact]} numberOfLines={1}>
              {cylinder.config.title}
            </Text>
            <View style={styles.feedbackRow}>
              <StatusDot active={cylinder.isPrimaryFeedbackActive} />
              <Text style={styles.feedbackText} numberOfLines={1}>
                {cylinder.config.primaryFeedbackLabel}
              </Text>
            </View>
          </View>
          <StatusDot active={cylinder.isSecondaryFeedbackActive} />
        </View>
        <View style={styles.twoButtonRow}>
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

const ClampSymbol = ({color, size}: {color: string; size: number}) => (
  <View style={{width: size, height: size}}>
    <View style={[styles.clampJaw, {borderColor: color, width: size * 0.44, height: size * 0.28, left: size * 0.28, top: size * 0.04}]} />
    <View style={[styles.clampJaw, {borderColor: color, width: size * 0.44, height: size * 0.28, left: size * 0.28, bottom: size * 0.04}]} />
    <View style={[styles.clampStem, {backgroundColor: color, width: 2.5, height: size * 0.5, left: size * 0.5 - 1.25, top: size * 0.25}]} />
  </View>
);

const LetterSymbol = ({letter, color, size}: {letter: string; color: string; size: number}) => (
  <Text adjustsFontSizeToFit style={[styles.letterSymbol, {color, fontSize: size * 0.76, lineHeight: size * 0.9}]} numberOfLines={1}>
    {letter}
  </Text>
);

const StatusReadout = ({label, tone, compact}: {label: string; tone: Tone; compact: boolean}) => (
  <View style={styles.statusReadout}>
    <View style={[styles.statusIndicatorDot, {backgroundColor: toneColor(tone)}]} />
    <Text style={[styles.cardStatusText, compact && styles.cardStatusTextCompact]} numberOfLines={1}>
      {label}
    </Text>
  </View>
);

const StatusDot = ({active}: {active: boolean}) => <View style={[styles.statusIndicatorDot, {backgroundColor: active ? colors.running : colors.idle}]} />;

const MetricCell = ({label, value, compact}: {label: string; value: string; compact: boolean}) => (
  <View style={[styles.metricCell, compact && styles.metricCellCompact]}>
    <Text style={styles.metricLabel}>{label}</Text>
    <Text style={styles.metricValue} numberOfLines={1}>
      {value}
    </Text>
  </View>
);

const toneColor = (tone: Tone): string => {
  if (tone === 'ok') {
    return '#5DD483';
  }
  if (tone === 'warning') {
    return colors.warning;
  }
  return colors.idle;
};

const styles = StyleSheet.create({
  root: {
    flex: 1,
    minHeight: 0,
    borderWidth: 1,
    borderColor: '#263B4A',
    backgroundColor: 'rgba(4, 12, 18, 0.92)',
  },
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
  panel: {
    flex: 1,
    minHeight: 0,
  },
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
    shadowColor: '#000',
    shadowOpacity: 0.28,
    shadowRadius: 8,
    shadowOffset: {width: 0, height: 4},
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
  symbolFrame: {
    width: 58,
    height: 52,
    alignItems: 'center',
    justifyContent: 'center',
    borderWidth: 1.5,
    borderColor: '#6E7E8A',
    backgroundColor: 'rgba(255, 255, 255, 0.035)',
  },
  symbolFrameCompact: {
    width: 38,
    height: 34,
  },
  titleBlock: {
    flex: 1,
    minWidth: 0,
    gap: 2,
  },
  cardTitle: {
    color: colors.text,
    fontFamily: typography.fontFamily,
    fontSize: 27,
    fontWeight: typography.weights.semibold,
    letterSpacing: 0,
    textShadowColor: 'rgba(255, 255, 255, 0.12)',
    textShadowOffset: {width: 0, height: 0},
    textShadowRadius: 3,
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
  statusIndicatorDot: {
    width: 13,
    height: 13,
    borderRadius: 7,
  },
  cardStatusText: {
    color: colors.textSecondary,
    fontFamily: typography.fontFamily,
    fontSize: 20,
    fontWeight: typography.weights.medium,
    letterSpacing: 0,
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
  feedbackRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 7,
  },
  feedbackText: {
    flexShrink: 1,
    color: colors.textSecondary,
    fontFamily: typography.fontFamily,
    fontSize: 12,
    fontWeight: typography.weights.medium,
    letterSpacing: 0,
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
  clampJaw: {
    position: 'absolute',
    borderWidth: 2,
    transform: [{rotate: '45deg'}],
  },
  clampStem: {
    position: 'absolute',
    borderRadius: 2,
  },
  letterSymbol: {
    color: colors.text,
    fontFamily: typography.fontFamily,
    fontWeight: typography.weights.bold,
    letterSpacing: 0,
    textAlign: 'center',
    includeFontPadding: false,
  },
});
