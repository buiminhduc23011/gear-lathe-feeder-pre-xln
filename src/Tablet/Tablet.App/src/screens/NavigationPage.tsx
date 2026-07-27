import React, {useEffect, useState} from 'react';
import {Pressable, StyleSheet, Text, useWindowDimensions, View} from 'react-native';
import {Icon, type IconName} from '../components/Icon';
import {appPages, type AppPage} from '../navigation/appPages';
import {colors, radius, spacing, typography} from '../styles/theme';

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

const pageDetails: Record<AppPage, {tone: 'primary' | 'warning' | 'running' | 'idle' | 'cyan'; description: string}> = {
  auto: {
    tone: 'running',
    description: 'Chạy chương trình\ntự động theo kịch bản',
  },
  manual: {
    tone: 'warning',
    description: 'Điều khiển thiết bị\nbằng tay',
  },
  io: {
    tone: 'primary',
    description: 'Theo dõi trạng thái\nđầu vào / đầu ra',
  },
  history: {
    tone: 'idle',
    description: 'Xem lại nhật ký và\nlịch sử hoạt động',
  },
  settings: {
    tone: 'cyan',
    description: 'Thiết lập hệ thống\ntheo nhu cầu',
  },
};

const toneColor = {
  primary: colors.primary,
  warning: colors.warning,
  running: colors.running,
  idle: colors.idle,
  cyan: colors.cyan,
};

const toneSurface = {
  primary: colors.primarySurface,
  warning: colors.warningSurface,
  running: colors.runningSurface,
  idle: colors.idleSurface,
  cyan: colors.cyanSurface,
};

const weekdayLabels = [
  'Chủ Nhật',
  'Thứ Hai',
  'Thứ Ba',
  'Thứ Tư',
  'Thứ Năm',
  'Thứ Sáu',
  'Thứ Bảy',
];

const padDatePart = (value: number): string => String(value).padStart(2, '0');

export const formatCurrentDateTime = (date: Date): string =>
  `${weekdayLabels[date.getDay()]}, ${padDatePart(date.getDate())}/${padDatePart(date.getMonth() + 1)}/${date.getFullYear()} • ${padDatePart(date.getHours())}:${padDatePart(date.getMinutes())}:${padDatePart(date.getSeconds())}`;

export const NavigationPage = ({onNavigate}: Props) => {
  const {height, width} = useWindowDimensions();
  const [currentDateTime, setCurrentDateTime] = useState(() => new Date());
  const isCompact = width <= 900 || height <= 520;

  useEffect(() => {
    const timer = setInterval(() => setCurrentDateTime(new Date()), 1000);
    return () => clearInterval(timer);
  }, []);

  return (
  <View style={[styles.root, isCompact && styles.rootCompact]}>
    <View style={[styles.welcomePanel, isCompact && styles.welcomePanelCompact]}>
      <View style={[styles.welcomeIconFrame, isCompact && styles.welcomeIconFrameCompact]}>
        <Icon name="apps" color={colors.primary} size={isCompact ? 40 : 50} />
      </View>
      <View style={[styles.welcomeCopy, isCompact && styles.welcomeCopyCompact]}>
        <Text style={[styles.welcomeTitle, isCompact && styles.welcomeTitleCompact]} numberOfLines={2}>
          Chào mừng bạn trở lại!
        </Text>
        <Text style={[styles.welcomeSubtitle, isCompact && styles.welcomeSubtitleCompact]} numberOfLines={2}>
          {formatCurrentDateTime(currentDateTime)}
        </Text>
      </View>
    </View>
    <View style={[styles.grid, isCompact && styles.gridCompact]}>
      {appPages.map(page => {
        const details = pageDetails[page.key];
        const accent = toneColor[details.tone];
        const surface = toneSurface[details.tone];
        const iconSize = isCompact ? 46 : 58;

        return (
          <Pressable
            key={page.key}
            accessibilityLabel={page.label}
            accessibilityRole="button"
            testID="navigation-tile"
            onPress={() => onNavigate(page.key)}
            style={({pressed}) => [
              styles.button,
              isCompact && styles.buttonCompact,
              {borderColor: accent, backgroundColor: surface},
              pressed && styles.buttonPressed,
            ]}>
            <View style={[styles.accentBar, {backgroundColor: accent}]} />
            <View style={[styles.tileCenter, isCompact && styles.tileCenterCompact]}>
              <View style={[styles.iconBay, isCompact && styles.iconBayCompact, {borderColor: accent}]}>
                <Icon name={pageIcons[page.key]} color={accent} size={iconSize} />
              </View>
              <Text ellipsizeMode="tail" numberOfLines={1} style={[styles.buttonLabel, isCompact && styles.buttonLabelCompact]}>
                {page.label}
              </Text>
              <Text ellipsizeMode="tail" style={[styles.buttonDescription, isCompact && styles.buttonDescriptionCompact]} numberOfLines={2}>
                {details.description}
              </Text>
            </View>
          </Pressable>
        );
      })}
    </View>
  </View>
  );
};

