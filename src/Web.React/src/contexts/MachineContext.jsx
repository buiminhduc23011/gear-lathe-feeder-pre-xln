import React, { createContext, useCallback, useContext, useEffect, useMemo, useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { API_ENDPOINTS, apiClient, getApiErrorMessage } from "../config/api";
import { showErrorMessage } from "../utils/appMessage";

export const STORAGE_KEY_LAST_MACHINE = "last_selected_machine_code";

export const MachineContext = createContext({
  machines: [],
  loadingMachines: false,
  currentMachine: null,
  currentMachineCode: null,
  currentMachineId: null,
  changeMachine: () => {},
  getMachinePath: (path) => path,
  fetchMachines: async () => []
});

export function useMachine() {
  return useContext(MachineContext);
}

export function useMachineContext() {
  return useContext(MachineContext);
}

/**
 * Trích xuất machineCode từ đường dẫn URL (dạng /m/:machineCode/...)
 */
export function extractMachineCodeFromPath(pathname) {
  if (!pathname) return null;
  const match = pathname.match(/^\/m\/([^/]+)/);
  return match ? decodeURIComponent(match[1]) : null;
}

/**
 * Trích xuất đường dẫn con đằng sau /m/:machineCode/
 */
export function extractSubPathFromPath(pathname) {
  if (!pathname) return "";
  return pathname.replace(/^\/m\/[^/]+/, "");
}

export function MachineProvider({ children }) {
  const [machines, setMachines] = useState([]);
  const [loadingMachines, setLoadingMachines] = useState(true);
  const location = useLocation();
  const navigate = useNavigate();

  const fetchMachines = useCallback(async () => {
    setLoadingMachines(true);
    try {
      const response = await apiClient.get(API_ENDPOINTS.machines);
      const list = Array.isArray(response.data) ? response.data : response.data?.data ?? [];
      setMachines(list);
      return list;
    } catch (error) {
      showErrorMessage(getApiErrorMessage(error, "Không tải được danh sách máy."));
      setMachines([]);
      return [];
    } finally {
      setLoadingMachines(false);
    }
  }, []);

  useEffect(() => {
    fetchMachines();
  }, [fetchMachines]);

  // Trích xuất mã máy từ URL hiện tại
  const urlMachineCode = useMemo(() => {
    return extractMachineCodeFromPath(location.pathname);
  }, [location.pathname]);

  // Khớp máy hiện tại từ URL hoặc fallback về Storage / Máy đầu tiên
  const currentMachine = useMemo(() => {
    if (machines.length === 0) return null;

    if (urlMachineCode) {
      const normalizedUrlCode = urlMachineCode.trim().toLowerCase();
      const matched = machines.find((m) =>
        m.machineCode?.trim().toLowerCase() === normalizedUrlCode ||
        String(m.machineId) === urlMachineCode.trim()
      );
      if (matched) return matched;
    }

    // Nếu không có trên URL hoặc URL không khớp, tìm máy đã lưu
    const savedCode = localStorage.getItem(STORAGE_KEY_LAST_MACHINE);
    if (savedCode) {
      const normalizedSaved = savedCode.trim().toLowerCase();
      const matchedSaved = machines.find((m) =>
        m.machineCode?.trim().toLowerCase() === normalizedSaved ||
        String(m.machineId) === savedCode.trim()
      );
      if (matchedSaved) return matchedSaved;
    }

    return machines[0] ?? null;
  }, [machines, urlMachineCode]);

  const currentMachineCode = currentMachine?.machineCode ?? urlMachineCode ?? null;
  const currentMachineId = currentMachine?.machineId ?? null;

  // Cập nhật localStorage khi có máy hợp lệ
  useEffect(() => {
    if (currentMachine?.machineCode) {
      localStorage.setItem(STORAGE_KEY_LAST_MACHINE, currentMachine.machineCode);
    }
  }, [currentMachine]);

  // Hàm chuyển đổi máy
  const changeMachine = useCallback((targetCodeOrId) => {
    if (!targetCodeOrId) return;

    let targetMachine = null;
    if (typeof targetCodeOrId === "number") {
      targetMachine = machines.find((m) => m.machineId === targetCodeOrId);
    } else {
      const targetStr = String(targetCodeOrId).trim().toLowerCase();
      targetMachine = machines.find((m) =>
        m.machineCode?.trim().toLowerCase() === targetStr ||
        String(m.machineId) === targetStr
      );
    }

    const nextCode = targetMachine?.machineCode || String(targetCodeOrId);
    localStorage.setItem(STORAGE_KEY_LAST_MACHINE, nextCode);

    // Tính toán sub-path hiện tại để điều hướng giữ nguyên trang con
    const currentSubPath = extractSubPathFromPath(location.pathname) || "/files";
    const nextUrl = `/m/${encodeURIComponent(nextCode)}${currentSubPath}${location.search}${location.hash}`;
    navigate(nextUrl);
  }, [location.hash, location.pathname, location.search, machines, navigate]);

  // Tạo URL gắn mã máy
  const getMachinePath = useCallback((subPath = "") => {
    const code = currentMachineCode || localStorage.getItem(STORAGE_KEY_LAST_MACHINE) || "default";
    const cleanSub = subPath.startsWith("/") ? subPath : `/${subPath}`;
    return `/m/${encodeURIComponent(code)}${cleanSub}`;
  }, [currentMachineCode]);

  const contextValue = useMemo(() => ({
    machines,
    loadingMachines,
    currentMachine,
    currentMachineCode,
    currentMachineId,
    changeMachine,
    getMachinePath,
    fetchMachines
  }), [
    machines,
    loadingMachines,
    currentMachine,
    currentMachineCode,
    currentMachineId,
    changeMachine,
    getMachinePath,
    fetchMachines
  ]);

  return (
    <MachineContext.Provider value={contextValue}>
      {children}
    </MachineContext.Provider>
  );
}

export default MachineProvider;
