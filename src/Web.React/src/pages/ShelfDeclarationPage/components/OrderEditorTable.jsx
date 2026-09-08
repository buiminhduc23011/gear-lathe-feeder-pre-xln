import React, { useEffect, useRef, useState } from "react";
import { Button, Input, InputNumber, Space, Table, Tag, Typography } from "antd";
import { DeleteOutlined, PlusOutlined } from "@ant-design/icons";

const { Text } = Typography;

function focusField(field, key) {
  window.setTimeout(() => {
    const root = document.querySelector(`[data-${field}-key="${key}"]`);
    (root?.querySelector?.("input") ?? root)?.focus?.();
  }, 0);
}

function isValidQuantityInput(value) {
  const number = Number((value ?? "").trim());
  return Number.isFinite(number) && number >= 1;
}

function OrderEditorTable({
  orders,
  totalCapacity,
  canAddOrder,
  computedOrders,
  orderRows,
  selectedOrderIndex,
  isBusyLocked,
  onAddOrder,
  onUpdateOrder,
  onResolveModelOrder,
  onRemoveOrder,
  onSelectOrder,
  onFocusSubmit
}) {
  const tableRows = orderRows ?? computedOrders;
  const addButtonRef = useRef(null);
  const previousRowCountRef = useRef(tableRows.length);
  const [invalidQuantityKeys, setInvalidQuantityKeys] = useState(() => new Set());

  useEffect(() => {
    if (!isBusyLocked && tableRows.length > previousRowCountRef.current) {
      const nextRow = tableRows[tableRows.length - 1];
      if (nextRow?.key) focusField("model-order", nextRow.key);
    }
    previousRowCountRef.current = tableRows.length;
  }, [isBusyLocked, tableRows]);

  const handleModelOrderEnter = (record, modelOrder) => {
    onSelectOrder(tableRows.findIndex((row) => row.key === record.key));
    const result = onResolveModelOrder?.(record.key, { modelOrder });
    if (result?.resolved) focusField("order", record.key);
  };

  const columns = [
    {
      title: "#",
      width: 44,
      align: "center",
      render: (_, __, index) => <Text strong>{index + 1}</Text>
    },
    {
      title: "Article ID",
      dataIndex: "reportModelName",
      width: 220,
      render: (value, record) => isBusyLocked ? (
        <Text>{value || record.modelName || "—"}</Text>
      ) : (
        <Input
          data-model-order-key={record.key}
          size="small"
          placeholder="Nhập Article ID"
          value={value || ""}
          onChange={(event) => onUpdateOrder(record.key, "reportModelName", event.target.value)}
          onBlur={(event) => {
            if (event.currentTarget.value?.trim() && !record.modelName?.trim()) {
              onResolveModelOrder?.(record.key, { modelOrder: event.currentTarget.value });
            }
          }}
          onKeyDown={(event) => {
            if (event.key === "Enter") {
              event.preventDefault();
              handleModelOrderEnter(record, event.currentTarget.value);
            }
          }}
          status={value?.trim() && !record.modelName?.trim() ? "error" : undefined}
          disabled={isBusyLocked}
        />
      )
    },
    {
      title: "ID",
      dataIndex: "modelName",
      width: 130,
      render: (value) => value ? <Text code style={{ fontSize: 12 }}>{value}</Text> : <Text type="secondary">—</Text>
    },
    {
      title: "Order",
      dataIndex: "orderId",
      width: 180,
      render: (value, record) => {
        const hasArticle = Boolean(record.modelName?.trim());
        const canEdit = !isBusyLocked && hasArticle && Number(record.orderInput ?? 1) !== 0;
        return isBusyLocked ? (
          <Text code style={{ fontSize: 12 }}>{value || "—"}</Text>
        ) : (
          <Input
            data-order-key={record.key}
            size="small"
            placeholder={!hasArticle ? "Chọn Article ID trước" : Number(record.orderInput ?? 1) === 0 ? "Tự sinh Order" : "Nhập Order"}
            value={value || ""}
            maxLength={30}
            onChange={(event) => onUpdateOrder(record.key, "orderId", event.target.value)}
            disabled={!canEdit}
          />
        );
      }
    },
    {
      title: "Jig",
      width: 90,
      render: (_, record) => record.jigType ? <Tag color="blue">Jig {record.jigType}</Tag> : <Tag>Chưa chọn</Tag>
    },
    {
      title: "Số lượng",
      dataIndex: "quantity",
      width: 120,
      render: (value, record) => isBusyLocked ? (
        <Text>{value ?? 0}</Text>
      ) : (
        <InputNumber
          data-quantity-key={record.key}
          size="small"
          style={{ width: "100%", fontVariantNumeric: "tabular-nums" }}
          min={1}
          value={value}
          placeholder="Nhập SL"
          status={invalidQuantityKeys.has(record.key) ? "error" : undefined}
          onChange={(nextValue) => {
            setInvalidQuantityKeys((current) => {
              const next = new Set(current);
              next.delete(record.key);
              return next;
            });
            onUpdateOrder(record.key, "quantity", nextValue);
          }}
          onKeyDown={(event) => {
            if (event.key === "Enter") {
              event.preventDefault();
              if (!isValidQuantityInput(event.currentTarget.value)) {
                setInvalidQuantityKeys((current) => new Set(current).add(record.key));
              } else {
                onFocusSubmit?.();
              }
            }
          }}
          disabled={isBusyLocked}
        />
      )
    },
    {
      title: "Phân bổ Xe hàng",
      width: 230,
      render: (_, record) => record.placements?.length
        ? record.placements.map((placement) => (
          <Tag key={placement.key} color="purple" style={{ marginBottom: 3 }}>
            Vị trí {placement.cartPositionIndex}: {placement.quantity}
          </Tag>
        ))
        : <Text type="secondary">Chưa phân bổ</Text>
    },
    {
      title: "",
      width: 44,
      render: (_, __, index) => (
        <Button
          type="text"
          danger
          size="small"
          icon={<DeleteOutlined />}
          onClick={() => onRemoveOrder(index)}
          disabled={isBusyLocked}
        />
      )
    }
  ];

  return (
    <div style={{ background: "#fcfcfd", border: "1px solid #eef2f7", borderRadius: 14, padding: 14, height: "100%" }}>
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", gap: 12, flexWrap: "wrap", marginBottom: 8 }}>
        <Space size={12} wrap>
          <Text strong>Danh sách order ({tableRows.length})</Text>
          <Tag>Xe hàng: 4 vị trí</Tag>
          <Tag>Sức chứa: {totalCapacity}</Tag>
        </Space>
        <Button ref={addButtonRef} type="dashed" icon={<PlusOutlined />} onClick={onAddOrder} disabled={!canAddOrder}>
          Thêm order
        </Button>
      </div>
      <Table
        dataSource={tableRows}
        columns={columns}
        rowKey="key"
        onRow={(record) => {
          const rowIndex = tableRows.findIndex((row) => row.key === record.key);
          return {
            onClick: () => onSelectOrder(rowIndex),
            style: { background: selectedOrderIndex === rowIndex ? "#fff1f2" : undefined, cursor: "pointer" }
          };
        }}
        pagination={false}
        size="small"
        bordered
        scroll={{ x: 1050, y: 540 }}
        locale={{ emptyText: "Chưa có order. Nhấn \"Thêm order\" để bắt đầu." }}
      />
    </div>
  );
}

export default OrderEditorTable;
