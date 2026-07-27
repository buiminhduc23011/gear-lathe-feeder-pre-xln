import React from "react";
import { Tooltip } from "antd";

function TrayGridPreview({ trayLabel, rows, cols, traySize, slots, highlightedPositions }) {
  const filledColor = traySize === "large" ? "#f97316" : "#2563eb";
  const selectedColor = "#a21caf";
  const slotSize = 24;
  const columnGap = 8;
  const rowGap = 4;
  const selectedRingWidth = 4;

  return (
    <div
      style={{
        background: "#f8fafc",
        border: "1px solid #e2e8f0",
        borderRadius: 12,
        display: "block",
        maxWidth: "100%",
        padding: 14
      }}
    >
      <div style={{ marginBottom: 10, fontSize: 15, fontWeight: 700, color: "#334155" }}>
        {trayLabel}
      </div>

      <div style={{ maxWidth: "100%" }}>
      <div
        style={{
          display: "grid",
          gridTemplateColumns: cols > 0 ? `repeat(${cols}, minmax(0, 1fr))` : "none",
          columnGap,
          rowGap,
          gridAutoRows: `${slotSize + (selectedRingWidth * 2)}px`,
          justifyItems: "center",
          alignItems: "center",
          width: "100%",
          padding: selectedRingWidth,
          boxSizing: "border-box",
          overflow: "visible"
        }}
      >
        {(() => {
          const gridRows = [];
          for (let r = 0; r < rows; r++) {
            const rowSlots = slots.slice(r * cols, (r + 1) * cols);
            gridRows.push(rowSlots);
          }
          // Reverse to draw grid from bottom-up visually
          return gridRows.reverse().flatMap(row => row);
        })().map((slot) => {
          const isHighlighted = highlightedPositions?.has(slot.position);
          return (
            <Tooltip
              key={`${trayLabel}-${slot.position}`}
              title={(
                <div>
                  <div>Vị trí: {slot.position}</div>
                  <div>Order: {slot.orderId ?? "—"}</div>
                  <div>Model: {slot.modelName ?? "—"}</div>
                </div>
              )}
            >
              <div
                style={{
                  width: `min(${slotSize}px, 78%)`,
                  aspectRatio: "1 / 1",
                  borderRadius: 999,
                  background: slot.status === "HasProduct" ? filledColor : "#e2e8f0",
                  border: isHighlighted
                    ? `1px solid ${selectedColor}`
                    : "1px solid rgba(15,23,42,0.06)",
                  boxShadow: isHighlighted
                    ? `0 0 0 2px #ffffff, 0 0 0 ${selectedRingWidth}px ${selectedColor}`
                    : "none",
                  position: "relative",
                  zIndex: isHighlighted ? 1 : 0,
                  transition: "background-color 120ms ease, border-color 120ms ease, box-shadow 120ms ease"
                }}
              />
            </Tooltip>
          );
        })}
      </div>
      </div>
    </div>
  );
}

export default TrayGridPreview;
