export const CART_POSITION_COUNT = 4;

export const CART_POSITION_OPTIONS = Array.from(
  { length: CART_POSITION_COUNT },
  (_, index) => index + 1
);

export function normalizePositiveNumber(value) {
  const number = Number(value);
  return Number.isFinite(number) && number > 0 ? number : 0;
}

export function getModelJigType(modelOrOrder) {
  const candidates = [
    modelOrOrder?.jigType,
    modelOrOrder?.robotData?.jigSupplyType,
    modelOrOrder?.line1Data?.jigType
  ];
  return candidates.reduce((resolved, value) => {
    if (resolved) return resolved;
    const jigType = Number(value);
    return Number.isInteger(jigType) && jigType >= 1 && jigType <= 4 ? jigType : 0;
  }, 0);
}

export function getModelInputThickness(modelOrOrder) {
  return [
    modelOrOrder?.inputBlankThickness,
    modelOrOrder?.robotData?.inputBlankThickness
  ].reduce((resolved, value) => resolved || normalizePositiveNumber(value), 0);
}

export function getMachineJigHeight(machine, jigType) {
  if (!machine || !Number.isInteger(jigType) || jigType < 1 || jigType > 4) {
    return 0;
  }

  const directValue = machine[`jig${jigType}HeightMm`];
  const mapValue = machine.jigHeights?.[jigType] ?? machine.jigHeights?.[String(jigType)];
  return normalizePositiveNumber(directValue ?? mapValue);
}

export function getJigCapacity(machine, jigType, inputThickness) {
  const jigHeight = getMachineJigHeight(machine, jigType);
  const thickness = normalizePositiveNumber(inputThickness);
  if (!jigHeight || !thickness) {
    return 0;
  }

  return Math.floor(jigHeight / thickness);
}

function createEmptyPosition(position) {
  return {
    position,
    jigType: 0,
    jigHeightMm: 0,
    inputThickness: 0,
    capacity: 0,
    quantity: 0,
    remaining: 0,
    orderIds: [],
    placements: []
  };
}

function normalizeOrderQuantity(value) {
  const quantity = Number(value);
  return Number.isFinite(quantity) && quantity >= 1 ? Math.floor(quantity) : 0;
}

function createPlacement(order, position, quantity, capacity) {
  return {
    key: `${order.key ?? order.orderId ?? order.modelName ?? "order"}-${position}-${order.orderSequence ?? ""}`,
    orderKey: order.key,
    orderId: order.orderId ?? "",
    modelName: order.modelName ?? "",
    reportModelName: order.reportModelName ?? "",
    orderSequence: order.orderSequence,
    jigType: order.jigType ?? 0,
    cartPositionIndex: position,
    quantity,
    maxQty: capacity
  };
}

/**
 * Allocates logical orders from left to right on the four fixed cart positions.
 * A position is never revisited after the cursor moves forward.
 */
