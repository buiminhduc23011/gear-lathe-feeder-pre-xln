import {
  buildReservedSlotMap,
  normalizeStagingSlotIndices
} from "./MachineSettings";

describe("MachineSettings helpers", () => {
  it("normalizeStagingSlotIndices sorts, de-duplicates, and filters invalid values", () => {
    expect(normalizeStagingSlotIndices([4, 2, 2, "3", null, 1])).toEqual([1, 2, 3, 4]);
  });

  it("buildReservedSlotMap excludes the machine currently being edited", () => {
    const reserved = buildReservedSlotMap([
      {
        machineId: 1,
        machineName: "Machine A",
        stagingSlotIndices: [1, 2]
      },
      {
        machineId: 2,
        machineName: "Machine B",
        stagingSlotIndices: [3, 4]
      }
    ], 2);

    expect(reserved.get(1)).toBe("Machine A");
    expect(reserved.get(2)).toBe("Machine A");
    expect(reserved.has(3)).toBe(false);
    expect(reserved.has(4)).toBe(false);
  });
});
