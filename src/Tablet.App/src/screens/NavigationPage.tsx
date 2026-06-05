import React from 'react';
import {Pressable, StyleSheet, Text, View} from 'react-native';
import {Icon, type IconName} from '../components/Icon';
import {appPages, type AppPage} from '../navigation/appPages';
import {colors, radius, spacing} from '../styles/theme';

interface Props {
  onNavigate(page: AppPage): void;
}

const pageIcons: Record<AppPage, IconName> = {
  auto: 'auto',
  manual: 'manual',
  io: 'io',
  history: 'history',
  settings: 'settings',
};

const pageDetails: Record<AppPage, {tone: 'primary' | 'warning' | 'running' | 'idle'}> = {
  auto: {
    tone: 'running',
  },
  manual: {
    tone: 'warning',
  },
  io: {
    tone: 'primary',
  },
  history: {
    tone: 'idle',
  },
  settings: {
    tone: 'primary',
  },
};

const toneColor = {
  primary: colors.primary,
  warning: colors.warning,
  running: colors.running,
  idle: colors.idle,
};

const toneSurface = {
  primary: colors.primarySurface,
  warning: colors.warningSurface,
  running: colors.runningSurface,
  idle: colors.idleSurface,
};

export const NavigationPage = ({onNavigate}: Props) => (
  <View style={styles.root}>
    <View style={styles.grid}>
      {appPages.map(page => {
        const details = pageDetails[page.key];
        const accent = toneColor[details.tone];
        const surface = toneSurface[details.tone];

        return (
          <Pressable
            key={page.key}
            accessibilityLabel={page.label}
            accessibilityRole="button"
            testID="navigation-tile"
            onPress={() => onNavigate(page.key)}
            style={({pressed}) => [
              styles.button,
              {borderColor: pressed ? accent : colors.border},
              pressed && styles.buttonPressed,
            ]}>
            <View style={[styles.accentBar, {backgroundColor: accent}]} />
            <View style={styles.tileCenter}>
              <View style={[styles.iconBay, {backgroundColor: surface, borderColor: accent}]}>
                <Icon name={pageIcons[page.key]} color={accent} size={62} />
              </View>
              <Text adjustsFontSizeToFit numberOfLines={1} style={styles.buttonLabel}>
                {page.label}
              </Text>
            </View>
          </Pressable>
        );
      })}
    </View>
  </View>
);

const styles = StyleSheet.create({
  root: {
    flex: 1,
  },
  grid: {
    flex: 1,
    flexDirection: 'row',
    flexWrap: 'wrap',
    alignContent: 'flex-start',
    gap: spacing.sm,
  },
  button: {
    width: '18.9%',
    minWidth: 118,
    minHeight: 176,
    alignItems: 'center',
    justifyContent: 'center',
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: radius.card,
    backgroundColor: colors.panel,
    overflow: 'hidden',
    paddingHorizontal: spacing.md,
    paddingVertical: spacing.lg,
  },
  buttonPressed: {
    backgroundColor: colors.subpanel,
    transform: [{scale: 0.96}],
  },
  accentBar: {
    position: 'absolute',
    top: 0,
    left: 0,
    right: 0,
    height: 4,
  },
  tileCenter: {
    alignItems: 'center',
    justifyContent: 'center',
    gap: spacing.md,
  },
  iconBay: {
    width: 92,
    height: 88,
    alignItems: 'center',
    justifyContent: 'center',
    borderWidth: 1.5,
    borderRadius: radius.card,
  },
  buttonLabel: {
    color: colors.text,
    fontSize: 18,
    fontWeight: '600',
    letterSpacing: 0,
    maxWidth: '100%',
  },
});