export function allocateCartOrders(orders, machine) {
  const positions = CART_POSITION_OPTIONS.map(createEmptyPosition);
  const computedOrders = [];
  const errors = [];
  let cursor = 0;

  (orders ?? []).forEach((order, orderIndex) => {
    const jigType = Number(order.jigType);
    const inputThickness = normalizePositiveNumber(order.inputThickness);
    const quantity = normalizeOrderQuantity(order.quantity);

    if (!Number.isInteger(jigType) || jigType < 1 || jigType > 4) {
      errors.push(`Order #${orderIndex + 1}: chưa cấu hình Jig loại 1-4.`);
      computedOrders.push({ ...order, placements: [], cartPositionIndex: null, maxQty: 0 });
      return;
    }

    const jigHeightMm = getMachineJigHeight(machine, jigType);
    const capacity = getJigCapacity(machine, jigType, inputThickness);
    if (!jigHeightMm) {
      errors.push(`Order #${orderIndex + 1}: chưa cấu hình chiều cao Jig ${jigType}.`);
    }
    if (!inputThickness) {
      errors.push(`Order #${orderIndex + 1}: độ dày phôi đầu vào phải lớn hơn 0.`);
    }
    if (capacity < 1) {
      errors.push(`Order #${orderIndex + 1}: Jig ${jigType} không chứa được sản phẩm.`);
    }
    if (quantity < 1) {
      computedOrders.push({ ...order, placements: [], cartPositionIndex: null, maxQty: 0 });
      return;
    }

    const placements = [];
    let remainingQuantity = quantity;
    while (remainingQuantity > 0) {
      if (cursor >= CART_POSITION_COUNT) {
        errors.push(`Order #${orderIndex + 1}: vượt quá 4 vị trí của Xe hàng.`);
        break;
      }

      const position = positions[cursor];
      if (position.jigType !== 0 && position.jigType !== jigType) {
        cursor += 1;
        continue;
      }

      if (position.jigType === 0) {
        position.jigType = jigType;
        position.jigHeightMm = jigHeightMm;
        position.inputThickness = inputThickness;
        position.capacity = capacity;
        position.remaining = capacity;
      }

      const placedQuantity = Math.min(remainingQuantity, position.remaining);
      if (placedQuantity <= 0) {
        cursor += 1;
        continue;
      }

      const placement = createPlacement(order, cursor + 1, placedQuantity, position.capacity);
      placements.push(placement);
      position.quantity += placedQuantity;
      position.remaining -= placedQuantity;
      const orderLabel = order.orderId || order.modelName || `#${orderIndex + 1}`;
      if (!position.orderIds.includes(orderLabel)) {
        position.orderIds = [...position.orderIds, orderLabel];
      }
      position.placements.push(placement);
      remainingQuantity -= placedQuantity;

      if (position.remaining <= 0) {
        cursor += 1;
      }
    }

    computedOrders.push({
      ...order,
      jigType,
      inputThickness,
      jigHeightMm,
      maxQty: capacity,
      cartPositionIndex: placements[0]?.cartPositionIndex ?? null,
      placements,
      allocatedQuantity: placements.reduce((sum, item) => sum + item.quantity, 0),
      overflowQuantity: remainingQuantity
    });
  });

  return {
    positions,
    computedOrders,
    errors,
    totalCapacity: positions.reduce((sum, position) => sum + position.capacity, 0),
    totalQuantity: positions.reduce((sum, position) => sum + position.quantity, 0)
  };
}

export function flattenCartPlacements(computedOrders) {
  return (computedOrders ?? []).flatMap((order) => (
    (order.placements ?? []).map((placement) => ({
      ...order,
      ...placement,
      key: placement.key,
      quantity: placement.quantity,
      cartPositionIndex: placement.cartPositionIndex,
      placements: undefined
    }))
  ));
}

export function buildCartPreview(computedOrders, machine) {
  const allocation = allocateCartOrders(computedOrders, machine);
  return allocation.positions.map((position) => ({
    ...position,
    status: position.quantity > 0 ? "HasProduct" : "Empty",
    jigTypeName: position.jigType ? `Jig ${position.jigType}` : "Trống"
  }));
}

export function buildCartPreviewFromPlacements(orders) {
  const positions = CART_POSITION_OPTIONS.map(createEmptyPosition);
  (orders ?? []).forEach((order) => {
    const positionIndex = Number(order.cartPositionIndex);
    if (!Number.isInteger(positionIndex) || positionIndex < 1 || positionIndex > CART_POSITION_COUNT) {
      return;
    }

    const position = positions[positionIndex - 1];
    const quantity = normalizeOrderQuantity(order.quantity);
    const jigType = Number(order.jigType) || 0;
    position.jigType = position.jigType || jigType;
    position.jigHeightMm = normalizePositiveNumber(order.jigHeightMm ?? order.jigHeight);
    position.inputThickness = normalizePositiveNumber(order.inputThickness);
    position.capacity = Number(order.maxQty ?? order.jigCapacity ?? position.capacity) || position.capacity;
    position.quantity += quantity;
    position.remaining = Math.max(0, position.capacity - position.quantity);
    const orderLabel = order.orderId || order.modelName || "-";
    if (!position.orderIds.includes(orderLabel)) position.orderIds.push(orderLabel);
    position.placements.push({
      ...order,
      cartPositionIndex: positionIndex,
      quantity
    });
  });

  return positions.map((position) => ({
    ...position,
    status: position.quantity > 0 ? "HasProduct" : "Empty",
    jigTypeName: position.jigType ? `Jig ${position.jigType}` : "Trống"
  }));
}
