import type {ManualAxisConfig, ManualCylinderConfig, ManualOriginActionConfig, ManualScreenConfig, PlcSnapshot} from '../types/plc';

export interface AxisLimitProfile {
  speedLimit?: number;
  negativeLimit?: number;
  positiveLimit?: number;
}

export interface ManualAxisState {
  config: ManualAxisConfig;
  currentPosition: number;
  manualSpeed: number;
  movePoint: number;
  isNegativeJogActive: boolean;
  isPositiveJogActive: boolean;
  isHomeCommandActive: boolean;
  isMoveToPointCommandActive: boolean;
  isHoming: boolean;
  isHomed: boolean;
  isNegativeLimitActive: boolean;
  isPositiveLimitActive: boolean;
  isServoOn: boolean;
  limitProfile: AxisLimitProfile;
  canJogNegative: boolean;
  canJogPositive: boolean;
}

export interface ManualCylinderState {
  config: ManualCylinderConfig;
  isPrimaryCommandActive: boolean;
  isSecondaryCommandActive: boolean;
  isPrimaryFeedbackActive: boolean;
  isSecondaryFeedbackActive: boolean;
}

export interface ManualOriginActionState {
  config: ManualOriginActionConfig;
  isActive: boolean;
  isDone: boolean;
  isCommandActive: boolean;
}

export interface ManualSummaryState {
  hasActiveAlarm: boolean;
  alarmSummaryText: string;
  hasSevereInterlock: boolean;
  interlockSummaryText: string;
  currentModeText: string;
  servoSummaryText: string;
  homingSummaryText: string;
  pressureText: string;
}

const tolerance = 0.0001;

export const readBool = (snapshot: PlcSnapshot, tagName: string): boolean => snapshot[tagName] === true;

export const readNumber = (snapshot: PlcSnapshot, tagName: string): number => {
  const value = snapshot[tagName];
  return typeof value === 'number' && Number.isFinite(value) ? value : 0;
};

const formatNumber = (value: number): string => {
  if (!Number.isFinite(value)) {
    return '0';
  }
  return Number(value.toFixed(3)).toString();
};

export const formatManualNumber = formatNumber;

export const getAxisLimitProfile = (axis: ManualAxisConfig, snapshot: PlcSnapshot): AxisLimitProfile => ({
  speedLimit: optionalLimit(snapshot, axis.limitTags?.speedLimitTag),
  negativeLimit: optionalLimit(snapshot, axis.limitTags?.negativeLimitTag),
  positiveLimit: optionalLimit(snapshot, axis.limitTags?.positiveLimitTag),
});

export const getAxisStates = (config: ManualScreenConfig, snapshot: PlcSnapshot): ManualAxisState[] =>
  config.axes.map(axis => {
    const currentPosition = readNumber(snapshot, axis.currentPositionTag);
    const limitProfile = getAxisLimitProfile(axis, snapshot);
    return {
      config: axis,
      currentPosition,
      manualSpeed: readNumber(snapshot, axis.manualSpeedTag),
      movePoint: readNumber(snapshot, axis.movePointTag),
      isNegativeJogActive: readBool(snapshot, axis.negativeJogTag),
      isPositiveJogActive: readBool(snapshot, axis.positiveJogTag),
      isHomeCommandActive: readBool(snapshot, axis.homeTag),
      isMoveToPointCommandActive: readBool(snapshot, axis.moveToPointTag),
      isHoming: readBool(snapshot, axis.isHomingTag),
      isHomed: readBool(snapshot, axis.isHomedTag),
      isNegativeLimitActive: readBool(snapshot, axis.negativeLimitAlarmTag),
      isPositiveLimitActive: readBool(snapshot, axis.positiveLimitAlarmTag),
      isServoOn: readBool(snapshot, axis.servoTag),
      limitProfile,
      canJogNegative: limitProfile.negativeLimit === undefined || currentPosition > limitProfile.negativeLimit + tolerance,
      canJogPositive: limitProfile.positiveLimit === undefined || currentPosition < limitProfile.positiveLimit - tolerance,
    };
  });

export const getCylinderStates = (config: ManualScreenConfig, snapshot: PlcSnapshot): ManualCylinderState[] =>
  config.cylinders.map(cylinder => ({
    config: cylinder,
    isPrimaryCommandActive: readBool(snapshot, cylinder.primaryCommandTag),
    isSecondaryCommandActive: readBool(snapshot, cylinder.secondaryCommandTag),
    isPrimaryFeedbackActive: readBool(snapshot, cylinder.primaryFeedbackTag),
    isSecondaryFeedbackActive: readBool(snapshot, cylinder.secondaryFeedbackTag),
  }));

