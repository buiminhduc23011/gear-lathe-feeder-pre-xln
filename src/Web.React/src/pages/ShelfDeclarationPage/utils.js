import {
  DECLARATION_MODES,
  NON_TERMINAL_STATUSES,
  SHELF_LAYOUT_TYPES
} from "./constants";

export function formatDateTime(value) {
  if (!value) {
    return "—";
  }

  return new Date(value).toLocaleString("vi-VN", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit"
  });
}

export function formatDuration(seconds) {
  if (!Number.isFinite(seconds) || seconds <= 0) {
    return "—";
  }

  const total = Math.floor(seconds);
  const minutes = Math.floor(total / 60);
  const remainSeconds = total % 60;
  return `${minutes}m ${remainSeconds}s`;
}

export function findLayoutInfo(shelfLayoutType) {
  return SHELF_LAYOUT_TYPES.find((layout) => layout.value === shelfLayoutType) ?? null;
}

export function getOrderDisplayModelName(order) {
  return (order?.reportModelName || order?.modelName || "").trim();
}

export function getModelTraySize(model) {
  return Number(model?.trayType) === 1 ? "large" : "small";
}

export function getModelOrderInput(model) {
  return Number(model?.orderInput ?? 1) === 0 ? 0 : 1;
}

export function isModelProfileActive(model) {
  return Boolean(model) && !model.isDeleted && model.isEnabled !== false;
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
  return [
    model?.modelName,
    model?.itemType,
    model?.spare1,
    model?.spare2
  ]
    .map((value) => (value ?? "").trim())
    .filter(Boolean);
}

function getLongestEmbeddedLookupLength(model, lookup) {
  return getModelOrderLookupValues(model)
    .map(normalizeLookupText)
    .filter((value) => lookup.includes(value))
    .reduce((maxLength, value) => Math.max(maxLength, value.length), 0);
}

export function resolveArticleByModelOrder(models, modelOrder) {
  const lookup = normalizeLookupText(modelOrder);
  if (!lookup) {
    return { status: "empty", model: null };
  }

  const availableModels = (models ?? []).filter((model) => !model?.isDeleted);
  const exactMatches = availableModels.filter((model) => (
    getModelOrderLookupValues(model).some((value) => normalizeLookupText(value) === lookup)
  ));

  if (exactMatches.length === 1) {
    return { status: "resolved", model: exactMatches[0] };
  }

  if (exactMatches.length > 1) {
    return { status: "ambiguous", model: null, matches: exactMatches };
  }

  const prefixMatches = availableModels.filter((model) => (
    getModelOrderLookupValues(model).some((value) => normalizeLookupText(value).startsWith(lookup))
  ));

  if (prefixMatches.length === 1) {
    return { status: "resolved", model: prefixMatches[0] };
  }

  if (prefixMatches.length > 1) {
    return { status: "ambiguous", model: null, matches: prefixMatches };
  }

  const embeddedMatches = availableModels
    .map((model) => ({
      model,
      matchLength: getLongestEmbeddedLookupLength(model, lookup)
    }))
    .filter((match) => match.matchLength > 0);

  if (embeddedMatches.length > 0) {
    const longestMatchLength = Math.max(...embeddedMatches.map((match) => match.matchLength));
    const bestMatches = embeddedMatches
      .filter((match) => match.matchLength === longestMatchLength)
      .map((match) => match.model);

    if (bestMatches.length === 1) {
      return { status: "resolved", model: bestMatches[0] };
    }

    return { status: "ambiguous", model: null, matches: bestMatches };
  }

  return { status: "not_found", model: null };
}

export function getTraySizeByLayout(layoutInfo, trayIndex) {
  if (!layoutInfo || !trayIndex) {
    return null;
  }

  const rows = trayIndex === 2 ? layoutInfo.tray2Rows : layoutInfo.tray1Rows;
  const cols = trayIndex === 2 ? layoutInfo.tray2Cols : layoutInfo.tray1Cols;
  return rows === 4 && cols === 8 ? "large" : "small";
}

function getTrayRowsByIndex(layoutInfo, trayIndex) {
  return trayIndex === 2 ? (layoutInfo?.tray2Rows ?? 0) : (layoutInfo?.tray1Rows ?? 0);
}

