import { DECLARATION_MODES, NON_TERMINAL_STATUSES } from "./constants";

export function formatDateTime(value) {
  if (!value) return "—";

  return new Date(value).toLocaleString("vi-VN", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit"
  });
}

export function formatDuration(seconds) {
  if (!Number.isFinite(seconds) || seconds <= 0) return "—";

  const total = Math.floor(seconds);
  return `${Math.floor(total / 60)}m ${total % 60}s`;
}

function padDatePart(value) {
  return String(value).padStart(2, "0");
}

function buildTimestamp(value) {
  const date = value instanceof Date ? value : new Date(value);
  return [
    padDatePart(date.getFullYear() % 100),
    padDatePart(date.getMonth() + 1),
    padDatePart(date.getDate()),
    padDatePart(date.getHours()),
    padDatePart(date.getMinutes()),
    padDatePart(date.getSeconds())
  ].join("");
}

export function buildGeneratedOrderId(articleId, rowIndex = 0, value = new Date()) {
  const normalizedArticle = (articleId ?? "")
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .replace(/[^A-Za-z0-9]/g, "")
    .toUpperCase();
  const prefix = (normalizedArticle || "ARTICLE").slice(0, 14);
  const timestamp = buildTimestamp(value);
  const rowNo = String((Number.isFinite(rowIndex) ? rowIndex : 0) + 1).padStart(2, "0").slice(-2);
  return `${prefix}-${timestamp}-${rowNo}`;
}

function normalizeLookupText(value) {
  return (value ?? "").trim().toLowerCase();
}

function getModelOrderLookupValues(model) {
  return [model?.modelName, model?.itemType, model?.spare1, model?.spare2]
    .map((value) => (value ?? "").trim())
    .filter(Boolean);
}

export function resolveArticleByModelOrder(models, modelOrder) {
  const lookup = normalizeLookupText(modelOrder);
  if (!lookup) return { status: "empty", model: null };

  const availableModels = (models ?? []).filter((model) => !model?.isDeleted);
  const matches = (predicate) => availableModels.filter((model) => (
    getModelOrderLookupValues(model).some((value) => predicate(normalizeLookupText(value)))
  ));

  const exactMatches = matches((value) => value === lookup);
  if (exactMatches.length === 1) return { status: "resolved", model: exactMatches[0] };
  if (exactMatches.length > 1) return { status: "ambiguous", model: null, matches: exactMatches };

  const prefixMatches = matches((value) => value.startsWith(lookup));
  if (prefixMatches.length === 1) return { status: "resolved", model: prefixMatches[0] };
  if (prefixMatches.length > 1) return { status: "ambiguous", model: null, matches: prefixMatches };

  const embeddedMatches = availableModels
    .map((model) => {
      const matchLength = getModelOrderLookupValues(model)
        .map(normalizeLookupText)
        .filter((value) => lookup.includes(value))
        .reduce((maxLength, value) => Math.max(maxLength, value.length), 0);
      return { model, matchLength };
    })
    .filter((match) => match.matchLength > 0);

  if (embeddedMatches.length === 0) return { status: "not_found", model: null };
  const longestMatchLength = Math.max(...embeddedMatches.map((match) => match.matchLength));
  const bestMatches = embeddedMatches
    .filter((match) => match.matchLength === longestMatchLength)
    .map((match) => match.model);

  return bestMatches.length === 1
    ? { status: "resolved", model: bestMatches[0] }
    : { status: "ambiguous", model: null, matches: bestMatches };
}

export function parseOrdersJson(ordersJson) {
  if (!ordersJson) return [];

  try {
    const parsed = JSON.parse(ordersJson);
    return Array.isArray(parsed) ? parsed : [];
  } catch {
    return [];
  }
}

export function buildMachineOptions(machines) {
  return (machines ?? []).map((machine) => ({
    value: machine.machineId,
    label: `${machine.machineName} (${machine.machineCode})`
  }));
}

export function buildOccupiedStagingSlots(slotStatuses) {
  const map = new Map();
  (slotStatuses ?? []).forEach((slot) => {
    if (slot.isOccupied) map.set(slot.slotIndex, slot);
  });
  return map;
}

export function buildBusyMachineSlots(history) {
  const map = new Map();
  (history ?? [])
    .filter((item) => item.mode === DECLARATION_MODES.MANUAL && NON_TERMINAL_STATUSES.includes(item.status))
    .forEach((item) => {
      if (item.machineSlotIndex) map.set(item.machineSlotIndex, item);
    });
  return map;
}

export function buildSelectedMachineStagingSlots(machines, selectedMachineId) {
  const selectedMachine = (machines ?? []).find((machine) => machine.machineId === selectedMachineId);
  return new Set(
    (selectedMachine?.stagingSlotIndices ?? [])
      .map(Number)
      .filter((slot) => Number.isFinite(slot) && slot >= 1 && slot <= 4)
      .sort((left, right) => left - right)
      .slice(0, 1)
  );
}

export function getFirstAvailableSlot(options, occupiedLookup, fallback = options[0]) {
  return (options ?? []).find((slot) => !occupiedLookup.has(slot)) ?? fallback;
}
