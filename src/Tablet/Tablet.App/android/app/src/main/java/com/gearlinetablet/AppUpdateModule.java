package com.gearlinetablet;

import android.content.Context;
import android.content.Intent;
import android.content.pm.PackageInfo;
import android.content.pm.PackageManager;
import android.net.Uri;
import android.os.Build;
import android.provider.Settings;
import androidx.annotation.NonNull;
import androidx.core.content.FileProvider;

import com.facebook.react.bridge.Arguments;
import com.facebook.react.bridge.Promise;
import com.facebook.react.bridge.ReactApplicationContext;
import com.facebook.react.bridge.ReactContextBaseJavaModule;
import com.facebook.react.bridge.ReactMethod;
import com.facebook.react.bridge.WritableMap;
import com.facebook.react.modules.core.DeviceEventManagerModule;

import java.io.BufferedInputStream;
import java.io.File;
import java.io.FileOutputStream;
import java.io.InputStream;
import java.net.HttpURLConnection;
import java.net.URL;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;

public class AppUpdateModule extends ReactContextBaseJavaModule {
    private static final String MODULE_NAME = "AppUpdate";
    private final ExecutorService executorService = Executors.newSingleThreadExecutor();
    private boolean isDownloading = false;

    public AppUpdateModule(ReactApplicationContext reactContext) {
        super(reactContext);
    }

    @NonNull
    @Override
    public String getName() {
        return MODULE_NAME;
    }

    @ReactMethod
    public void addListener(String eventName) {
        // Required by NativeEventEmitter.
    }

    @ReactMethod
    public void removeListeners(double count) {
        // Required by NativeEventEmitter.
    }