function getTrayColsByIndex(layoutInfo, trayIndex) {
  return trayIndex === 2 ? (layoutInfo?.tray2Cols ?? 0) : (layoutInfo?.tray1Cols ?? 0);
}

function getRequiredRows(quantity, cols) {
  if (cols <= 0) {
    return 0;
  }

  return Math.ceil(normalizeQuantity(quantity) / cols);
}

export function inferShelfLayoutType(orders) {
  const sizes = (orders ?? [])
    .map((order) => order.traySize)
    .filter(Boolean);

  if (sizes.length === 0) {
    return null;
  }

  const hasSmall = sizes.includes("small");
  const hasLarge = sizes.includes("large");

  if (hasSmall && !hasLarge) {
    return 1;
  }

  if (hasLarge && !hasSmall) {
    return 2;
  }

  return sizes.indexOf("small") < sizes.indexOf("large") ? 3 : 4;
}

export function assignOrderTrayIndexes(layoutInfo, orders) {
  const usedRowsByTray = new Map([
    [1, 0],
    [2, 0]
  ]);

  return (orders ?? []).map((order) => {
    if (!layoutInfo || !order.traySize) {
      return { ...order, trayIndex: null };
    }

    const matchingTrays = [1, 2].filter((trayIndex) => getTraySizeByLayout(layoutInfo, trayIndex) === order.traySize);

    const selectedTray = matchingTrays.find((trayIndex) => {
      const rows = getTrayRowsByIndex(layoutInfo, trayIndex);
      const cols = getTrayColsByIndex(layoutInfo, trayIndex);
      const requiredRows = getRequiredRows(order.quantity, cols);
      const usedRows = usedRowsByTray.get(trayIndex) ?? 0;
      return usedRows + requiredRows <= rows;
    }) ?? null;

    if (selectedTray) {
      const nextUsedRows = (usedRowsByTray.get(selectedTray) ?? 0)
        + getRequiredRows(order.quantity, getTrayColsByIndex(layoutInfo, selectedTray));
      usedRowsByTray.set(selectedTray, nextUsedRows);
    }

    return {
      ...order,
      trayIndex: selectedTray
    };
  });
}

export function buildOrderQuantityAllocation(layoutInfo, orders, targetIndex, requestedQuantity) {
  const requestedQty = normalizeQuantity(requestedQuantity);
  if (!layoutInfo || !Array.isArray(orders) || targetIndex < 0 || targetIndex >= orders.length || requestedQty < 1) {
    return {
      requestedQty,
      maxAllowed: 0,
      fits: false,
      allocations: []
    };
  }

  const targetOrder = orders[targetIndex];
  if (!targetOrder?.traySize) {
    return {
      requestedQty,
      maxAllowed: 0,
      fits: false,
      allocations: []
    };
  }

  const usedRowsByTray = new Map([
    [1, 0],
    [2, 0]
  ]);

  assignOrderTrayIndexes(layoutInfo, orders.slice(0, targetIndex)).forEach((order) => {
    if (!order.trayIndex) {
      return;
    }

    const cols = getTrayColsByIndex(layoutInfo, order.trayIndex);
    usedRowsByTray.set(
      order.trayIndex,
      (usedRowsByTray.get(order.trayIndex) ?? 0) + getRequiredRows(order.quantity, cols)
    );
  });

  const matchingTrays = [1, 2].filter((trayIndex) => (
    getTraySizeByLayout(layoutInfo, trayIndex) === targetOrder.traySize
  ));

  let remainingQty = requestedQty;
  let maxAllowed = 0;
  const allocations = [];

  matchingTrays.forEach((trayIndex) => {
    const rows = getTrayRowsByIndex(layoutInfo, trayIndex);
    const cols = getTrayColsByIndex(layoutInfo, trayIndex);
    const remainingRows = Math.max(0, rows - (usedRowsByTray.get(trayIndex) ?? 0));
    const trayCapacity = remainingRows * cols;

    maxAllowed += trayCapacity;

    if (remainingQty > 0 && trayCapacity > 0) {
      const quantity = Math.min(remainingQty, trayCapacity);
      allocations.push({ trayIndex, quantity });
      remainingQty -= quantity;
    }
  });

  return {
    requestedQty,
    maxAllowed,
    fits: remainingQty === 0,
    allocations
  };
}

