export const DECLARATION_MODES = {
  AGV: "Agv",
  MANUAL: "ManualLoad"
};

export const NON_TERMINAL_STATUSES = ["Created", "AgvTaken", "Loaded", "InProduction"];

export const STAGING_SLOT_OPTIONS = [1, 2, 3, 4];

export const MACHINE_SLOT_OPTIONS = [1];

export const JIG_TYPE_OPTIONS = [
  { value: 0, label: "0: Không xác định" },
  { value: 1, label: "1: Jig Phi 20" },
  { value: 2, label: "2: Jig Phi 30" },
  { value: 3, label: "3: Jig Phi 40" },
  { value: 4, label: "4: Jig có thể điều chỉnh" }
];

export const STATUS_LABELS = {
  Created: "Đã tạo",
  AgvTaken: "AGV đã lấy",
  Loaded: "Đã load",
  InProduction: "Đang sản xuất",
  Completed: "Hoàn thành",
  Cleared: "Đã clear",
  Cancelled: "Đã hủy"
};

export const STATUS_COLORS = {
  Created: "processing",
  AgvTaken: "cyan",
  Loaded: "blue",
  InProduction: "gold",
  Completed: "success",
  Cleared: "default",
  Cancelled: "default"
};
