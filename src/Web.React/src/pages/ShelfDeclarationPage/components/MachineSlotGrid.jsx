import React from "react";
import { Tag, Typography } from "antd";

const { Text } = Typography;

function MachineSlotGrid({ machineSlotIndex, busyMachineSlots, onSelectMachineSlot }) {
  return (
    <div
      style={{
        display: "grid",
        gridTemplateColumns: "repeat(4, minmax(180px, 1fr))",
        gap: 10,
        justifyContent: "end",
        overflowX: "auto",
        paddingBottom: 2
      }}
    >
      {[1, 2, 3, 4].map((slotIndex) => {
        const declaration = busyMachineSlots.get(slotIndex);
        const isSelected = machineSlotIndex === slotIndex;
        const isOwned = slotIndex === 1;
        const isOccupied = Boolean(declaration);
        const isDisabled = !isOwned;
        const tagColor = !isOwned ? "default" : isOccupied ? "orange" : "success";
        const tagLabel = !isOwned ? "Không thuộc máy" : isOccupied ? "Bận" : "Trống";
        const background = !isOwned ? "#f8fafc" : isOccupied ? "#f8fafc" : "#eff6ff";
        const borderColor = !isOwned
          ? "#e5e7eb"
          : isSelected
            ? "#2563eb"
            : isOccupied
              ? "#cbd5e1"
              : "#dbeafe";

        return (
          <div
            key={slotIndex}
            data-testid={`machine-slot-${slotIndex}`}
            role="button"
            tabIndex={isDisabled ? -1 : 0}
            aria-disabled={isDisabled ? "true" : "false"}
            aria-busy={isOccupied && isOwned ? "true" : "false"}
            aria-pressed={isSelected ? "true" : "false"}
            onClick={() => {
              if (!isDisabled) {
                onSelectMachineSlot(slotIndex);
              }
            }}
            onKeyDown={(event) => {
              if (!isDisabled && (event.key === "Enter" || event.key === " ")) {
                event.preventDefault();
                onSelectMachineSlot(slotIndex);
              }
            }}
            style={{
              minHeight: 74,
              minWidth: 180,
              borderRadius: 12,
              border: `1px solid ${borderColor}`,
              background,
              padding: "10px 12px",
              boxShadow: isSelected ? "0 0 0 2px rgba(37,99,235,0.08)" : "none",
              cursor: isDisabled ? "not-allowed" : "pointer",
              opacity: isDisabled ? 0.75 : 1
            }}
          >
            <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", gap: 10, marginBottom: 4 }}>
              <Text strong style={{ whiteSpace: "nowrap", flexShrink: 0 }}>Slot {slotIndex}</Text>
              <Tag color={tagColor} style={{ marginInlineEnd: 0, whiteSpace: "nowrap", flexShrink: 0 }}>
                {tagLabel}
              </Tag>
            </div>

            <Text type="secondary" style={{ fontSize: 12 }}>
              {!isOwned ? "Không khả dụng cho máy này" : isOccupied ? "Đang có khai báo hoạt động" : "Sẵn sàng"}
            </Text>
          </div>
        );
      })}
    </div>
  );
}

export default MachineSlotGrid;