export function createEmptyTray(rows, cols) {
  return Array.from({ length: rows * cols }, (_, index) => ({
    position: index + 1,
    status: "Empty",
    orderId: null,
    modelName: null,
    reportModelName: null
  }));
}

function normalizeQuantity(value) {
  const number = Number(value);
  return Number.isFinite(number) && number >= 1 ? number : 0;
}

function getTray1Capacity(layoutInfo) {
  return (layoutInfo?.tray1Rows ?? 0) * (layoutInfo?.tray1Cols ?? 0);
}

function getOffsetLocation(layoutInfo, offset) {
  const tray1Rows = layoutInfo?.tray1Rows ?? 0;
  const tray1Cols = layoutInfo?.tray1Cols ?? 0;
  const tray2Cols = layoutInfo?.tray2Cols ?? 0;
  const tray1Capacity = getTray1Capacity(layoutInfo);

  if (offset < tray1Capacity) {
    const rowZero = Math.floor(offset / tray1Cols);
    return {
      trayIndex: 1,
      globalRowIndex: rowZero,
      rowInTray: rowZero + 1,
      colInRow: (offset % tray1Cols) + 1,
      position: offset + 1
    };
  }

  const localOffset = offset - tray1Capacity;
  const tray2RowZero = Math.floor(localOffset / tray2Cols);
  return {
    trayIndex: 2,
    globalRowIndex: tray1Rows + tray2RowZero,
    rowInTray: tray2RowZero + 1,
    colInRow: (localOffset % tray2Cols) + 1,
    position: localOffset + 1
  };
}

export function buildPreviewSlots(layoutInfo, orders) {
  if (!layoutInfo) {
    return {
      tray1Slots: [],
      tray2Slots: [],
      errors: []
    };
  }

  const tray1Slots = createEmptyTray(layoutInfo.tray1Rows, layoutInfo.tray1Cols);
  const tray2Slots = createEmptyTray(layoutInfo.tray2Rows, layoutInfo.tray2Cols);
  const errors = [];

  const tray1Capacity = getTray1Capacity(layoutInfo);
  const totalCapacity = getTotalLayoutCapacity(layoutInfo);

  const combinedSlots = [
    ...tray1Slots.map((slot) => ({ trayIndex: 1, slot })),
    ...tray2Slots.map((slot) => ({ trayIndex: 2, slot }))
  ];

  const computeStartOffset = (order) => {
    const startColumn = Number.isFinite(order.startColumn) ? order.startColumn : 1;
    const cols = order.trayIndex === 2 ? layoutInfo.tray2Cols : layoutInfo.tray1Cols;
    const rows = order.trayIndex === 2 ? layoutInfo.tray2Rows : layoutInfo.tray1Rows;

    if (!Number.isFinite(order.startPosition) || order.startPosition < 1 || order.startPosition > rows) {
      return -1;
    }

    if (!Number.isFinite(startColumn) || startColumn < 1 || startColumn > cols) {
      return -1;
    }

    const localOffset = ((order.startPosition - 1) * cols) + (startColumn - 1);
    if (order.trayIndex === 2) {
      return tray1Capacity + localOffset;
    }

    return localOffset;
  };

  orders.forEach((order, index) => {
    const startOffset = computeStartOffset(order);
    if (startOffset < 0 || startOffset >= totalCapacity) {
      errors.push(`Order #${index + 1} có vị trí bắt đầu không hợp lệ.`);
      return;
    }

    for (let itemIndex = 0; itemIndex < order.quantity; itemIndex += 1) {
      const targetOffset = startOffset + itemIndex;
      const target = combinedSlots[targetOffset];

      if (!target) {
        errors.push(`Order #${index + 1} bị tràn khỏi preview.`);
        continue;
      }

      if (target.slot.status === "HasProduct") {
        errors.push(`Order #${index + 1} bị chồng lấp với order khác.`);
        continue;
      }

      target.slot.status = "HasProduct";
      target.slot.orderId = order.orderId || null;
      target.slot.modelName = getOrderDisplayModelName(order);
      target.slot.reportModelName = order.reportModelName ?? null;
    }
  });

  return {
    tray1Slots,
    tray2Slots,
    errors
  };
}

