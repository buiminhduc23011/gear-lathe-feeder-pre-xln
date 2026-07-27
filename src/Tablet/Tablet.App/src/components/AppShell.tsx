import React from 'react';
import {Pressable, StatusBar, StyleSheet, Text, useWindowDimensions, View} from 'react-native';
import {colors, typography} from '../styles/theme';
import {AlarmBar, AlarmBarState} from './AlarmBar';
import {AppLogo} from './AppLogo';

interface Props {
  isConnected: boolean;
  showHomeButton: boolean;
  screenEyebrow?: string;
  screenTitle?: string;
  alarmText?: string;
  alarmState?: AlarmBarState;
  onHome(): void;
  currentPage?: string;
  onNavigate?(page: any): void;
  children: React.ReactNode;
}

export const AppShell = React.memo(({
  isConnected,
  screenEyebrow,
  screenTitle,
  alarmText,
  alarmState = AlarmBarState.Normal,
  onHome,
  children,
}: Props) => {
  const {height, width} = useWindowDimensions();
  const isCompact = width <= 900 || height <= 520;
  const headerEyebrow = screenEyebrow ?? 'GEAR LINE';
  const headerTitle = screenTitle ?? 'Bảng điều khiển';

  return (
    <View style={styles.safeArea}>
      <StatusBar hidden />
      <View style={[styles.topBar, isCompact && styles.topBarCompact]}>
        <Pressable
          accessibilityLabel="Home"
          accessibilityRole="button"
          hitSlop={8}
          onPress={onHome}
          style={({pressed}) => [styles.logoButton, pressed && styles.logoButtonPressed]}>
          <AppLogo style={[styles.logo, isCompact && styles.logoCompact]} />
        </Pressable>

        <View style={[styles.cellIdentity, isCompact && styles.cellIdentityCompact]}>
          <Text style={[styles.eyebrow, isCompact && styles.eyebrowCompact]} numberOfLines={1}>
            {headerEyebrow}
          </Text>
          <Text ellipsizeMode="tail" style={[styles.machineName, isCompact && styles.machineNameCompact]} numberOfLines={1}>
            {headerTitle}
          </Text>
        </View>

        {alarmText ? <AlarmBar text={alarmText} state={alarmState} /> : null}
      </View>

      <View style={[styles.body, isCompact && styles.bodyCompact]}>{children}</View>
    </View>
  );
});

const styles = StyleSheet.create({
  safeArea: {
    flex: 1,
    backgroundColor: colors.window,
    overflow: 'hidden',
  },
  topBar: {
    minHeight: 102,
    flexDirection: 'row',
    alignItems: 'center',
    borderBottomWidth: 1.5,
    borderBottomColor: colors.divider,
    backgroundColor: 'rgba(7, 21, 36, 0.94)',
    paddingHorizontal: 44,
    gap: 20,
  },
  topBarCompact: {
    minHeight: 70,
    paddingHorizontal: 20,
    gap: 12,
  },
  logoButton: {
    alignItems: 'center',
    justifyContent: 'center',
  },
  logoButtonPressed: {
    opacity: 0.82,
  },
  logo: {
    width: 109,
    height: 70,
  },
  logoCompact: {
    width: 75,
    height: 48,
  },
  cellIdentity: {
    width: 360,
    minWidth: 118,
    flexShrink: 1,
    gap: 4,
  },
  cellIdentityCompact: {
    width: 180,
    minWidth: 160,
    flexShrink: 0,
  },
  eyebrow: {
    color: colors.warning,
    fontFamily: typography.fontFamily,
    fontSize: 14,
    fontWeight: typography.weights.semibold,
    letterSpacing: 0,
  },
  eyebrowCompact: {
    fontSize: 12,
  },
  machineName: {
    color: colors.text,
    fontFamily: typography.fontFamily,
    fontSize: 32,
    fontWeight: typography.weights.semibold,
    letterSpacing: 0,
  },
  machineNameCompact: {
    fontSize: 22,
    lineHeight: 30,
    paddingBottom: 2,
  },
  body: {
    flex: 1,
    paddingHorizontal: 44,
    paddingTop: 24,
    paddingBottom: 24,
  },
  bodyCompact: {
    paddingHorizontal: 20,
    paddingTop: 10,
    paddingBottom: 10,
  },
});
