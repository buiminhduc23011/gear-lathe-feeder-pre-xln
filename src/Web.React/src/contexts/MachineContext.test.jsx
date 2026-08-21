import { extractMachineCodeFromPath, extractSubPathFromPath } from "./MachineContext";

describe("MachineContext Path Helpers", () => {
  it("extracts machineCode correctly from path", () => {
    expect(extractMachineCodeFromPath("/m/PGR-01/shelf-declaration")).toBe("PGR-01");
    expect(extractMachineCodeFromPath("/m/PGR-02/production-report")).toBe("PGR-02");
    expect(extractMachineCodeFromPath("/m/1/files")).toBe("1");
    expect(extractMachineCodeFromPath("/files")).toBeNull();
    expect(extractMachineCodeFromPath("/")).toBeNull();
    expect(extractMachineCodeFromPath("")).toBeNull();
  });

  it("extracts subpath correctly after machine prefix", () => {
    expect(extractSubPathFromPath("/m/PGR-01/shelf-declaration")).toBe("/shelf-declaration");
    expect(extractSubPathFromPath("/m/PGR-01/settings/models")).toBe("/settings/models");
    expect(extractSubPathFromPath("/m/PGR-01")).toBe("");
    expect(extractSubPathFromPath("/files")).toBe("/files");
  });
});
