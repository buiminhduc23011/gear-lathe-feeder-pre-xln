import {
  assignOrderTrayIndexes,
  buildOrderQuantityAllocation,
  buildSelectedOrderSlots,
  buildComputedOrders,
  buildGeneratedOrderId,
  getOrderQuantityCap,
  inferShelfLayoutType,
  buildPreviewSlots,
  buildSelectedMachineStagingSlots,
  buildValidationErrors,
  findLayoutInfo,
  formatDateTime,
  formatDuration,
  resolveArticleByModelOrder
} from "./utils";
import { DECLARATION_MODES } from "./constants";

describe("ShelfDeclaration utils", () => {
  it("maps layout 3 with tray 2 large on top and tray 1 small below", () => {
    const layoutInfo = findLayoutInfo(3);

    expect(layoutInfo).toMatchObject({
      tray1: "Nhỏ",
      tray1Rows: 5,
      tray1Cols: 9,
      tray2: "Lớn",
      tray2Rows: 4,
      tray2Cols: 8
    });
  });

  it("infers layout and assigns mixed tray types from model profile tray size", () => {
    const smallThenLarge = [
      { key: 1, modelName: "Small", quantity: 10, traySize: "small" },
      { key: 2, modelName: "Large", quantity: 8, traySize: "large" }
    ];
    const largeThenSmall = [
      { key: 1, modelName: "Large", quantity: 8, traySize: "large" },
      { key: 2, modelName: "Small", quantity: 10, traySize: "small" }
    ];

    expect(inferShelfLayoutType(smallThenLarge)).toBe(3);
    expect(assignOrderTrayIndexes(findLayoutInfo(3), smallThenLarge).map((order) => order.trayIndex)).toEqual([1, 2]);

    expect(inferShelfLayoutType(largeThenSmall)).toBe(4);
    expect(assignOrderTrayIndexes(findLayoutInfo(4), largeThenSmall).map((order) => order.trayIndex)).toEqual([1, 2]);
  });

  it("resolves Article ID from Model Order text without requiring a selectable Article ID", () => {
    const models = [
      { modelName: "AQT365G-26D10", itemType: "Model Order A", trayType: 0 },
      { modelName: "DJ662LG", itemType: "Model Order B", trayType: 1 },
      { modelName: "INACTIVE", itemType: "Model Order C", trayType: 1, isEnabled: false }
    ];

    expect(resolveArticleByModelOrder(models, "Model Order A")).toMatchObject({
      status: "resolved",
      model: { modelName: "AQT365G-26D10" }
    });
    expect(resolveArticleByModelOrder(models, "DJ662")).toMatchObject({
      status: "resolved",
      model: { modelName: "DJ662LG" }
    });
    expect(resolveArticleByModelOrder(models, "Không có")).toMatchObject({
      status: "not_found",
      model: null
    });
    expect(resolveArticleByModelOrder(models, "Model Order C")).toMatchObject({
      status: "resolved",
      model: { modelName: "INACTIVE" }
    });
  });

  it("resolves embedded Model Order text by the longest matching Article ID or alias", () => {
    const models = [
      { modelName: "A", itemType: "Alpha", spare1: "Line A", trayType: 0 },
      { modelName: "AA", itemType: "Double A", spare1: "Line AA", trayType: 0 }
    ];

    ["A1", "Ab", "AC", "caB"].forEach((modelOrder) => {
      expect(resolveArticleByModelOrder(models, modelOrder)).toMatchObject({
        status: "resolved",
        model: { modelName: "A" }
      });
    });

    ["cAAkd", "AAjh"].forEach((modelOrder) => {
      expect(resolveArticleByModelOrder(models, modelOrder)).toMatchObject({
        status: "resolved",
        model: { modelName: "AA" }
      });
    });
  });

  it("returns ambiguous when embedded Model Order matches tie at the longest length", () => {
    const models = [
      { modelName: "AB", trayType: 0 },
      { modelName: "CD", trayType: 0 }
    ];

    const result = resolveArticleByModelOrder(models, "xABCDx");

    expect(result.status).toBe("ambiguous");
    expect(result.matches).toHaveLength(2);
    expect(result.matches.map((model) => model.modelName)).toEqual(["AB", "CD"]);
  });

  it("buildGeneratedOrderId keeps generated order within backend length limit", () => {
    const result = buildGeneratedOrderId("AQT365G-26D10-EXTRA", 0, new Date(2026, 4, 8, 14, 30, 12));

    expect(result).toBe("AQT365G26D10EX-260508143012-01");
    expect(result).toHaveLength(30);
  });

  it("buildComputedOrders starts each order at a new row", () => {
    const layoutInfo = findLayoutInfo(1);
    const orders = assignOrderTrayIndexes(layoutInfo, [
      { key: 1, orderId: "", modelName: "A", quantity: 6, jigType: 0, traySize: "small" },
      { key: 2, orderId: "", modelName: "B", quantity: 4, jigType: 0, traySize: "small" },
      { key: 3, orderId: "", modelName: "C", quantity: 2, jigType: 0, traySize: "small" }
    ]);

    const result = buildComputedOrders(layoutInfo, orders);

    expect(result[0]).toMatchObject({ trayIndex: 1, startPosition: 1, startColumn: 1 });
    expect(result[1]).toMatchObject({ trayIndex: 1, startPosition: 2, startColumn: 1 });
    expect(result[2]).toMatchObject({ trayIndex: 1, startPosition: 3, startColumn: 1 });
  });

  it("buildComputedOrders preserves empty quantity for the input value", () => {
    const layoutInfo = findLayoutInfo(1);
    const orders = assignOrderTrayIndexes(layoutInfo, [
      { key: 1, orderId: "", modelName: "A", quantity: null, traySize: "small" }
    ]);

    const result = buildComputedOrders(layoutInfo, orders);

    expect(result[0].quantity).toBeNull();
    expect(result[0]).toMatchObject({ rowsUsed: 0, startPosition: 1 });
  });

  it("buildComputedOrders moves next order to tray 2 when tray 1 rows are exhausted", () => {
    const layoutInfo = findLayoutInfo(1);
    const orders = assignOrderTrayIndexes(layoutInfo, [
      { key: 1, orderId: "", modelName: "A", quantity: 42, traySize: "small" },
      { key: 2, orderId: "", modelName: "B", quantity: 3, traySize: "small" }
    ]);
    const computedOrders = buildComputedOrders(layoutInfo, orders);

    expect(computedOrders[0]).toMatchObject({ trayIndex: 1, startPosition: 1, endPosition: 5 });
    expect(computedOrders[1]).toMatchObject({ trayIndex: 2, startPosition: 1, startColumn: 1 });
  });

  it("buildPreviewSlots fills selected order from row start with no overlap", () => {
    const layoutInfo = findLayoutInfo(1);
    const orders = assignOrderTrayIndexes(layoutInfo, [
      { key: 1, orderId: "", modelName: "A", quantity: 42, traySize: "small" },
      { key: 2, orderId: "", modelName: "B", reportModelName: "Friendly B", quantity: 5, traySize: "small" }
    ]);
    const computedOrders = buildComputedOrders(layoutInfo, orders);

    const result = buildPreviewSlots(layoutInfo, computedOrders);

    expect(result.errors).toEqual([]);
    expect(result.tray1Slots[0]).toMatchObject({ status: "HasProduct", modelName: "A" });
    expect(result.tray1Slots[41]).toMatchObject({ status: "HasProduct", modelName: "A" });
    expect(result.tray2Slots[0]).toMatchObject({ status: "HasProduct", modelName: "Friendly B" });
  });

  it("getOrderQuantityCap uses previous orders in the same assigned tray", () => {
    const layoutInfo = findLayoutInfo(1);
    const orders = assignOrderTrayIndexes(layoutInfo, [
      { key: 1, quantity: 42, traySize: "small" },
      { key: 2, quantity: 20, traySize: "small" },
      { key: 3, quantity: 15, traySize: "small" }
    ]);

    expect(getOrderQuantityCap(layoutInfo, orders, 0)).toBe(45);
    expect(getOrderQuantityCap(layoutInfo, orders, 1)).toBe(45);
    expect(getOrderQuantityCap(layoutInfo, orders, 2)).toBe(18);
  });

  it("buildOrderQuantityAllocation splits overflow into the next matching tray", () => {
    const layoutInfo = findLayoutInfo(2);
    const orders = [
      { key: 1, quantity: 12, traySize: "large" },
      { key: 2, quantity: null, traySize: "large" }
    ];

    const result = buildOrderQuantityAllocation(layoutInfo, orders, 1, 24);

    expect(result).toMatchObject({
      requestedQty: 24,
      maxAllowed: 48,
      fits: true,
      allocations: [
        { trayIndex: 1, quantity: 16 },
        { trayIndex: 2, quantity: 8 }
      ]
    });
  });

  it("buildOrderQuantityAllocation does not split into a tray with a different type", () => {
    const layoutInfo = findLayoutInfo(4);
    const orders = [
      { key: 1, quantity: 12, traySize: "large" },
      { key: 2, quantity: null, traySize: "large" }
    ];

    const result = buildOrderQuantityAllocation(layoutInfo, orders, 1, 24);

    expect(result).toMatchObject({
      requestedQty: 24,
      maxAllowed: 16,
      fits: false,
      allocations: [
        { trayIndex: 1, quantity: 16 }
      ]
    });
  });

  it("buildSelectedOrderSlots returns slot positions for highlighted order", () => {
    const layoutInfo = findLayoutInfo(1);
    const orders = assignOrderTrayIndexes(layoutInfo, [
      { key: 1, orderId: "A", modelName: "A", quantity: 42, traySize: "small" },
      { key: 2, orderId: "B", modelName: "B", quantity: 5, traySize: "small" }
    ]);
    const computedOrders = buildComputedOrders(layoutInfo, orders);

    const selected = buildSelectedOrderSlots(layoutInfo, computedOrders, 1);

    expect(selected.tray1Positions.size).toBe(0);
    expect(selected.tray2Positions.has(1)).toBe(true);
    expect(selected.tray2Positions.has(5)).toBe(true);
  });

  it("buildSelectedMachineStagingSlots returns slots for the active machine", () => {
    const result = buildSelectedMachineStagingSlots([
      { machineId: 1, stagingSlotIndices: [2, 1] },
      { machineId: 2, stagingSlotIndices: [3, 4] }
    ], 1);

    expect(Array.from(result)).toEqual([1, 2]);
  });

  it("buildValidationErrors returns expected messages for missing fields and invalid staging ownership", () => {
    const errors = buildValidationErrors({
      selectedMachineId: 1,
      shelfLayoutType: null,
      ordersLength: 0,
      computedOrders: [
        { modelName: "", reportModelName: "", quantity: 0, maxQty: 8 }
      ],
      previewErrors: ["Preview lỗi"],
      mode: DECLARATION_MODES.AGV,
      stagingSlotIndex: 3,
      machineSlotIndex: 1,
      selectedMachineStagingSlots: new Set([1, 2]),
      occupiedStagingSlots: new Map([[1, { slotIndex: 1 }]]),
      busyMachineSlots: new Map(),
      knownModelNames: new Set(["model a"])
    });

    expect(errors).toEqual(expect.arrayContaining([
      "Cần ít nhất 1 order.",
      "Order #1 chưa nhập Order.",
      "Order #1 chưa nhập Article ID.",
      "Preview lỗi",
      "Staging slot 3 không thuộc máy đã chọn."
    ]));
  });

  it("buildValidationErrors does not require manual Order when orderInput is disabled", () => {
    const errors = buildValidationErrors({
      selectedMachineId: 1,
      shelfLayoutType: 1,
      ordersLength: 1,
      computedOrders: [
        {
          orderId: "MODELA-260508143012-01",
          modelName: "Model A",
          reportModelName: "Model A",
          orderInput: 0,
          trayIndex: 1,
          traySize: "small",
          quantity: 1,
          maxQty: 8
        }
      ],
      previewErrors: [],
      mode: DECLARATION_MODES.AGV,
      stagingSlotIndex: 1,
      machineSlotIndex: 1,
      selectedMachineStagingSlots: new Set([1, 2]),
      occupiedStagingSlots: new Map(),
      busyMachineSlots: new Map(),
      knownModelNames: new Set(["model a"])
    });

    expect(errors).not.toContain("Order #1 chưa nhập Order.");
  });

  it("buildValidationErrors reports when a machine has no free owned staging slot", () => {
    const errors = buildValidationErrors({
      selectedMachineId: 1,
      shelfLayoutType: 1,
      ordersLength: 1,
      computedOrders: [
        { modelName: "Model A", quantity: 1, maxQty: 8 }
      ],
      previewErrors: [],
      mode: DECLARATION_MODES.AGV,
      stagingSlotIndex: null,
      machineSlotIndex: 1,
      selectedMachineStagingSlots: new Set([1, 2]),
      occupiedStagingSlots: new Map([
        [1, { slotIndex: 1 }],
        [2, { slotIndex: 2 }]
      ]),
      busyMachineSlots: new Map()
    });

    expect(errors).toContain("Máy đang chọn không còn staging slot trống.");
  });

  it("formatters return fallbacks for empty values", () => {
    expect(formatDuration(0)).toBe("—");
    expect(formatDateTime(null)).toBe("—");
    expect(formatDuration(125)).toBe("2m 5s");
    expect(formatDateTime("2026-03-23T12:00:00Z")).not.toBe("—");
  });
});