export function buildComputedOrders(layoutInfo, orders) {
  if (!layoutInfo) {
    return orders.map((order, index) => ({
      ...order,
      orderSequence: index + 1,
      trayIndex: order.trayIndex || 1,
      startPosition: index + 1,
      maxQty: 0,
      trayTypeName: "—",
      rowsUsed: 0
    }));
  }

  const tray1Cols = layoutInfo.tray1Cols ?? 0;
  const tray1Rows = layoutInfo.tray1Rows ?? 0;
  const tray2Cols = layoutInfo.tray2Cols ?? 0;
  const tray2Rows = layoutInfo.tray2Rows ?? 0;
  const totalCapacityTray1 = tray1Rows * tray1Cols;

  let cursorRowTray1 = 0;
  let cursorRowTray2 = 0;

  return orders.map((order, index) => {
    const requestedQty = normalizeQuantity(order.quantity);
    const trayIndex = order.trayIndex;
    
    if (!trayIndex) {
      return {
        ...order,
        orderSequence: index + 1,
        trayIndex: null,
        startPosition: 0,
        startColumn: 0,
        endTrayIndex: null,
        endPosition: 0,
        startOffset: -1,
        maxQty: 0,
        trayTypeName: "—",
        rowsUsed: 0
      };
    }

    let cursorRow = trayIndex === 1 ? cursorRowTray1 : cursorRowTray2;
    const trayRows = trayIndex === 1 ? tray1Rows : tray2Rows;
    const trayCols = trayIndex === 1 ? tray1Cols : tray2Cols;

    const remainingRows = Math.max(0, trayRows - cursorRow);
    const maxQty = remainingRows * trayCols;

    const placedQty = Math.min(requestedQty, maxQty);
    
    let rowsUsed = 0;
    if (placedQty > 0) {
      let remain = placedQty;
      while (remain > 0) {
        remain -= trayCols;
        rowsUsed += 1;
      }
    }

    const startLocalOffset = cursorRow * trayCols;
    const startOffsetGlobal = trayIndex === 1 ? startLocalOffset : (totalCapacityTray1 + startLocalOffset);
    const endRow = cursorRow + Math.max(0, rowsUsed - 1);

    if (trayIndex === 1) {
      cursorRowTray1 += rowsUsed;
    } else {
      cursorRowTray2 += rowsUsed;
    }

    return {
      ...order,
      orderSequence: index + 1,
      trayIndex,
      startPosition: cursorRow + 1,
      startColumn: 1,
      endTrayIndex: trayIndex,
      endPosition: endRow + 1,
      startOffset: startOffsetGlobal,
      maxQty,
      trayTypeName: trayIndex === 1 ? layoutInfo.tray1 : layoutInfo.tray2,
      rowsUsed
    };
  });
}

export function getTotalLayoutCapacity(layoutInfo) {
  if (!layoutInfo) {
    return 0;
  }

  return (layoutInfo.tray1Rows * layoutInfo.tray1Cols) + (layoutInfo.tray2Rows * layoutInfo.tray2Cols);
}

export function getOrderQuantityCap(layoutInfo, orders, targetIndex) {
  if (!layoutInfo || !Array.isArray(orders) || targetIndex < 0 || targetIndex >= orders.length) {
    return 1;
  }

  const targetOrder = orders[targetIndex];
  const trayIndex = targetOrder.trayIndex;
  if (!trayIndex) return 0;

  const trayRows = trayIndex === 1 ? (layoutInfo.tray1Rows ?? 0) : (layoutInfo.tray2Rows ?? 0);
  const trayCols = trayIndex === 1 ? (layoutInfo.tray1Cols ?? 0) : (layoutInfo.tray2Cols ?? 0);

  let cursorRow = 0;
  for (let i = 0; i < targetIndex; i++) {
    const order = orders[i];
    if ((order.trayIndex || 1) === trayIndex) {
       const qty = normalizeQuantity(order.quantity);
       let rowsUsed = 0;
       let remain = qty;
       while (remain > 0) {
          remain -= trayCols;
          rowsUsed += 1;
       }
       cursorRow += rowsUsed;
    }
  }

  const remainingRows = Math.max(0, trayRows - cursorRow);
  const maxPossible = remainingRows * trayCols;
  return maxPossible > 0 ? maxPossible : 0;
}

