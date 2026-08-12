import React from "react";
import { Space, Tag, Tooltip, Typography } from "antd";
import { DECLARATION_MODES } from "../constants";

const { Text } = Typography;

const JIG_COLORS = {
  1: "#2563eb",
  2: "#f97316",
  3: "#16a34a",
  4: "#a21caf"
};

const MAX_VISIBLE_LAYERS = 20;

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

function CartStack({ position, highlighted }) {
  const capacity = Math.max(0, Number(position.capacity) || 0);
  const quantity = Math.max(0, Number(position.quantity) || 0);
  const layerCount = Math.min(Math.max(capacity, quantity), MAX_VISIBLE_LAYERS);
  const color = JIG_COLORS[position.jigType] ?? "#cbd5e1";

  if (!layerCount) {
    return (
      <div
        style={{
          alignItems: "center",
          color: "#94a3b8",
          display: "flex",
          flex: 1,
          fontSize: 12,
          justifyContent: "center",
          minHeight: 220
        }}
      >
        Chưa xếp hàng
      </div>
    );
  }

  return (
    <div
      aria-label={`Vị trí ${position.position}: ${quantity} sản phẩm trên sức chứa ${capacity}`}
      style={{
        alignItems: "stretch",
        display: "flex",
        flex: 1,
        flexDirection: "column-reverse",
        gap: 3,
        justifyContent: "flex-start",
        minHeight: 220,
        padding: "8px 10px"
      }}
    >
      {Array.from({ length: layerCount }, (_, index) => {
        const filled = index < quantity;
        return (
          <Tooltip
            key={`${position.position}-layer-${index}`}
            title={`${filled ? "Sản phẩm" : "Khoảng trống"} — lớp ${index + 1}/${capacity}`}
          >
            <div
              style={{
                background: filled ? color : "#e2e8f0",
                border: "1px solid rgba(15,23,42,0.06)",
                borderRadius: 4,
                boxShadow: highlighted && filled ? "0 0 0 2px #ef4444" : "none",
                flex: "1 1 0",
                minHeight: 7,
                opacity: filled ? 1 : 0.72,
                transition: "background-color 120ms ease, box-shadow 120ms ease"
              }}
            />
          </Tooltip>
        );
      })}
    </div>
  );
}

function CartPositionPreview({ position, highlighted }) {
  const filled = position.quantity > 0;
  const color = JIG_COLORS[position.jigType] ?? "#cbd5e1";
  const hiddenLayers = Math.max(0, (Number(position.capacity) || 0) - MAX_VISIBLE_LAYERS);

  return (
    <div style={{ width: 92 }}>
      <div style={{ marginBottom: 6, textAlign: "center" }}>
        <Text strong style={{ color: "#334155", fontSize: 12 }}>
          VỊ TRÍ {position.position}
        </Text>
      </div>
      <div
        style={{
          background: filled ? "#f8fafc" : "#fbfdff",
          border: `1px solid ${highlighted ? "#ef4444" : filled ? color : "#e2e8f0"}`,
          borderRadius: 10,
          boxShadow: highlighted ? "0 0 0 2px rgba(239,68,68,0.16)" : "none",
          boxSizing: "border-box",
          display: "flex",
          flexDirection: "column",
          height: 270,
          overflow: "hidden",
          transition: "border-color 120ms ease, box-shadow 120ms ease"
        }}
      >
        <div
          style={{
            alignItems: "center",
            background: filled ? `${color}12` : "#f8fafc",
            borderBottom: "1px solid #eef2f7",
            display: "flex",
            justifyContent: "center",
            minHeight: 32,
            padding: "0 5px"
          }}
        >
          <Tag color={filled ? color : "default"} style={{ marginInlineEnd: 0, fontSize: 11 }}>
            {filled ? `Jig ${position.jigType}` : "Trống"}
          </Tag>
        </div>

        <CartStack position={position} highlighted={highlighted} />

        <div style={{ borderTop: "1px solid #eef2f7", padding: "7px 8px 8px" }}>
          <Text strong style={{ display: "block", fontSize: 12, textAlign: "center" }}>
            {position.quantity} / {position.capacity || 0}
          </Text>
          <Text type="secondary" style={{ display: "block", fontSize: 10, textAlign: "center" }}>
            {position.jigHeightMm || 0} mm · {position.inputThickness || 0} mm
          </Text>
        </div>
      </div>
      <Tooltip title={position.orderIds?.join(", ") || "Chưa có order"}>
        <Text
          ellipsis
          type="secondary"
          style={{ display: "block", fontSize: 11, marginTop: 6, textAlign: "center" }}
        >
          {position.orderIds?.length ? position.orderIds.join(", ") : "Chưa có order"}
        </Text>
      </Tooltip>
      {hiddenLayers > 0 ? (
        <Text type="secondary" style={{ display: "block", fontSize: 10, textAlign: "center" }}>
          +{hiddenLayers} lớp
        </Text>
      ) : null}
    </div>
  );
}

function TrayPreviewPanel({ mode, machineSlotIndex, busyMachineSlots, cartPreview, selectedOrderSlots }) {
  const highlightedPositions = selectedOrderSlots?.cartPositions ?? new Set();
  const positions = cartPreview ?? [];

  return (
    <Space direction="vertical" size={12} style={{ display: "flex" }}>
      {mode !== DECLARATION_MODES.AGV ? (
        <div style={{ background: "#fcfcfd", border: "1px solid #eef2f7", borderRadius: 14, padding: 14 }}>
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
          margin: "0 auto",
          maxWidth: "100%",
          padding: 10,
          width: "fit-content"
        }}
      >
        <div style={{ marginBottom: 8 }}>
          <Text strong>Trực quan hóa — Xe hàng</Text>
          <div style={{ display: "flex", flexWrap: "wrap", gap: "3px 8px", marginTop: 6, maxWidth: "100%" }} aria-label="Chú thích">
            {[1, 2, 3, 4].map((jigType) => (
              <span key={jigType} style={{ alignItems: "center", display: "inline-flex", flex: "0 0 auto", gap: 5 }}>
                <LegendDot color={JIG_COLORS[jigType]} />
                <Text type="secondary" style={{ fontSize: 12 }}>Jig {jigType}</Text>
              </span>
            ))}
            <span style={{ alignItems: "center", display: "inline-flex", flex: "0 0 auto", gap: 5 }}>
              <LegendDot color="#e2e8f0" />
              <Text type="secondary" style={{ fontSize: 12 }}>Trống</Text>
            </span>
            <span style={{ alignItems: "center", display: "inline-flex", flex: "0 0 auto", gap: 5 }}>
              <LegendDot color="#ef4444" />
              <Text type="secondary" style={{ fontSize: 12 }}>Order đang chọn</Text>
            </span>
          </div>
        </div>

        <div
          style={{
            display: "grid",
            gridTemplateColumns: "repeat(4, max-content)",
            gap: 10,
            alignItems: "start",
            justifyContent: "center",
            overflowX: "auto",
            paddingBottom: 2
          }}
        >
          {positions.map((position) => (
            <CartPositionPreview
              key={position.position}
              position={position}
              highlighted={highlightedPositions.has(position.position)}
            />
          ))}
        </div>
      </div>
    </Space>
  );
}

export default TrayPreviewPanel;
