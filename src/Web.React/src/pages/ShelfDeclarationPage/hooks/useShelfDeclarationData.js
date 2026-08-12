import { useCallback, useEffect, useState } from "react";
import { API_ENDPOINTS, apiClient, getApiErrorMessage } from "../../../config/api";
import { showErrorMessage, showSuccessMessage } from "../../../utils/appMessage";

function normalizeArrayResponse(response) {
  return Array.isArray(response?.data) ? response.data : [];
}

export default function useShelfDeclarationData() {
  const [machines, setMachines] = useState([]);
  const [selectedMachineId, setSelectedMachineId] = useState(null);
  const [models, setModels] = useState([]);
  const [history, setHistory] = useState([]);
  const [slotStatuses, setSlotStatuses] = useState([]);
  const [loadingHistory, setLoadingHistory] = useState(false);
  const [loadingSlots, setLoadingSlots] = useState(false);
  const [submitting, setSubmitting] = useState(false);

  const loadMachines = useCallback(async () => {
    try {
      const response = await apiClient.get(API_ENDPOINTS.machines);
      const list = Array.isArray(response.data) ? response.data : response.data?.data ?? [];
      setMachines(list);
      setSelectedMachineId((current) => (
        list.some((machine) => machine.machineId === current)
          ? current
          : list[0]?.machineId ?? null
      ));
    } catch {
      setMachines([]);
      setSelectedMachineId(null);
    }
  }, []);

  const loadModels = useCallback(async (machineId) => {
    if (!machineId) {
      setModels([]);
      return;
    }

    try {
      const response = await apiClient.get(API_ENDPOINTS.machineModels(machineId));
      const list = Array.isArray(response.data) ? response.data : [];
      setModels(list.filter((item) => !item.isDeleted));
    } catch {
      setModels([]);
    }
  }, []);

  const loadHistory = useCallback(async (machineId) => {
    if (!machineId) {
      setHistory([]);
      return;
    }

    setLoadingHistory(true);
    try {
      const response = await apiClient.get(API_ENDPOINTS.shelfDeclarations(machineId));
      setHistory(normalizeArrayResponse(response));
    } catch {
      setHistory([]);
    } finally {
      setLoadingHistory(false);
    }
  }, []);

  const loadSlotStatuses = useCallback(async (machineId) => {
    if (!machineId) {
      setSlotStatuses([]);
      return;
    }

    setLoadingSlots(true);
    try {
      const response = await apiClient.get(API_ENDPOINTS.shelfDeclarationSlotStatus(machineId));
      setSlotStatuses(normalizeArrayResponse(response));
    } catch {
      setSlotStatuses([]);
    } finally {
      setLoadingSlots(false);
    }
  }, []);

  const refreshAll = useCallback(async (machineId = selectedMachineId) => {
    if (!machineId) {
      return;
    }

    await Promise.all([
      loadHistory(machineId),
      loadSlotStatuses(machineId)
    ]);
  }, [loadHistory, loadSlotStatuses, selectedMachineId]);

  const submitDeclaration = useCallback(async (payload, machineId = selectedMachineId) => {
    setSubmitting(true);

    try {
      await apiClient.post(API_ENDPOINTS.shelfDeclarations(machineId), payload);
      showSuccessMessage("Đã tạo khai báo kệ thành công.");
      await refreshAll(machineId);
      return true;
    } catch (error) {
      showErrorMessage(getApiErrorMessage(error, "Không thể tạo khai báo."));
      return false;
    } finally {
      setSubmitting(false);
    }
  }, [refreshAll, selectedMachineId]);

  const cancelDeclaration = useCallback(async (id, machineId = selectedMachineId) => {
    try {
      await apiClient.delete(`/api/shelf-declarations/${id}`);
      showSuccessMessage("Đã hủy khai báo.");
      await refreshAll(machineId);
    } catch (error) {
      showErrorMessage(getApiErrorMessage(error, "Không thể hủy khai báo."));
    }
  }, [refreshAll, selectedMachineId]);

  const handleMachineChange = useCallback((machineId) => {
    setSelectedMachineId(machineId);
  }, []);

  useEffect(() => {
    loadMachines();
  }, [loadMachines]);

  useEffect(() => {
    if (!selectedMachineId) {
      setModels([]);
      setHistory([]);
      setSlotStatuses([]);
      return;
    }

    loadModels(selectedMachineId);
    refreshAll(selectedMachineId);
  }, [loadModels, refreshAll, selectedMachineId]);

  return {
    machines,
    selectedMachineId,
    models,
    history,
    slotStatuses,
    loadingHistory,
    loadingSlots,
    submitting,
    handleMachineChange,
    refreshAll,
    submitDeclaration,
    cancelDeclaration
  };
}
