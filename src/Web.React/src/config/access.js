export const SHELF_DECLARATION_ROLES = ["Admin", "Technician", "Operator"];
export const PRODUCTION_REPORT_ROLES = ["Admin", "Technician", "Operator"];

export function canAccessShelfDeclaration(role) {
  return SHELF_DECLARATION_ROLES.includes(role);
}

export function canAccessProductionReport(role) {
  return PRODUCTION_REPORT_ROLES.includes(role);
}
