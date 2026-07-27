package com.gearlinetablet;

import android.content.Context;
import android.content.SharedPreferences;

import androidx.annotation.NonNull;

import com.facebook.react.bridge.Promise;
import com.facebook.react.bridge.ReactApplicationContext;
import com.facebook.react.bridge.ReactContextBaseJavaModule;
import com.facebook.react.bridge.ReactMethod;

public class LineSettingsModule extends ReactContextBaseJavaModule {
    private static final String MODULE_NAME = "LineSettingsStorage";
    private static final String PREFS_NAME = "GearLineTabletSettings";
    private static final String LINES_KEY = "lines_json";

    public LineSettingsModule(ReactApplicationContext reactContext) {
        super(reactContext);
    }

    @NonNull
    @Override
    public String getName() {
        return MODULE_NAME;
    }

    @ReactMethod
    public void loadLines(Promise promise) {
        try {
            SharedPreferences prefs = getReactApplicationContext().getSharedPreferences(PREFS_NAME, Context.MODE_PRIVATE);
            promise.resolve(prefs.getString(LINES_KEY, null));
        } catch (Exception error) {
            promise.reject("LINE_SETTINGS_LOAD_FAILED", error);
        }
    }

    @ReactMethod
    public void saveLines(String linesJson, Promise promise) {
        try {
            SharedPreferences prefs = getReactApplicationContext().getSharedPreferences(PREFS_NAME, Context.MODE_PRIVATE);
            prefs.edit().putString(LINES_KEY, linesJson).apply();
            promise.resolve(true);
        } catch (Exception error) {
            promise.reject("LINE_SETTINGS_SAVE_FAILED", error);
        }
    }
}
