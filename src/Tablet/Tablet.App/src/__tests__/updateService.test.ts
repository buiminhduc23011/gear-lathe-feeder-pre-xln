import {OTA_CHECK_URL, compareVersions, updateService} from '../services/UpdateService';

const setFetchResponse = (payload: unknown) => {
  const fetchMock = jest.fn();
  fetchMock.mockResolvedValue({
    ok: true,
    json: async () => payload,
  });
  (globalThis as any).fetch = fetchMock;
  return fetchMock;
};

describe('update service', () => {
  afterEach(() => {
    jest.restoreAllMocks();
    delete (globalThis as any).fetch;
  });

  it('uses versionCode when the server versionName is older', async () => {
    jest.spyOn(updateService, 'getAppVersion').mockResolvedValue({versionName: '0.1.0', versionCode: 1});
    const fetchMock = setFetchResponse({
      versionName: '0.0.2',
      versionCode: 2,
      changelog: 'Init app',
      downloadUrl: 'https://example.com/update.apk',
    });

    const result = await updateService.checkUpdate('https://example.com/update.json');

    expect(fetchMock).toHaveBeenCalledWith(
      expect.stringMatching(/^https:\/\/example\.com\/update\.json\?_ts=\d+$/),
      expect.objectContaining({
        headers: expect.objectContaining({
          'Cache-Control': 'no-cache',
          Pragma: 'no-cache',
          Expires: '0',
        }),
      }),
    );
    expect(result).toEqual({
      hasUpdate: true,
      latestVersion: '0.0.2',
      latestVersionCode: 2,
      changelog: 'Init app',
      downloadUrl: 'https://example.com/update.apk',
    });
  });

  it('calls the OTA check endpoint by default', async () => {
    jest.spyOn(updateService, 'getAppVersion').mockResolvedValue({versionName: '0.1.0', versionCode: 1});
    const fetchMock = setFetchResponse({
      versionName: '0.0.2',
      versionCode: 2,
      changelog: 'Init app',
      downloadUrl: 'https://example.com/update.apk',
    });

    await updateService.checkUpdate();

    expect(fetchMock).toHaveBeenCalledWith(
      expect.stringMatching(
        /^http:\/\/mes\.stivietnam\.com:2016\/api\/ota\/check\?project=gear-xln&app=android&version=latest&_ts=\d+$/,
      ),
      expect.any(Object),
    );
  });

  it('falls back to apkUrl when downloadUrl is missing', async () => {
    jest.spyOn(updateService, 'getAppVersion').mockResolvedValue({versionName: '0.1.0', versionCode: 1});
    setFetchResponse({
      versionName: '0.1.1',
      versionCode: 3,
      changelog: 'APK manifest',
      apkUrl: 'https://example.com/fallback.apk',
    });

    const result = await updateService.checkUpdate('https://example.com/update.json');

    expect(result.hasUpdate).toBe(true);
    expect(result.downloadUrl).toBe('https://example.com/fallback.apk');
  });

  it('compares semantic versions when versionCode is absent', async () => {
    jest.spyOn(updateService, 'getAppVersion').mockResolvedValue({versionName: '0.1.0', versionCode: 1});
    setFetchResponse({
      versionName: '0.1.2',
      changelog: 'Semver only',
      downloadUrl: 'https://example.com/update.apk',
    });

    const result = await updateService.checkUpdate('https://example.com/update.json');

    expect(result.hasUpdate).toBe(true);
    expect(result.latestVersionCode).toBeNull();
  });

  it('compares version fragments numerically', () => {
    expect(compareVersions('v1.2.10', '1.2.2')).toBe(1);
    expect(compareVersions('1.2.0', '1.2.0')).toBe(0);
    expect(compareVersions('1.1.9', '1.2.0')).toBe(-1);
  });
});
