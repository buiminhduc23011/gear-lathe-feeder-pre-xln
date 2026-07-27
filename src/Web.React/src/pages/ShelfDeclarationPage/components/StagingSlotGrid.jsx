import React from "react";
import { Space, Tag, Typography } from "antd";

const { Text } = Typography;

function StagingSlotGrid({
  loadingSlots,
  occupiedStagingSlots,
  selectedMachineStagingSlots,
  stagingSlotIndex,
  onSelectStagingSlot
}) {
  return (
    <div>
      <div style={{ marginBottom: 8 }}>
        <Space size={8}>
        
          {loadingSlots ? <Tag color="processing">Đang tải</Tag> : null}
        </Space>
      </div>

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
          const slot = occupiedStagingSlots.get(slotIndex);
          const isSelected = stagingSlotIndex === slotIndex;
          const isOccupied = Boolean(slot);
          const isOwned = selectedMachineStagingSlots.has(slotIndex);
          const isDisabled = !isOwned;
          const isBlockedForDeclaration = isOwned && isOccupied;
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
              data-testid={`staging-slot-${slotIndex}`}
              role="button"
              tabIndex={isDisabled ? -1 : 0}
              aria-disabled={isDisabled ? "true" : "false"}
              aria-busy={isBlockedForDeclaration ? "true" : "false"}
              aria-pressed={isSelected ? "true" : "false"}
              onClick={() => {
                if (!isDisabled) {
                  onSelectStagingSlot(slotIndex);
                }
              }}
              onKeyDown={(event) => {
                if (!isDisabled && (event.key === "Enter" || event.key === " ")) {
                  event.preventDefault();
                  onSelectStagingSlot(slotIndex);
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
                opacity: !isOwned ? 0.75 : 1
              }}
            >
              <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", gap: 10, marginBottom: 4 }}>
                <Text strong style={{ whiteSpace: "nowrap", flexShrink: 0 }}>Slot {slotIndex}</Text>
                <Tag color={tagColor} style={{ marginInlineEnd: 0, whiteSpace: "nowrap", flexShrink: 0 }}>
                  {tagLabel}
                </Tag>
              </div>

              {!isOwned ? (
                <Text type="secondary" style={{ fontSize: 12 }}>
                  Không khả dụng cho máy này
                </Text>
              ) : isOccupied ? (
                <Text type="secondary" style={{ fontSize: 12 }}>
                  {slot.shelfLayoutName ?? `Orders: ${slot.orderCount ?? 0}`}
                </Text>
              ) : (
                <Text type="secondary" style={{ fontSize: 12 }}>
                  Sẵn sàng
                </Text>
              )}
            </div>
          );
        })}
      </div>
    </div>
  );
}

export default StagingSlotGrid;
