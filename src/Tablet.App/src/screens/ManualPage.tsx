import React, {useEffect, useMemo, useState} from 'react';
import {Pressable, ScrollView, StyleSheet, Text, TextInput, View} from 'react-native';
import {tabletAppConfig} from '../config/manualConfig';
import {HoldToRunButton} from '../components/HoldToRunButton';
import {StatusBadge} from '../components/StatusBadge';
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
import {colors, radius, spacing} from '../styles/theme';

type ManualGroup = 'origin' | 'axis' | 'cylinder';

interface Props {
  runtime: ManualRuntime;
}

export const ManualPage = ({runtime}: Props) => {
  const [selectedGroup, setSelectedGroup] = useState<ManualGroup>('origin');
  const config = tabletAppConfig.manualScreen;
  const axes = useMemo(() => getAxisStates(config, runtime.snapshot), [config, runtime.snapshot]);
  const cylinders = useMemo(() => getCylinderStates(config, runtime.snapshot), [config, runtime.snapshot]);
  const origins = useMemo(() => getOriginActionStates(config, runtime.snapshot), [config, runtime.snapshot]);
  const isConnected = runtime.isConnected;

  return (
    <View style={styles.root}>
      <View style={styles.content}>
        <ManualGroupRail selectedGroup={selectedGroup} onSelect={setSelectedGroup} />
        <View style={styles.panel}>
          {selectedGroup === 'origin' ? (
            <OriginGrid actions={origins} isConnected={isConnected} controller={runtime.controller} />
          ) : null}
          {selectedGroup === 'axis' ? (
            <AxisGrid axes={axes} isConnected={isConnected} controller={runtime.controller} />
          ) : null}
          {selectedGroup === 'cylinder' ? (
            <CylinderGrid cylinders={cylinders} isConnected={isConnected} controller={runtime.controller} />
          ) : null}
        </View>
      </View>
    </View>
  );
};

const ManualGroupRail = ({selectedGroup, onSelect}: {selectedGroup: ManualGroup; onSelect(group: ManualGroup): void}) => {
  const groups: Array<{key: ManualGroup; label: string}> = [
    {key: 'origin', label: 'Origin'},
    {key: 'axis', label: '3 Trục'},
    {key: 'cylinder', label: 'Xy Lanh'},
  ];

  return (
    <View style={styles.rail}>
      {groups.map(group => {
        const selected = group.key === selectedGroup;
        return (
          <Pressable key={group.key} onPress={() => onSelect(group.key)} style={[styles.railButton, selected && styles.railButtonSelected]}>
            <Text style={[styles.railButtonText, selected && styles.railButtonTextSelected]}>{group.label}</Text>
          </Pressable>
        );
      })}
    </View>
  );
};

const OriginGrid = ({
  actions,
  isConnected,
  controller,
}: {
  actions: ManualOriginActionState[];
  isConnected: boolean;
  controller: any;
}) => (
  <ScrollView contentContainerStyle={styles.originGrid}>
    {actions.map(action => (
      <View key={action.config.commandTag} testID="manual-origin-card" style={styles.originCard}>
        <View style={styles.cardHeader}>
          <Text style={styles.cardTitle}>{action.config.title}</Text>
          <View style={styles.cardStatus}>
            <View style={[styles.statusIndicatorDot, {backgroundColor: action.isActive ? colors.warning : action.isDone ? colors.running : colors.idle}]} />
            <Text style={styles.cardStatusText}>{action.isActive ? 'Homing' : action.isDone ? 'Home' : 'Lệch'}</Text>
          </View>
        </View>
        <HoldToRunButton
          label="Chạy Home"
          disabled={!isConnected}
          active={action.isActive || action.isCommandActive}
          onStart={() => controller.pressCommand(action.config.commandTag)}
          onStop={() => controller.releaseCommand(action.config.commandTag)}
        />
      </View>
    ))}
  </ScrollView>
);