export const getOriginActionStates = (config: ManualScreenConfig, snapshot: PlcSnapshot): ManualOriginActionState[] => {
  const axes = getAxisStates(config, snapshot);
  const axisLookup = new Map(axes.map(axis => [axis.config.key, axis]));
  const allDone = axes.every(axis => axis.isHomed)
    && readBool(snapshot, 'manual.home_cart_1_clamp_done')
    && readBool(snapshot, 'manual.home_cart_2_clamp_done')
    && readBool(snapshot, 'manual.home_rotate_cylinder_done')
    && readBool(snapshot, 'manual.home_tool_clamp_done')
    && readBool(snapshot, 'manual.home_tool_change_done');
  const anyBusy = axes.some(axis => axis.isHoming || axis.isHomeCommandActive)
    || readBool(snapshot, 'manual.home_all')
    || readBool(snapshot, 'manual.home_cart_1_clamp')
    || readBool(snapshot, 'manual.home_cart_2_clamp')
    || readBool(snapshot, 'manual.home_rotate_cylinder')
    || readBool(snapshot, 'manual.home_tool_change_cylinder')
    || readBool(snapshot, 'manual.home_tool_clamp_cylinder');

  return config.originActions.map(action => {
    if (action.commandTag === 'manual.home_all') {
      return {config: action, isActive: anyBusy, isDone: allDone, isCommandActive: readBool(snapshot, action.commandTag)};
    }

    if (action.axisKey) {
      const axis = axisLookup.get(action.axisKey);
      return {
        config: action,
        isActive: Boolean(axis?.isHoming || axis?.isHomeCommandActive),
        isDone: Boolean(axis?.isHomed),
        isCommandActive: readBool(snapshot, action.commandTag),
      };
    }

    return {
      config: action,
      isActive: readBool(snapshot, action.commandTag),
      isDone: action.doneTag ? readBool(snapshot, action.doneTag) : false,
      isCommandActive: readBool(snapshot, action.commandTag),
    };
  });
};

export const getSummaryState = (config: ManualScreenConfig, snapshot: PlcSnapshot): ManualSummaryState => {
  const alarmTags = config.alarmTags.filter(tagName => readBool(snapshot, tagName));
  const interlockTags = config.interlockTags.filter(tagName => readBool(snapshot, tagName));
  const axes = getAxisStates(config, snapshot);
  const homing = getOriginActionStates(config, snapshot).filter(item => item.isActive).map(item => item.config.title);
  const servoOn = axes.filter(axis => axis.isServoOn).map(axis => axis.config.displayName);

  return {
    hasActiveAlarm: alarmTags.length > 0,
    alarmSummaryText: alarmTags.length > 0 ? alarmTags.slice(0, 3).join(' | ') : 'Không có alarm đang kích hoạt',
    hasSevereInterlock: interlockTags.length > 0,
    interlockSummaryText: interlockTags.length > 0 ? `Khóa thao tác: ${interlockTags.join(', ')}` : 'Liên động an toàn OK',
    currentModeText: readBool(snapshot, config.autoModeTag) ? 'Auto' : 'Manual / Service',
    servoSummaryText: servoOn.length > 0 ? `ON: ${servoOn.join(', ')}` : 'Tất cả servo OFF',
    homingSummaryText: homing.length > 0 ? homing.slice(0, 3).join(', ') : 'Không có lệnh home đang chạy',
    pressureText: readBool(snapshot, config.pressureHealthyTag) ? 'Áp khí OK' : 'Áp khí thấp',
  };
};

export const validateAxisSpeed = (value: number, profile: AxisLimitProfile): string => {
  if (!Number.isFinite(value) || value < 0) {
    return 'Tốc độ phải lớn hơn hoặc bằng 0.';
  }
  if (profile.speedLimit !== undefined && profile.speedLimit > 0 && value > profile.speedLimit + tolerance) {
    return `Tốc độ phải nhỏ hơn hoặc bằng ${formatNumber(profile.speedLimit)} mm/s.`;
  }
  return '';
};

export const validateAxisPosition = (value: number, profile: AxisLimitProfile): string => {
  if (!Number.isFinite(value)) {
    return 'Giá trị điểm chạy không hợp lệ.';
  }
  if (profile.negativeLimit !== undefined && profile.negativeLimit !== 0 && value < profile.negativeLimit - tolerance) {
    return `Giá trị phải lớn hơn hoặc bằng ${formatNumber(profile.negativeLimit)} mm.`;
  }
  if (profile.positiveLimit !== undefined && profile.positiveLimit !== 0 && value > profile.positiveLimit + tolerance) {
    return `Giá trị phải nhỏ hơn hoặc bằng ${formatNumber(profile.positiveLimit)} mm.`;
  }
  return '';
};

const optionalLimit = (snapshot: PlcSnapshot, tagName?: string): number | undefined => {
  if (!tagName) {
    return undefined;
  }
  const value = readNumber(snapshot, tagName);
  return value === 0 ? undefined : value;
};
