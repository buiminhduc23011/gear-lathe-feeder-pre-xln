import type {DeltaConnectionType, PlcDataType, PlcSnapshot, PlcTag} from '@sti/delta-plc';

export type {DeltaConnectionType, PlcDataType, PlcSnapshot, PlcTag} from '@sti/delta-plc';

export interface LineConfig {
  id: string;
  name: string;
  host: string;
  port: number;
  slaveId: number;
  pollIntervalMs: number;
  connectionType?: DeltaConnectionType;
}

export interface AxisLimitTags {
  speedLimitTag?: string;
  negativeLimitTag?: string;
  positiveLimitTag?: string;
}

export interface ManualAxisConfig {
  key: string;
  displayName: string;
  negativeLabel: string;
  positiveLabel: string;
  negativeJogTag: string;
  positiveJogTag: string;
  homeTag: string;
  moveToPointTag: string;
  manualSpeedTag: string;
  movePointTag: string;
  currentPositionTag: string;
  isHomingTag: string;
  isHomedTag: string;
  negativeLimitAlarmTag: string;
  positiveLimitAlarmTag: string;
  servoTag: string;
  limitTags?: AxisLimitTags;
}

export interface ManualCylinderConfig {
  key: string;
  title: string;
  subtitle: string;
  primaryActionLabel: string;
  secondaryActionLabel: string;
  primaryFeedbackLabel: string;
  secondaryFeedbackLabel: string;
  primaryCommandTag: string;
  secondaryCommandTag: string;
  primaryFeedbackTag: string;
  secondaryFeedbackTag: string;
}

export interface ManualOriginActionConfig {
  title: string;
  subtitle: string;
  commandTag: string;
  doneTag?: string;
  axisKey?: string;
}

export interface ManualScreenConfig {
  originActions: ManualOriginActionConfig[];
  axes: ManualAxisConfig[];
  cylinders: ManualCylinderConfig[];
  interlockTags: string[];
  alarmTags: string[];
  autoModeTag: string;
  pressureHealthyTag: string;
}

export interface TabletAppConfig {
  machineName: string;
  machineCode: string;
  lines: LineConfig[];
  tags: PlcTag[];
  manualScreen: ManualScreenConfig;
}
