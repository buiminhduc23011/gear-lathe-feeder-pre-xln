import React from "react";
import { Space, Typography } from "antd";
import { DECLARATION_MODES } from "../constants";
import TrayGridPreview from "./TrayGridPreview";

const { Text } = Typography;

const TRAY_DISPLAY_ORDER = [
  { index: 2, layoutKey: "tray2", previewKey: "tray2Slots" },
  { index: 1, layoutKey: "tray1", previewKey: "tray1Slots" }
];

function getTraySize(layoutInfo, layoutKey) {
  const rows = layoutInfo?.[`${layoutKey}Rows`] ?? 0;
  const cols = layoutInfo?.[`${layoutKey}Cols`] ?? 0;
  return rows === 4 && cols === 8 ? "large" : "small";
}

function LegendDot({ color, border }) {
  return (
    <span
      style={{
        width: 10,
        height: 10,
        borderRadius: 999,
        background: color,
        border: border ?? "1px solid rgba(15,23,42,0.08)",
        display: "inline-block"
      }}
    />
  );
}

function TrayPreviewPanel({ mode, machineSlotIndex, busyMachineSlots, layoutInfo, preview, selectedOrderSlots }) {
  const selectedTray1 = selectedOrderSlots?.tray1Positions ?? new Set();
  const selectedTray2 = selectedOrderSlots?.tray2Positions ?? new Set();

  return (
    <Space direction="vertical" size={12} style={{ display: "flex" }}>
      {mode !== DECLARATION_MODES.AGV ? (
        <div
          style={{
            background: "#fcfcfd",
            border: "1px solid #eef2f7",
            borderRadius: 14,
            padding: 14
          }}
        >
          <Space direction="vertical" size={6} style={{ width: "100%" }}>
            <Text strong>Thông tin load tay</Text>
            <Text type="secondary">Machine slot {machineSlotIndex}</Text>
            <Text type="secondary">
              {busyMachineSlots.has(machineSlotIndex)
                ? "Slot này đang có khai báo hoạt động."
                : "Slot này đang trống và có thể gửi yêu cầu load."}
            </Text>
          </Space>
        </div>
      ) : null}

      <div
        style={{
          background: "#fcfcfd",
          border: "1px solid #eef2f7",
          borderRadius: 14,
          padding: 14
        }}
      >
        <div style={{ marginBottom: 10 }}>
          <Text strong>Trực quan hóa</Text>

          <div style={{ display: "flex", flexWrap: "wrap", gap: "4px 10px", marginTop: 8, maxWidth: "100%" }}>
            <span style={{ display: "inline-flex", alignItems: "center", gap: 5, flex: "0 0 auto" }}>
              <LegendDot color="#2563eb" />
              <Text type="secondary" style={{ fontSize: 12 }}>Tray nhỏ</Text>
            </span>
            <span style={{ display: "inline-flex", alignItems: "center", gap: 5, flex: "0 0 auto" }}>
              <LegendDot color="#f97316" />
              <Text type="secondary" style={{ fontSize: 12 }}>Tray to</Text>
            </span>
            <span style={{ display: "inline-flex", alignItems: "center", gap: 5, flex: "0 0 auto" }}>
              <LegendDot color="#e2e8f0" />
              <Text type="secondary" style={{ fontSize: 12 }}>Trống</Text>
            </span>
            <span style={{ display: "inline-flex", alignItems: "center", gap: 5, flex: "0 0 auto" }}>
              <LegendDot color="#ffffff" border="2px solid #a21caf" />
              <Text type="secondary" style={{ fontSize: 12 }}>Order đang chọn</Text>
            </span>
          </div>
        </div>

        <Space direction="vertical" size={10} style={{ display: "flex" }}>
          {TRAY_DISPLAY_ORDER.map(({ index, layoutKey, previewKey }) => (
            <TrayGridPreview
              key={index}
              trayLabel={`TRAY ${index} · Tray ${layoutInfo?.[layoutKey] ?? "—"} (${layoutInfo?.[`${layoutKey}Rows`] ?? 0}x${layoutInfo?.[`${layoutKey}Cols`] ?? 0})`}
              rows={layoutInfo?.[`${layoutKey}Rows`] ?? 0}
              cols={layoutInfo?.[`${layoutKey}Cols`] ?? 0}
              traySize={getTraySize(layoutInfo, layoutKey)}
              slots={preview[previewKey]}
              highlightedPositions={index === 1 ? selectedTray1 : selectedTray2}
            />
          ))}
        </Space>
      </div>
    </Space>
  );
}

export default TrayPreviewPanel;