    @ReactMethod
    public void getAppVersion(Promise promise) {
        try {
            ReactApplicationContext context = getReactApplicationContext();
            PackageInfo pInfo = context.getPackageManager().getPackageInfo(context.getPackageName(), 0);
            WritableMap map = Arguments.createMap();
            map.putString("versionName", pInfo.versionName);
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.P) {
                map.putDouble("versionCode", pInfo.getLongVersionCode());
            } else {
                map.putDouble("versionCode", pInfo.versionCode);
            }
            promise.resolve(map);
        } catch (PackageManager.NameNotFoundException e) {
            promise.reject("VERSION_ERROR", "Không lấy được thông tin phiên bản", e);
        }
    }

    @ReactMethod
    public void checkInstallPermission(Promise promise) {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            boolean hasPermission = getReactApplicationContext().getPackageManager().canRequestPackageInstalls();
            promise.resolve(hasPermission);
        } else {
            promise.resolve(true);
        }
    }

    @ReactMethod
    public void openInstallPermissionSettings() {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            Uri packageUri = Uri.parse("package:" + getReactApplicationContext().getPackageName());
            Intent intent = new Intent(Settings.ACTION_MANAGE_UNKNOWN_APP_SOURCES, packageUri);
            intent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
            getReactApplicationContext().startActivity(intent);
        }
    }

    @ReactMethod
    public void installApk(String filePath, Promise promise) {
        try {
            File apkFile = new File(filePath);
            if (!apkFile.exists()) {
                promise.reject("FILE_NOT_FOUND", "Không tìm thấy file APK tại đường dẫn: " + filePath);
                return;
            }

            ReactApplicationContext context = getReactApplicationContext();

            // Kiểm tra quyền trên Android 8.0+
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
                if (!context.getPackageManager().canRequestPackageInstalls()) {
                    openInstallPermissionSettings();
                    promise.reject("PERMISSION_DENIED", "Yêu cầu cấp quyền cài đặt ứng dụng từ nguồn không xác định");
                    return;
                }
            }

            Uri apkUri;
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.N) {
                String authority = context.getPackageName() + ".fileprovider";
                apkUri = FileProvider.getUriForFile(context, authority, apkFile);
            } else {
                apkUri = Uri.fromFile(apkFile);
            }

            Intent intent = new Intent(Intent.ACTION_VIEW);
            intent.setDataAndType(apkUri, "application/vnd.android.package-archive");
            intent.addFlags(Intent.FLAG_GRANT_READ_URI_PERMISSION);
            intent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
            context.startActivity(intent);

            promise.resolve(true);
        } catch (Exception e) {
            promise.reject("INSTALL_ERROR", "Lỗi trong quá trình kích hoạt trình cài đặt APK", e);
        }
    }

    @ReactMethod
    public void downloadAndInstallApk(final String downloadUrl, final Promise promise) {
        if (isDownloading) {
            promise.reject("DOWNLOAD_IN_PROGRESS", "Đang có một tiến trình tải về đang chạy");
            return;
        }

        isDownloading = true;
        executorService.execute(new Runnable() {
            @Override
            public void run() {
                File outputApkFile = null;
                try {
                    ReactApplicationContext context = getReactApplicationContext();
                    URL url = new URL(downloadUrl);
                    HttpURLConnection connection = (HttpURLConnection) url.openConnection();
                    connection.setRequestMethod("GET");
                    connection.setConnectTimeout(15000);
                    connection.setReadTimeout(15000);
                    connection.connect();

                    if (connection.getResponseCode() != HttpURLConnection.HTTP_OK) {
                        throw new Exception("Server trả về mã lỗi HTTP: " + connection.getResponseCode());
                    }

                    int fileLength = connection.getContentLength();

                    // Lưu file vào thư mục Cache ngoài (để FileProvider có quyền chia sẻ dễ dàng hơn)
                    File cacheDir = context.getExternalCacheDir();
                    if (cacheDir == null) {
                        cacheDir = context.getCacheDir();
                    }
                    outputApkFile = new File(cacheDir, "update.apk");
                    if (outputApkFile.exists()) {
                        outputApkFile.delete();
                    }

                    InputStream input = new BufferedInputStream(connection.getInputStream(), 8192);
                    FileOutputStream output = new FileOutputStream(outputApkFile);

                    byte[] data = new byte[8192];
                    long total = 0;
                    int count;
                    int lastProgress = -1;

                    while ((count = input.read(data)) != -1) {
                        total += count;
                        output.write(data, 0, count);

                        if (fileLength > 0) {
                            int progress = (int) (total * 100 / fileLength);
                            if (progress != lastProgress) {
                                lastProgress = progress;
                                WritableMap progressMap = Arguments.createMap();
                                progressMap.putString("status", "downloading");
                                progressMap.putInt("progress", progress);
                                progressMap.putDouble("received", total);
                                progressMap.putDouble("total", fileLength);
                                sendEvent("onDownloadProgress", progressMap);
                            }
                        }
                    }

                    output.flush();
                    output.close();
                    input.close();

                    isDownloading = false;

                    // Gửi event hoàn tất tải về
                    WritableMap doneMap = Arguments.createMap();
                    doneMap.putString("status", "done");
                    doneMap.putInt("progress", 100);
                    doneMap.putDouble("received", total);
                    doneMap.putDouble("total", total);
                    doneMap.putString("filePath", outputApkFile.getAbsolutePath());
                    sendEvent("onDownloadProgress", doneMap);

                    // Tự động gọi cài đặt
                    final File finalFile = outputApkFile;
                    context.runOnUiQueueThread(new Runnable() {
                        @Override
                        public void run() {
                            installApk(finalFile.getAbsolutePath(), promise);
                        }
                    });

                } catch (Exception e) {
                    isDownloading = false;
                    WritableMap errorMap = Arguments.createMap();
                    errorMap.putString("status", "error");
                    errorMap.putString("message", e.getMessage());
                    sendEvent("onDownloadProgress", errorMap);
                    
                    promise.reject("DOWNLOAD_ERROR", "Lỗi tải file APK cập nhật: " + e.getMessage(), e);
                }
            }
        });
    }

    private void sendEvent(String eventName, WritableMap params) {
        getReactApplicationContext()
            .getJSModule(DeviceEventManagerModule.RCTDeviceEventEmitter.class)
            .emit(eventName, params);
    }
}
