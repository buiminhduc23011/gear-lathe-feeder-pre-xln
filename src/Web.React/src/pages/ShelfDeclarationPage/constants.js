export const DECLARATION_MODES = {
  AGV: "Agv",
  MANUAL: "ManualLoad"
};

export const NON_TERMINAL_STATUSES = ["Created", "AgvTaken", "Loaded", "InProduction"];

export const STAGING_SLOT_OPTIONS = [1, 2, 3, 4];

export const MACHINE_SLOT_OPTIONS = [1, 2];

export const JIG_TYPE_OPTIONS = [
  { value: 0, label: "0: Không xác định" },
  { value: 1, label: "1: Tay kẹp nhỏ" },
  { value: 2, label: "2: Tay kẹp to rộng 12mm" },
  { value: 3, label: "3: Tay kẹp to rộng 25mm" }
];

export const SHELF_LAYOUT_TYPES = [
  { value: 1, label: "Loại 1: 2 Tray Nhỏ", tray1: "Nhỏ", tray2: "Nhỏ", tray1Rows: 5, tray1Cols: 9, tray2Rows: 5, tray2Cols: 9 },
  { value: 2, label: "Loại 2: 2 Tray Lớn", tray1: "Lớn", tray2: "Lớn", tray1Rows: 4, tray1Cols: 8, tray2Rows: 4, tray2Cols: 8 },
  { value: 3, label: "Loại 3: Nhỏ dưới + Lớn trên", tray1: "Nhỏ", tray2: "Lớn", tray1Rows: 5, tray1Cols: 9, tray2Rows: 4, tray2Cols: 8 },
  { value: 4, label: "Loại 4: Lớn dưới + Nhỏ trên", tray1: "Lớn", tray2: "Nhỏ", tray1Rows: 4, tray1Cols: 8, tray2Rows: 5, tray2Cols: 9 }
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
