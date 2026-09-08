import {
  buildBusyMachineSlots,
  buildGeneratedOrderId,
  buildSelectedMachineStagingSlots,
  formatDateTime,
  formatDuration,
  resolveArticleByModelOrder
} from "./utils";
import { DECLARATION_MODES } from "./constants";

describe("shelf declaration utilities", () => {
  it("formats history values", () => {
    expect(formatDuration(0)).toBe("—");
    expect(formatDuration(125)).toBe("2m 5s");
    expect(formatDateTime(null)).toBe("—");
    expect(formatDateTime("2026-03-23T12:00:00Z")).not.toBe("—");
  });

  it("generates a stable automatic order id", () => {
    expect(buildGeneratedOrderId("Mẫu A", 0, new Date(2026, 2, 23, 12, 34, 56)))
      .toBe("MAUA-260323123456-01");
  });

  it("resolves model order by exact, prefix and ambiguous matches", () => {
    const models = [
      { modelName: "A", itemType: "Alpha" },
      { modelName: "AB", itemType: "Beta" }
    ];

    expect(resolveArticleByModelOrder(models, "A").status).toBe("resolved");
    expect(resolveArticleByModelOrder(models, "Al").model.modelName).toBe("A");
    expect(resolveArticleByModelOrder(models, "not-found").status).toBe("not_found");
  });

  it("builds machine slot lookups for active declarations", () => {
    const history = [{ mode: DECLARATION_MODES.MANUAL, status: "Created", machineSlotIndex: 1 }];
    expect(buildBusyMachineSlots(history).has(1)).toBe(true);
    expect(buildSelectedMachineStagingSlots([{ machineId: 4, stagingSlotIndices: [3] }], 4).has(3)).toBe(true);
  });
});