export function buildSelectedOrderSlots(layoutInfo, computedOrders, selectedOrderIndex) {
  const empty = {
    tray1Positions: new Set(),
    tray2Positions: new Set()
  };

  if (!layoutInfo || !Array.isArray(computedOrders) || !Number.isInteger(selectedOrderIndex)) {
    return empty;
  }

  const selectedOrder = computedOrders[selectedOrderIndex];
  if (!selectedOrder || !Number.isFinite(selectedOrder.startOffset)) {
    return empty;
  }

  const totalCapacity = getTotalLayoutCapacity(layoutInfo);

  for (let itemIndex = 0; itemIndex < normalizeQuantity(selectedOrder.quantity); itemIndex += 1) {
    const offset = selectedOrder.startOffset + itemIndex;
    if (offset >= totalCapacity) {
      break;
    }

    const location = getOffsetLocation(layoutInfo, offset);
    if (!location) {
      continue;
    }

    if (location.trayIndex === 1) {
      empty.tray1Positions.add(location.position);
    } else {
      empty.tray2Positions.add(location.position);
    }
  }

  return empty;
}

export function parseOrdersJson(ordersJson) {
  if (!ordersJson) {
    return [];
  }

  try {
    const parsed = JSON.parse(ordersJson);
    return Array.isArray(parsed) ? parsed : [];
  } catch {
    return [];
  }
}

export function buildPersistedComputedOrders(layoutInfo, persistedOrders) {
  return (persistedOrders ?? [])
    .filter((order) => Number.isFinite(order?.orderSequence))
    .sort((left, right) => left.orderSequence - right.orderSequence)
    .map((order, index) => ({
      key: `persisted-${order.orderSequence ?? index + 1}-${order.orderId ?? index}`,
      orderId: order.orderId ?? "",
      modelName: order.modelName ?? "",
      reportModelName: order.reportModelName ?? "",
      quantity: order.quantity ?? 0,
      jigType: order.jigType ?? 0,
      orderSequence: order.orderSequence ?? index + 1,
      trayIndex: order.trayIndex ?? 1,
      startPosition: order.startPosition ?? 1,
      maxQty: order.trayIndex === 2 ? (layoutInfo?.tray2Cols ?? 0) : (layoutInfo?.tray1Cols ?? 0),
      trayTypeName: order.trayIndex === 2 ? (layoutInfo?.tray2 ?? "—") : (layoutInfo?.tray1 ?? "—"),
      startColumn: 1,
      endTrayIndex: order.trayIndex ?? 1,
      endPosition: order.startPosition ?? 1,
      startOffset: Number.isFinite(order.startPosition)
        ? ((order.trayIndex === 2 ? getTray1Capacity(layoutInfo) : 0)
          + ((Math.max(1, order.startPosition) - 1) * (order.trayIndex === 2 ? (layoutInfo?.tray2Cols ?? 0) : (layoutInfo?.tray1Cols ?? 0))))
        : 0
    }));
}

export function buildMachineOptions(machines) {
  return machines.map((machine) => ({
    value: machine.machineId,
    label: `${machine.machineName} (${machine.machineCode})`
  }));
}

export function buildModelOptions(models) {
  return models.map((model) => ({
    value: model.modelName,
    label: `${model.modelName} (${getModelTraySize(model) === "large" ? "Tray to" : "Tray nhỏ"})`,
    trayType: model.trayType,
    orderInput: getModelOrderInput(model)
  }));
}

export function buildOccupiedStagingSlots(slotStatuses) {
  const map = new Map();

  slotStatuses.forEach((slot) => {
    if (slot.isOccupied) {
      map.set(slot.slotIndex, slot);
    }
  });

  return map;
}