const styles = StyleSheet.create({
  root: {
    flex: 1,
    gap: 22,
  },
  rootCompact: {
    gap: 10,
  },
  welcomePanel: {
    minHeight: 112,
    flexDirection: 'row',
    alignItems: 'center',
    position: 'relative',
    overflow: 'hidden',
    paddingHorizontal: spacing.xl,
  },
  welcomePanelCompact: {
    minHeight: 108,
    paddingHorizontal: spacing.sm,
  },
  welcomeIconFrame: {
    width: 86,
    height: 86,
    alignItems: 'center',
    justifyContent: 'center',
    borderWidth: 1.5,
    borderColor: 'rgba(18, 164, 255, 0.3)',
    borderRadius: 43,
    backgroundColor: 'rgba(18, 164, 255, 0.11)',
  },
  welcomeIconFrameCompact: {
    width: 68,
    height: 68,
    borderRadius: 34,
  },
  welcomeCopy: {
    flex: 1,
    minWidth: 0,
    marginLeft: 28,
    gap: spacing.sm,
  },
  welcomeCopyCompact: {
    marginLeft: 20,
    gap: 4,
  },
  welcomeTitle: {
    color: colors.text,
    fontFamily: typography.fontFamily,
    fontSize: 30,
    fontWeight: typography.weights.semibold,
    letterSpacing: 0,
  },
  welcomeTitleCompact: {
    fontSize: 25,
    lineHeight: 31,
  },
  welcomeSubtitle: {
    color: colors.textSecondary,
    fontFamily: typography.fontFamily,
    fontSize: 22,
    fontWeight: typography.weights.regular,
    letterSpacing: 0,
  },
  welcomeSubtitleCompact: {
    fontSize: 18,
    lineHeight: 24,
  },
  grid: {
    flex: 1,
    flexDirection: 'row',
    flexWrap: 'nowrap',
    alignItems: 'stretch',
    gap: 20,
  },
  gridCompact: {
    gap: 8,
  },
  button: {
    flex: 1,
    minWidth: 0,
    alignItems: 'center',
    justifyContent: 'center',
    borderWidth: 1.5,
    borderRadius: 4,
    overflow: 'hidden',
    paddingHorizontal: spacing.lg,
    paddingVertical: 18,
  },
  buttonCompact: {
    paddingHorizontal: spacing.sm,
    paddingVertical: 12,
  },
  buttonPressed: {
    transform: [{scale: 0.985}],
  },
  accentBar: {
    position: 'absolute',
    top: 0,
    left: 0,
    right: 0,
    height: 3,
  },
  tileCenter: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
    gap: 12,
    width: '100%',
  },
  tileCenterCompact: {
    gap: 9,
  },
  iconBay: {
    width: 106,
    height: 106,
    alignItems: 'center',
    justifyContent: 'center',
    borderWidth: 1.5,
    borderRadius: 53,
    backgroundColor: 'rgba(4, 17, 31, 0.38)',
  },
  iconBayCompact: {
    width: 76,
    height: 76,
    borderRadius: 38,
  },
  buttonLabel: {
    color: colors.text,
    fontFamily: typography.fontFamily,
    width: '100%',
    minHeight: 32,
    fontSize: 23,
    lineHeight: 30,
    fontWeight: typography.weights.semibold,
    letterSpacing: 0,
    maxWidth: '100%',
    textAlign: 'center',
  },
  buttonLabelCompact: {
    minHeight: 25,
    fontSize: 19,
    lineHeight: 25,
  },
  buttonDescription: {
    minHeight: 50,
    color: colors.textSecondary,
    fontFamily: typography.fontFamily,
    fontSize: 17,
    lineHeight: 25,
    fontWeight: typography.weights.regular,
    letterSpacing: 0,
    textAlign: 'center',
  },
  buttonDescriptionCompact: {
    minHeight: 38,
    fontSize: 13,
    lineHeight: 18,
  },
});
