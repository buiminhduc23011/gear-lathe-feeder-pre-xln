import React from 'react';
import {Pressable, StatusBar, StyleSheet, Text, View} from 'react-native';
import {tabletAppConfig} from '../config/manualConfig';
import {colors, radius, spacing} from '../styles/theme';
import {AlarmTicker, type AlarmTone} from './AlarmTicker';
import {AppLogo} from './AppLogo';
import {Icon} from './Icon';

interface Props {
  isConnected: boolean;
  showHomeButton: boolean;
  screenEyebrow?: string;
  screenTitle?: string;
  alarmText?: string;
  alarmTone?: AlarmTone;
  onHome(): void;
  currentPage?: string;
  onNavigate?(page: any): void;
  children: React.ReactNode;
}

export const AppShell = ({
  isConnected,
  showHomeButton,
  screenEyebrow,
  screenTitle,
  alarmText,
  alarmTone,
  onHome,
  children,
}: Props) => {
  const headerEyebrow = screenEyebrow ?? 'GEAR LINE';
  const headerTitle = screenTitle ?? 'Bảng điều khiển';

  return (
    <View style={styles.safeArea}>
      <StatusBar hidden />
      <View style={styles.topBar}>
        <AppLogo style={styles.logo} />

        <View style={styles.cellIdentity}>
          <Text style={styles.eyebrow} numberOfLines={1}>
            {headerEyebrow}
          </Text>
          <Text style={styles.machineName} numberOfLines={1}>
            {headerTitle}
          </Text>
        </View>

        {alarmText ? <AlarmTicker text={alarmText} tone={alarmTone} /> : null}

        <Pressable
          accessibilityLabel="Home"
          accessibilityRole="button"
          onPress={onHome}
          style={({pressed}) => [
            styles.menuButton,
            !showHomeButton && styles.menuButtonCurrent,
            pressed && styles.menuButtonPressed,
          ]}>
          <Icon name="menu" color={showHomeButton ? colors.inverse : colors.primaryStrong} size={24} />
        </Pressable>
      </View>

      <View style={styles.body}>{children}</View>
    </View>
  );
};

const styles = StyleSheet.create({
  safeArea: {
    flex: 1,
    backgroundColor: colors.window,
  },
  topBar: {
    minHeight: 74,
    flexDirection: 'row',
    alignItems: 'center',
    borderBottomWidth: 1,
    borderBottomColor: colors.divider,
    backgroundColor: colors.shell,
    paddingHorizontal: spacing.md,
    gap: spacing.sm,
  },
  menuButton: {
    width: 52,
    height: 52,
    alignItems: 'center',
    justifyContent: 'center',
    borderWidth: 1.5,
    borderColor: colors.primary,
    borderRadius: radius.button,
    backgroundColor: colors.primary,
  },
  menuButtonCurrent: {
    borderColor: colors.borderStrong,
    backgroundColor: colors.raised,
  },
  menuButtonPressed: {
    opacity: 0.82,
  },
  logo: {
    width: 68,
    height: 46,
  },
  cellIdentity: {
    width: 370,
    minWidth: 118,
    flexShrink: 1,
    gap: 2,
  },
  eyebrow: {
    color: colors.warning,
    fontSize: 10,
    fontWeight: '600',
    letterSpacing: 0,
  },
  machineName: {
    color: colors.text,
    fontSize: 21,
    fontWeight: '600',
    letterSpacing: 0,
  },
  statusTile: {
    width: 92,
    minHeight: 52,
    justifyContent: 'center',
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: radius.button,
    backgroundColor: colors.recessed,
    gap: 2,
    paddingHorizontal: spacing.sm,
  },
  statusTileLabel: {
    color: colors.textMuted,
    fontSize: 9,
    fontWeight: '600',
    letterSpacing: 0,
  },
  statusTileValue: {
    color: colors.text,
    fontFamily: 'monospace',
    fontSize: 14,
    fontWeight: '600',
    letterSpacing: 0,
  },
  statusOk: {
    color: colors.running,
  },
  statusFault: {
    color: colors.fault,
  },
  body: {
    flex: 1,
    paddingHorizontal: spacing.md,
    paddingTop: spacing.md,
    paddingBottom: 42,
  },
});