export function buildBusyMachineSlots(history) {
  const map = new Map();

  history
    .filter((item) => item.mode === DECLARATION_MODES.MANUAL && NON_TERMINAL_STATUSES.includes(item.status))
    .forEach((item) => {
      if (item.machineSlotIndex) {
        map.set(item.machineSlotIndex, item);
      }
    });

  return map;
}

export function buildSelectedMachineStagingSlots(machines, selectedMachineId) {
  const selectedMachine = machines.find((machine) => machine.machineId === selectedMachineId);
  return new Set(
    (selectedMachine?.stagingSlotIndices ?? [])
      .filter((slot) => Number.isFinite(slot))
      .sort((left, right) => left - right)
  );
}

export function buildValidationErrors({
  selectedMachineId,
  shelfLayoutType,
  ordersLength,
  computedOrders,
  previewErrors,
  mode,
  stagingSlotIndex,
  machineSlotIndex,
  selectedMachineStagingSlots,
  occupiedStagingSlots,
  busyMachineSlots,
  knownModelNames
}) {
  const errors = [];

  if (!selectedMachineId) {
    errors.push("Vui lòng chọn máy.");
  }

  if (ordersLength === 0) {
    errors.push("Cần ít nhất 1 order.");
  }

  computedOrders.forEach((order, index) => {
    if (getModelOrderInput(order) === 1 && !order.orderId?.trim()) {
      errors.push(`Order #${index + 1} chưa nhập Order.`);
    } else if (getModelOrderInput(order) === 0 && !order.orderId?.trim()) {
      errors.push(`Order #${index + 1} chưa sinh Order tự động.`);
    }

    if (!order.reportModelName?.trim()) {
      errors.push(`Order #${index + 1} chưa nhập Article ID.`);
    } else if (!order.modelName?.trim()) {
      errors.push(`Order #${index + 1} không tìm thấy ID theo Article ID.`);
    } else if (knownModelNames && !knownModelNames.has(order.modelName.trim().toLowerCase())) {
      errors.push(`Order #${index + 1} ID không tồn tại trong cấu hình model.`);
    }

    if (!order.trayIndex) {
      errors.push(
        order.traySize
          ? `Order #${index + 1} khay phù hợp đã đầy hoặc không tồn tại.`
          : `Order #${index + 1} chưa xác định loại khay.`
      );
    } else if (!Number.isFinite(order.quantity) || order.quantity < 1) {
      errors.push(`Order #${index + 1} có số lượng không hợp lệ.`);
    } else if (order.quantity > order.maxQty) {
      errors.push(`Order #${index + 1} vượt quá sức chứa còn lại.`);
    }
  });

  if (!shelfLayoutType && ordersLength > 0 && computedOrders.every((order) => order.modelName?.trim())) {
    errors.push("Chưa xác định được loại kệ từ Article ID.");
  }

  errors.push(...previewErrors);

  if (mode === DECLARATION_MODES.AGV) {
    const ownedSlots = Array.from(selectedMachineStagingSlots ?? []);
    const hasAvailableOwnedSlot = ownedSlots.some((slot) => !occupiedStagingSlots.has(slot));

    if (ownedSlots.length === 0) {
      errors.push("Máy đang chọn chưa được gán staging slot.");
    } else if (!hasAvailableOwnedSlot) {
      errors.push("Máy đang chọn không còn staging slot trống.");
    } else if (!Number.isFinite(stagingSlotIndex)) {
      errors.push("Vui lòng chọn staging slot.");
    } else if (!selectedMachineStagingSlots.has(stagingSlotIndex)) {
      errors.push(`Staging slot ${stagingSlotIndex} không thuộc máy đã chọn.`);
    } else if (occupiedStagingSlots.has(stagingSlotIndex)) {
      errors.push(`Staging slot ${stagingSlotIndex} đã có khai báo.`);
    }
  }

  if (mode === DECLARATION_MODES.MANUAL && busyMachineSlots.has(machineSlotIndex)) {
    errors.push(`Machine slot ${machineSlotIndex} đang có khai báo hoạt động.`);
  }

  return Array.from(new Set(errors));
}

export function getFirstAvailableSlot(options, occupiedLookup, fallback = options[0]) {
  return options.find((slot) => !occupiedLookup.has(slot)) ?? fallback;
}