const AxisGrid = ({axes, isConnected, controller}: {axes: ManualAxisState[]; isConnected: boolean; controller: any}) => (
  <ScrollView contentContainerStyle={styles.axisGrid}>
    {axes.map(axis => (
      <AxisCard key={axis.config.key} axis={axis} isConnected={isConnected} controller={controller} />
    ))}
  </ScrollView>
);

const AxisCard = ({axis, isConnected, controller}: {axis: ManualAxisState; isConnected: boolean; controller: any}) => {
  return (
    <View testID="manual-axis-card" style={styles.axisCard}>
      <View style={styles.cardHeader}>
        <View>
          <Text style={styles.cardTitle}>{axis.config.displayName}</Text>
          <View style={styles.positionRow}>
            <Text style={styles.positionValue}>{formatManualNumber(axis.currentPosition)}</Text>
            <Text style={styles.unit}>mm</Text>
          </View>
        </View>
        <View style={styles.cardStatus}>
          <View style={[styles.statusIndicatorDot, {backgroundColor: axis.isHomed ? colors.running : axis.isHoming ? colors.warning : colors.idle}]} />
          <Text style={styles.cardStatusText}>{axis.isHomed ? 'Home' : axis.isHoming ? 'Homing' : 'Chưa'}</Text>
        </View>
      </View>

      <View style={styles.jogRow}>
        <HoldToRunButton
          label={axis.config.negativeLabel}
          disabled={!isConnected}
          active={axis.isNegativeJogActive}
          onStart={() => controller.pressCommand(axis.config.negativeJogTag)}
          onStop={() => controller.releaseCommand(axis.config.negativeJogTag)}
        />
        <HoldToRunButton
          label="Home"
          disabled={!isConnected}
          active={axis.isHomeCommandActive || axis.isHoming}
          onStart={() => controller.pressCommand(axis.config.homeTag)}
          onStop={() => controller.releaseCommand(axis.config.homeTag)}
        />
        <HoldToRunButton
          label={axis.config.positiveLabel}
          disabled={!isConnected}
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
}: {
  cylinders: ManualCylinderState[];
  isConnected: boolean;
  controller: any;
}) => (
  <ScrollView contentContainerStyle={styles.cylinderGrid}>
    {cylinders.map(cylinder => (
      <View key={cylinder.config.key} testID="manual-cylinder-card" style={styles.cylinderCard}>
        <View style={styles.cardHeader}>
          <Text style={styles.cardTitle}>{cylinder.config.title}</Text>
          <View style={styles.dotRowCompact}>
            <View style={[styles.statusIndicatorDot, {backgroundColor: cylinder.isPrimaryFeedbackActive ? colors.running : colors.idle}]} />
            <View style={[styles.statusIndicatorDot, {backgroundColor: cylinder.isSecondaryFeedbackActive ? colors.running : colors.idle}]} />
          </View>
        </View>
        <View style={styles.twoButtonRow}>
          <HoldToRunButton
            label={cylinder.config.primaryActionLabel}
            disabled={!isConnected}
            active={cylinder.isPrimaryCommandActive}
            onStart={() => controller.pressCommand(cylinder.config.primaryCommandTag)}
            onStop={() => controller.releaseCommand(cylinder.config.primaryCommandTag)}
          />
          <HoldToRunButton
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

const StatusDot = ({label, active, warning, fault}: {label: string; active: boolean; warning?: boolean; fault?: boolean}) => (
  <View style={styles.statusDotGroup}>
    <View style={[styles.statusDot, active && {backgroundColor: fault ? colors.fault : warning ? colors.warning : colors.running}]} />
    <Text style={styles.statusDotLabel}>{label}</Text>
  </View>
);

const LabeledInput = ({
  label,
  value,
  error,
  onFocus,
  onChangeText,
  onSubmit,
}: {
  label: string;
  value: string;
  error: string;
  onFocus(): void;
  onChangeText(value: string): void;
  onSubmit(): void;
}) => (
  <View style={styles.inputBlock}>
    <View style={styles.inputRow}>
      <Text style={styles.inputLabel}>{label}</Text>
      <TextInput
        keyboardType="numeric"
        inputMode="decimal"
        value={value}
        onFocus={onFocus}
        onChangeText={onChangeText}
        onBlur={onSubmit}
        onSubmitEditing={onSubmit}
        style={styles.input}
      />
    </View>
    {error ? <Text style={styles.inputError}>{error}</Text> : null}
  </View>
);

const CommandButton = ({label, disabled, primary, onPress}: {label: string; disabled?: boolean; primary?: boolean; onPress(): Promise<void> | void}) => (
  <Pressable
    accessibilityRole="button"
    disabled={disabled}
    onPress={() => void onPress()}
    style={({pressed}) => [
      styles.commandButton,
      primary && styles.commandPrimary,
      disabled && styles.commandDisabled,
      pressed && !disabled && styles.commandPressed,
    ]}>
    <Text style={[styles.commandText, primary && styles.commandPrimaryText, disabled && styles.commandDisabledText]} numberOfLines={1}>
      {label}
    </Text>
  </Pressable>
);

const styles = StyleSheet.create({
  root: {
    flex: 1,
    gap: spacing.md,
  },
  runtimeBanner: {
    minHeight: 38,
    flexDirection: 'row',
    alignItems: 'center',
    borderWidth: 1.5,
    borderRadius: radius.button,
    paddingHorizontal: spacing.md,
    gap: spacing.md,
  },
  runtimeBannerFault: {
    borderColor: colors.fault,
    backgroundColor: colors.faultSurface,
  },
  runtimeBannerRun: {
    borderColor: colors.running,
    backgroundColor: colors.runningSurface,
  },
  runtimeBannerLabel: {
    color: colors.text,
    fontSize: 12,
    fontWeight: '600',
    letterSpacing: 0,
  },
  runtimeBannerText: {
    flex: 1,
    color: colors.text,
    fontFamily: 'monospace',
    fontSize: 14,
    fontWeight: '600',
    letterSpacing: 0,
  },
  content: {
    flex: 1,
    minHeight: 0,
    flexDirection: 'row',
    gap: spacing.lg,
  },
  rail: {
    width: 120,
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: radius.panel,
    backgroundColor: colors.panel,
    padding: spacing.md,
    gap: spacing.sm,
  },
  railButton: {
    minHeight: 52,
    justifyContent: 'center',
    alignItems: 'center',
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: radius.button,
    backgroundColor: colors.recessed,
    paddingHorizontal: spacing.sm,
  },
  railButtonSelected: {
    backgroundColor: colors.primarySurface,
    borderColor: colors.primary,
    borderLeftWidth: 5,
  },
  railButtonText: {
    color: colors.text,
    fontSize: 16,
    fontWeight: '600',
    letterSpacing: 0,
  },
  railButtonTextSelected: {
    color: colors.primaryStrong,
  },
  panel: {
    flex: 1,
    minWidth: 0,
  },
  originGrid: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: spacing.md,
    paddingBottom: spacing.md,
  },
  originCard: {
    width: '31.8%',
    minWidth: 180,
    minHeight: 96,
    justifyContent: 'space-between',
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: radius.card,
    backgroundColor: colors.panel,
    padding: spacing.md,
    gap: spacing.sm,
  },
  cardHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    gap: spacing.xs,
  },
  cardTitleGroup: {
    flex: 1,
  },
  cardTitle: {
    color: colors.text,
    fontSize: 16,
    fontWeight: '600',
    letterSpacing: 0,
  },
  cardStatus: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 4,
  },
  statusIndicatorDot: {
    width: 8,
    height: 8,
    borderRadius: 4,
  },
  cardStatusText: {
    color: colors.textSecondary,
    fontSize: 11,
    fontWeight: '600',
  },
  dotRowCompact: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 4,
  },
  axisGrid: {
    flexDirection: 'row',
    gap: spacing.md,
    paddingBottom: spacing.md,
  },
  axisCard: {
    flex: 1,
    minWidth: 180,
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: radius.card,
    backgroundColor: colors.panel,
    padding: spacing.md,
    gap: spacing.sm,
  },
  positionRow: {
    marginTop: 4,
    flexDirection: 'row',
    alignItems: 'flex-end',
    gap: spacing.xs,
  },
  positionValue: {
    color: colors.primaryStrong,
    fontSize: 32,
    fontWeight: '600',
    fontFamily: 'monospace',
    letterSpacing: 0,
  },
  unit: {
    color: colors.textSecondary,
    fontSize: 13,
    paddingBottom: 4,
  },
  dotRow: {
    minHeight: 32,
    flexDirection: 'row',
    alignItems: 'center',
    flexWrap: 'wrap',
    gap: spacing.sm,
  },
  statusDotGroup: {
    flexDirection: 'row',
    alignItems: 'center',
    minHeight: 28,
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: radius.badge,
    backgroundColor: colors.recessed,
    paddingHorizontal: spacing.sm,
    gap: 6,
  },
  statusDot: {
    width: 9,
    height: 9,
    borderRadius: 5,
    backgroundColor: colors.idle,
  },
  statusDotLabel: {
    color: colors.textSecondary,
    fontSize: 12,
    fontWeight: '600',
    letterSpacing: 0,
  },
  limitStrip: {
    flexDirection: 'row',
    gap: spacing.sm,
  },
  metricPill: {
    flex: 1,
    minHeight: 58,
    justifyContent: 'center',
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: radius.button,
    backgroundColor: colors.recessed,
    paddingHorizontal: spacing.sm,
    gap: 2,
  },
  metricLabel: {
    color: colors.textMuted,
    fontSize: 10,
    fontWeight: '600',
    letterSpacing: 0,
  },
  metricValue: {
    color: colors.text,
    fontFamily: 'monospace',
    fontSize: 16,
    fontWeight: '600',
    letterSpacing: 0,
  },
  metricUnit: {
    color: colors.textSecondary,
    fontSize: 10,
    fontWeight: '600',
    letterSpacing: 0,
  },
  inputBlock: {
    gap: spacing.xs,
  },
  inputRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: spacing.md,
  },
  inputLabel: {
    width: 118,
    color: colors.textSecondary,
    fontSize: 13,
    fontWeight: '600',
    letterSpacing: 0,
  },
  input: {
    flex: 1,
    minHeight: 48,
    borderWidth: 1.5,
    borderColor: colors.borderStrong,
    borderRadius: radius.button,
    backgroundColor: colors.recessed,
    color: colors.text,
    fontSize: 17,
    fontWeight: '600',
    fontFamily: 'monospace',
    letterSpacing: 0,
    paddingHorizontal: spacing.md,
    paddingVertical: 0,
  },
  inputError: {
    marginLeft: 130,
    color: colors.fault,
    fontSize: 12,
    fontWeight: '600',
  },
  jogRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: spacing.sm,
  },
  cylinderGrid: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: spacing.md,
    paddingBottom: spacing.md,
  },
  cylinderCard: {
    width: '31.8%',
    minWidth: 180,
    minHeight: 104,
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: radius.card,
    backgroundColor: colors.panel,
    padding: spacing.md,
    gap: spacing.sm,
  },
  twoButtonRow: {
    flexDirection: 'row',
    gap: spacing.sm,
  },
  commandButton: {
    minHeight: 56,
    minWidth: 96,
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    borderRadius: radius.button,
    borderWidth: 1.5,
    borderColor: colors.borderStrong,
    backgroundColor: colors.raised,
    paddingHorizontal: spacing.md,
  },
  commandPrimary: {
    backgroundColor: colors.primary,
    borderColor: colors.primary,
  },
  commandDisabled: {
    opacity: 0.45,
  },
  commandPressed: {
    backgroundColor: colors.primarySurface,
    borderColor: colors.primaryStrong,
  },
  commandText: {
    color: colors.text,
    fontSize: 14,
    fontWeight: '600',
    textTransform: 'uppercase',
    letterSpacing: 0,
  },
  commandPrimaryText: {
    color: colors.inverse,
  },
  commandDisabledText: {
    color: colors.textDisabled,
  },
});
