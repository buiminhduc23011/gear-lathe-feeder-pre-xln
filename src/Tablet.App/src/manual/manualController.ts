import type {ManualAxisConfig, ManualScreenConfig, PlcSnapshot} from '../types/plc';
import {getAxisLimitProfile, readBool, validateAxisPosition, validateAxisSpeed} from './manualSelectors';

export interface ManualPlcPort {
  readonly isConnected: boolean;
  snapshot(): PlcSnapshot;
  write(tagName: string, value: unknown): Promise<void>;
}

export class ManualController {
  private activeJogTagName: string | null = null;

  constructor(private readonly config: ManualScreenConfig, private readonly plc: ManualPlcPort) {}

  get activeJogTag(): string | null {
    return this.activeJogTagName;
  }

  canIssueCommands(snapshot: PlcSnapshot = this.plc.snapshot()): boolean {
    return this.plc.isConnected && !this.config.interlockTags.some(tagName => readBool(snapshot, tagName));
  }

  async pressCommand(tagName: string): Promise<void> {
    if (!this.plc.isConnected) {
      return;
    }
    const cylinder = this.resolveCylinderCommand(tagName);
    if (cylinder) {
      await this.plc.write(cylinder.oppositeTag, false);
    }
    this.activeJogTagName = tagName;
    await this.plc.write(tagName, true);
  }

  async releaseCommand(tagName: string): Promise<void> {
    if (!this.plc.isConnected) {
      return;
    }
    await this.plc.write(tagName, false);
    if (this.activeJogTagName === tagName) {
      this.activeJogTagName = null;
    }
  }

  async runOneShot(tagName: string): Promise<void> {
    if (!this.canExecuteOneShot(tagName)) {
      return;
    }

    const cylinder = this.resolveCylinderCommand(tagName);
    if (cylinder) {
      await this.plc.write(cylinder.oppositeTag, false);
    }

    await this.plc.write(tagName, true);
  }

  async applyAxisSpeed(axis: ManualAxisConfig, rawValue: string | number): Promise<string> {
    const value = Number(rawValue);
    const error = validateAxisSpeed(value, getAxisLimitProfile(axis, this.plc.snapshot()));
    if (error || !this.canIssueCommands()) {
      return error || 'PLC offline hoặc đang khóa thao tác.';
    }

    await this.plc.write(axis.manualSpeedTag, value);
    return '';
  }

  async writeMovePoint(axis: ManualAxisConfig, rawValue: string | number): Promise<string> {
    const value = Number(rawValue);
    const error = validateAxisPosition(value, getAxisLimitProfile(axis, this.plc.snapshot()));
    if (error || !this.canIssueCommands()) {
      return error || 'PLC offline hoặc đang khóa thao tác.';
    }

    await this.plc.write(axis.movePointTag, value);
    return '';
  }

  async moveAxisToPoint(axis: ManualAxisConfig, rawValue: string | number): Promise<string> {
    const snapshot = this.plc.snapshot();
    const value = Number(rawValue);
    const error = validateAxisPosition(value, getAxisLimitProfile(axis, snapshot));
    if (error || !this.canMoveAxisToPoint(axis, snapshot)) {
      return error || 'Trục đang bận hoặc PLC đang khóa thao tác.';
    }

    await this.plc.write(axis.movePointTag, value);
    await this.plc.write(axis.moveToPointTag, true);
    return '';
  }

  async startJog(tagName: string): Promise<void> {
    if (!this.canStartJog(tagName)) {
      return;
    }

    this.activeJogTagName = tagName;
    try {
      await this.plc.write(tagName, true);
    } catch (error) {
      this.activeJogTagName = null;
      throw error;
    }
  }

  async stopJog(tagName?: string | null): Promise<void> {
    const resolvedTag = tagName ?? this.activeJogTagName;
    if (!resolvedTag) {
      return;
    }

    try {
      if (this.plc.isConnected) {
        await this.plc.write(resolvedTag, false);
      }
    } finally {
      if (this.activeJogTagName === resolvedTag) {
        this.activeJogTagName = null;
      }
    }
  }

  async clearStaleJogTags(): Promise<void> {
    if (!this.plc.isConnected) {
      return;
    }

    const snapshot = this.plc.snapshot();
    for (const axis of this.config.axes) {
      for (const tagName of [axis.negativeJogTag, axis.positiveJogTag]) {
        if (tagName !== this.activeJogTagName && readBool(snapshot, tagName)) {
          await this.plc.write(tagName, false);
        }
      }
    }
  }

  private canExecuteOneShot(tagName: string): boolean {
    const snapshot = this.plc.snapshot();
    if (!this.canIssueCommands(snapshot)) {
      return false;
    }

    const origin = this.config.originActions.find(action => action.commandTag === tagName);
    if (origin) {
      return !readBool(snapshot, origin.commandTag);
    }

    const cylinder = this.resolveCylinderCommand(tagName);
    return cylinder ? !readBool(snapshot, tagName) : false;
  }

  private canMoveAxisToPoint(axis: ManualAxisConfig, snapshot: PlcSnapshot): boolean {
    return this.canIssueCommands(snapshot)
      && !readBool(snapshot, axis.moveToPointTag)
      && !readBool(snapshot, axis.homeTag)
      && !readBool(snapshot, axis.isHomingTag);
  }

  private canStartJog(tagName: string): boolean {
    const snapshot = this.plc.snapshot();
    if (!this.canIssueCommands(snapshot) || this.activeJogTagName) {
      return false;
    }

    const axis = this.config.axes.find(item => item.negativeJogTag === tagName || item.positiveJogTag === tagName);
    if (!axis) {
      return false;
    }

    const profile = getAxisLimitProfile(axis, snapshot);
    const currentPosition = Number(snapshot[axis.currentPositionTag] ?? 0);
    if (axis.negativeJogTag === tagName) {
      return profile.negativeLimit === undefined || currentPosition > profile.negativeLimit + 0.0001;
    }
    return profile.positiveLimit === undefined || currentPosition < profile.positiveLimit - 0.0001;
  }

  private resolveCylinderCommand(tagName: string): {oppositeTag: string} | null {
    for (const cylinder of this.config.cylinders) {
      if (cylinder.primaryCommandTag === tagName) {
        return {oppositeTag: cylinder.secondaryCommandTag};
      }
      if (cylinder.secondaryCommandTag === tagName) {
        return {oppositeTag: cylinder.primaryCommandTag};
      }
    }
    return null;
  }
}
