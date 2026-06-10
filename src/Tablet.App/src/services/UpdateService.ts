import {NativeEventEmitter, NativeModules} from 'react-native';

export interface AppVersion {
  versionName: string;
  versionCode: number;
}

export interface UpdateCheckResult {
  hasUpdate: boolean;
  latestVersion: string;
  latestVersionCode: number | null;
  changelog: string;
  downloadUrl: string | null;
}

export interface DownloadProgressEvent {
  status: 'downloading' | 'done' | 'error';
  progress?: number;
  received?: number;
  total?: number;
  message?: string;
  filePath?: string;
}

interface AppUpdateNativeModule {
  getAppVersion(): Promise<AppVersion>;
  checkInstallPermission(): Promise<boolean>;
  openInstallPermissionSettings(): void;
  downloadAndInstallApk(downloadUrl: string): Promise<void>;
  installApk(filePath: string): Promise<boolean>;
}

interface UpdateApiResponse {
  versionName?: unknown;
  versionCode?: unknown;
  changelog?: unknown;
  downloadUrl?: unknown;
  apkUrl?: unknown;
}

export const OTA_CHECK_URL =
  'http://mes.stivietnam.com:2016/api/ota/check?project=gear-xln&app=android&version=latest';

const appUpdateModule = NativeModules.AppUpdate as AppUpdateNativeModule | undefined;
const appUpdateEmitter = appUpdateModule ? new NativeEventEmitter(appUpdateModule as any) : null;

export const compareVersions = (v1: string, v2: string): number => {
  const cleanV1 = v1.replace(/^v/i, '').split('.');
  const cleanV2 = v2.replace(/^v/i, '').split('.');

  const len = Math.max(cleanV1.length, cleanV2.length);
  for (let i = 0; i < len; i++) {
    const num1 = parseInt(cleanV1[i] || '0', 10);
    const num2 = parseInt(cleanV2[i] || '0', 10);
    if (num1 > num2) {
      return 1;
    }
    if (num1 < num2) {
      return -1;
    }
  }
  return 0;
};

const parseVersionName = (value: unknown): string => {
  return typeof value === 'string' ? value.trim() : '';
};

const parseVersionCode = (value: unknown): number | null => {
  if (typeof value === 'number' && Number.isInteger(value) && value > 0) {
    return value;
  }

  if (typeof value === 'string' && value.trim() !== '') {
    const parsed = Number(value);
    if (Number.isInteger(parsed) && parsed > 0) {
      return parsed;
    }
  }

  return null;
};

const parseDownloadUrl = (updateData: UpdateApiResponse): string | null => {
  const rawUrl =
    typeof updateData.downloadUrl === 'string' && updateData.downloadUrl.trim() !== ''
      ? updateData.downloadUrl.trim()
      : typeof updateData.apkUrl === 'string' && updateData.apkUrl.trim() !== ''
        ? updateData.apkUrl.trim()
        : null;

  return rawUrl;
};

const withCacheBuster = (url: string): string => {
  const separator = url.includes('?') ? '&' : '?';
  return `${url}${separator}_ts=${Date.now()}`;
};

class UpdateService {
  async getAppVersion(): Promise<AppVersion> {
    if (!appUpdateModule?.getAppVersion) {
      return {versionName: '0.0.0', versionCode: 0};
    }

    try {
      return await appUpdateModule.getAppVersion();
    } catch (error) {
      console.error('Error getting app version:', error);
      return {versionName: '0.0.0', versionCode: 0};
    }
  }

  async checkInstallPermission(): Promise<boolean> {
    if (!appUpdateModule?.checkInstallPermission) {
      return false;
    }

    try {
      return await appUpdateModule.checkInstallPermission();
    } catch {
      return false;
    }
  }

  openInstallPermissionSettings(): void {
    appUpdateModule?.openInstallPermissionSettings?.();
  }

  async checkUpdate(updateUrl: string = OTA_CHECK_URL): Promise<UpdateCheckResult> {
    try {
      const current = await this.getAppVersion();
      const response = await fetch(withCacheBuster(updateUrl), {
        headers: {
          'Cache-Control': 'no-cache',
          Pragma: 'no-cache',
          Expires: '0',
        },
      });

      if (!response.ok) {
        throw new Error(`Update server returned HTTP ${response.status}`);
      }

      const updateData = (await response.json()) as UpdateApiResponse;
      const latestVersionCode = parseVersionCode(updateData.versionCode);
      const latestVersionName = parseVersionName(updateData.versionName);
      const latestVersion =
        latestVersionName || (latestVersionCode !== null ? String(latestVersionCode) : current.versionName);
      const changelog =
        typeof updateData.changelog === 'string' && updateData.changelog.trim() !== ''
          ? updateData.changelog.trim()
          : 'No update description available.';
      const downloadUrl = parseDownloadUrl(updateData);

      const hasUpdate =
        (latestVersionCode !== null && latestVersionCode > current.versionCode) ||
        (latestVersionName ? compareVersions(latestVersionName, current.versionName) > 0 : false);

      return {
        hasUpdate,
        latestVersion,
        latestVersionCode,
        changelog,
        downloadUrl,
      };
    } catch (error) {
      console.error('Error checking for updates:', error);
      throw error;
    }
  }

  downloadAndInstall(downloadUrl: string, onProgress: (event: DownloadProgressEvent) => void): Promise<boolean> {
    if (!appUpdateModule?.downloadAndInstallApk) {
      return Promise.reject(new Error('App update native module is not available'));
    }

    return new Promise((resolve, reject) => {
      if (!appUpdateEmitter) {
        appUpdateModule
          .downloadAndInstallApk(downloadUrl)
          .then(() => resolve(true))
          .catch(error => reject(error));
        return;
      }

      const subscription = appUpdateEmitter.addListener('onDownloadProgress', (event: DownloadProgressEvent) => {
        onProgress(event);

        if (event.status === 'done') {
          subscription.remove();
          resolve(true);
        } else if (event.status === 'error') {
          subscription.remove();
          reject(new Error(event.message || 'APK download failed'));
        }
      });

      appUpdateModule.downloadAndInstallApk(downloadUrl).catch(error => {
        subscription.remove();
        reject(error);
      });
    });
  }

  async installApk(filePath: string): Promise<boolean> {
    if (!appUpdateModule?.installApk) {
      throw new Error('App update native module is not available');
    }

    try {
      return await appUpdateModule.installApk(filePath);
    } catch (error) {
      console.error('Error installing APK:', error);
      throw error;
    }
  }
}

export const updateService = new UpdateService();
