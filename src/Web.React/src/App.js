import React from "react";
import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom";
import { App as AntApp, ConfigProvider, theme } from "antd";
import viVN from "antd/locale/vi_VN";
import MainLayout from "./layouts/MainLayout";
import { AuthProvider } from "./contexts/AuthContext";
import ProtectedRoute from "./components/ProtectedRoute";
import AppMessageBridge from "./components/AppMessageBridge";
import LoginPage from "./pages/LoginPage";
import NotFoundPage from "./pages/NotFoundPage";
import FilesPage from "./pages/FilesPage";
import MachineSettings from "./pages/SettingsPage/MachineSettings";
import ModelProfiles from "./pages/SettingsPage/ModelProfiles";
import UserSettings from "./pages/SettingsPage/UserSettings";
import ShelfDeclarationPage from "./pages/ShelfDeclarationPage";
import ProductionReportPage from "./pages/ProductionReportPage";
import {
  PRODUCTION_REPORT_ROLES,
  SHELF_DECLARATION_ROLES
} from "./config/access";

const FONT_STACK = [
  "Inter",
  "\"Segoe UI Variable Text\"",
  "\"Segoe UI\"",
  "Roboto",
  "\"Helvetica Neue\"",
  "Arial",
  "system-ui",
  "-apple-system",
  "BlinkMacSystemFont",
  "sans-serif"
].join(", ");

const appTheme = {
  algorithm: theme.defaultAlgorithm,
  token: {
    colorPrimary: "#2457d6",
    colorInfo: "#2457d6",
    colorSuccess: "#198754",
    colorWarning: "#b7791f",
    colorError: "#d14343",
    colorTextBase: "#152238",
    colorBgBase: "#f5f7fb",
    colorBorder: "#d9e1ec",
    colorBorderSecondary: "#e6edf5",
    colorFillSecondary: "#edf3fb",
    colorFillTertiary: "#f4f7fb",
    colorTextSecondary: "#5f7089",
    zIndexPopupBase: 1700,
    borderRadius: 12,
    borderRadiusLG: 16,
    fontFamily: FONT_STACK,
    fontSize: 15,
    lineHeight: 1.5,
    controlHeight: 40,
    controlHeightLG: 42,
    boxShadowSecondary: "0 4px 14px rgba(15, 23, 42, 0.04)"
  },
  components: {
    Layout: {
      headerBg: "transparent",
      bodyBg: "#f5f7fb",
      siderBg: "transparent",
      footerBg: "transparent"
    },
    Menu: {
      itemBorderRadius: 14,
      itemHeight: 44,
      itemMarginBlock: 2,
      itemSelectedBg: "#eaf1ff",
      itemSelectedColor: "#2457d6",
      itemColor: "#52637e",
      iconSize: 18,
      subMenuItemBg: "transparent"
    },
    Card: {
      borderRadiusLG: 16,
      boxShadowTertiary: "0 3px 10px rgba(15, 23, 42, 0.035)",
      paddingLG: 16
    },
    Button: {
      borderRadius: 14,
      controlHeightLG: 42,
      fontWeight: 600
    },
    Input: {
      borderRadius: 14,
      controlHeightLG: 42,
      activeBorderColor: "#2457d6",
      hoverBorderColor: "#7fa3ef"
    },
    Select: {
      borderRadius: 14,
      controlHeightLG: 42
    },
    Table: {
      borderColor: "#e5ebf4",
      headerBg: "#f7f9fc",
      headerColor: "#334155",
      headerBorderRadius: 14,
      rowHoverBg: "#f8fbff",
      cellPaddingBlock: 12,
      cellPaddingInline: 16
    },
    Modal: {
      borderRadiusLG: 26
    },
    Drawer: {
      colorBgElevated: "#ffffff"
    },
    Tabs: {
      cardBg: "#f3f6fb",
      itemSelectedColor: "#2457d6",
      itemHoverColor: "#2457d6"
    },
    Typography: {
      titleMarginBottom: 0,
      titleMarginTop: 0
    }
  }
};

function App() {
  return (
    <ConfigProvider locale={viVN} theme={appTheme}>
      <AntApp message={{ maxCount: 3, top: 24 }}>
        <AppMessageBridge />
        <AuthProvider>
          <BrowserRouter>
            <Routes>
              <Route path="/login" element={<LoginPage />} />
              <Route path="/" element={<MainLayout />}>
              <Route index element={<Navigate to="/files" replace />} />
                <Route
                  path="files"
                  element={
                    <ProtectedRoute>
                      <FilesPage />
                    </ProtectedRoute>
                  }
                />
                <Route
                  path="settings"
                  element={<Navigate to="/settings/machines" replace />}
                />
                <Route
                  path="settings/machines"
                  element={
                    <ProtectedRoute roles={["Admin", "Technician"]}>
                      <MachineSettings />
                    </ProtectedRoute>
                  }
                />
                <Route
                  path="settings/models"
                  element={
                    <ProtectedRoute roles={["Admin", "Technician"]}>
                      <ModelProfiles />
                    </ProtectedRoute>
                  }
                />
                <Route
                  path="settings/users"
                  element={
                    <ProtectedRoute roles={["Admin"]}>
                      <UserSettings />
                    </ProtectedRoute>
                  }
                />

                <Route
                  path="shelf-declaration"
                  element={
                    <ProtectedRoute roles={SHELF_DECLARATION_ROLES}>
                      <ShelfDeclarationPage />
                    </ProtectedRoute>
                  }
                />

                <Route
                  path="production-report"
                  element={
                    <ProtectedRoute roles={PRODUCTION_REPORT_ROLES}>
                      <ProductionReportPage />
                    </ProtectedRoute>
                  }
                />

                <Route path="*" element={<NotFoundPage />} />
              </Route>
            </Routes>
          </BrowserRouter>
        </AuthProvider>
      </AntApp>
    </ConfigProvider>
  );
}

export default App;
