import {
  canAccessProductionReport,
  canAccessShelfDeclaration,
  PRODUCTION_REPORT_ROLES,
  SHELF_DECLARATION_ROLES
} from "./access";

describe("shelf declaration access", () => {
  it.each(["Admin", "Technician", "Operator"])("allows %s", (role) => {
    expect(canAccessShelfDeclaration(role)).toBe(true);
    expect(SHELF_DECLARATION_ROLES).toContain(role);
  });

  it("does not allow Viewer", () => {
    expect(canAccessShelfDeclaration("Viewer")).toBe(false);
  });
});

describe("production report access", () => {
  it.each(["Admin", "Technician", "Operator"])("allows %s", (role) => {
    expect(canAccessProductionReport(role)).toBe(true);
    expect(PRODUCTION_REPORT_ROLES).toContain(role);
  });

  it("does not allow Viewer", () => {
    expect(canAccessProductionReport("Viewer")).toBe(false);
  });
});
