import React, { useEffect, useRef, useState } from "react";
import {
  Button,
  Input,
  InputNumber,
  Space,
  Table,
  Tag,
  Typography
} from "antd";
import { DeleteOutlined, PlusOutlined } from "@ant-design/icons";

const { Text } = Typography;

function buildLayoutTags(layoutInfo) {
  if (!layoutInfo) {
    return [];
  }

  return [
    {
      key: 1,
      color: layoutInfo.tray1Rows === 4 && layoutInfo.tray1Cols === 8 ? "orange" : "blue",
      text: `Tray 1: ${layoutInfo.tray1} ${layoutInfo.tray1Rows}x${layoutInfo.tray1Cols}`
    },
    {
      key: 2,
      color: layoutInfo.tray2Rows === 4 && layoutInfo.tray2Cols === 8 ? "orange" : "blue",
      text: `Tray 2: ${layoutInfo.tray2} ${layoutInfo.tray2Rows}x${layoutInfo.tray2Cols}`
    }
  ];
}

function focusField(field, key) {
  window.setTimeout(() => {
    const root = document.querySelector(`[data-${field}-key="${key}"]`);
    const target = root?.querySelector?.("input") ?? root;
    target?.focus?.();
  }, 0);
}

function isValidQuantityInput(value) {
  const text = (value ?? "").trim();
  if (!text) {
    return false;
  }

  const number = Number(text);
  return Number.isFinite(number) && number >= 1;
}

function getDisplayedQuantity(value, record) {
  if (record.hasAutoSplit && Number.isFinite(record.splitTotalQuantity)) {
    return record.splitTotalQuantity;
  }

  return value;
}

function OrderEditorTable({
  layoutInfo,
  orders,
  maxTotal,
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
  const layoutTags = buildLayoutTags(layoutInfo);
  const tableRows = orderRows ?? computedOrders;
  const addButtonRef = useRef(null);
  const previousRowCountRef = useRef(tableRows.length);
  const [invalidQuantityKeys, setInvalidQuantityKeys] = useState(() => new Set());

  useEffect(() => {
    if (!isBusyLocked && tableRows.length > previousRowCountRef.current) {
      const nextRow = tableRows[tableRows.length - 1];
      if (nextRow?.key && !nextRow.isAutoSplit) {
        focusField("model-order", nextRow.key);
      }
    }
    previousRowCountRef.current = tableRows.length;
  }, [isBusyLocked, tableRows]);

  const handleOrderEnter = (record) => {
    onSelectOrder(tableRows.findIndex((row) => row.key === record.key));
    focusField("quantity", record.key);
  };

  const handleModelOrderEnter = (record, modelOrder) => {
    onSelectOrder(tableRows.findIndex((row) => row.key === record.key));
    const result = onResolveModelOrder?.(record.key, { modelOrder });
    if (result?.resolved && result.orderInput === 0) {
      focusField("quantity", record.key);
      return;
    }

    if (result?.resolved || result === true) {
      focusField("order", record.key);
    }
  };

  const handleQuantityEnter = () => {
    if (canAddOrder) {
      addButtonRef.current?.focus?.();
      return;
    }

    onFocusSubmit?.();
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
      width: 230,
      render: (value, record) => (isBusyLocked ? (
        <Text>{value || record.modelName || "—"}</Text>
      ) : (
        <Space direction="vertical" size={4} style={{ width: "100%" }}>
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
            disabled={isBusyLocked || record.isAutoSplit}
          />
        </Space>
      ))
    },
    {
      title: "ID",
      dataIndex: "modelName",
      width: 130,
      render: (value) => (
        value ? (
          <Text code style={{ fontSize: 12 }}>{value}</Text>
        ) : (
          <Text type="secondary">---</Text>
        )
      )
    },
    {
      title: "Order",
      dataIndex: "orderId",
      width: 190,
      ellipsis: true,
      render: (value, record) => {
        const hasArticle = Boolean(record.modelName?.trim());
        const orderInput = Number(record.orderInput ?? 1);
        const canEditOrder = !isBusyLocked && !record.isAutoSplit && hasArticle && orderInput !== 0;
        const placeholder = !hasArticle
          ? "Chọn Article ID trước"
          : orderInput === 0
            ? "Tự sinh Order"
            : "Nhập Order";

        return isBusyLocked ? (
          value ? (
            <Text code style={{ fontSize: 12 }}>{value}</Text>
          ) : (
            <Text type="secondary">---</Text>
          )
        ) : (
          <Input
            data-order-key={record.key}
            size="small"
            placeholder={placeholder}
            value={value || ""}
            maxLength={30}
            onChange={(event) => onUpdateOrder(record.key, "orderId", event.target.value)}
            onKeyDown={(event) => {
              if (event.key === "Enter") {
                event.preventDefault();
                handleOrderEnter(record);
              }
            }}
            status={canEditOrder && !value?.trim() ? "error" : undefined}
            disabled={!canEditOrder}
          />
        );
      }
    },
    {
      title: "Số lượng",
      dataIndex: "quantity",
      width: 125,
      render: (value, record) => {
        const displayedQuantity = getDisplayedQuantity(value, record);

        return isBusyLocked ? (
          <Text>{displayedQuantity ?? 0}</Text>
        ) : (
          <Space direction="vertical" size={4} style={{ width: "100%" }}>
            <InputNumber
              data-quantity-key={record.key}
              size="small"
              style={{ width: "100%", fontVariantNumeric: "tabular-nums" }}
              min={1}
              value={displayedQuantity}
              placeholder="Nhập SL"
              status={invalidQuantityKeys.has(record.key) ? "error" : undefined}
              onChange={(nextValue) => {
                setInvalidQuantityKeys((current) => {
                  if (!current.has(record.key)) {
                    return current;
                  }

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
                    return;
                  }

                  handleQuantityEnter();
                }
              }}
              disabled={isBusyLocked || record.isAutoSplit}
            />
          </Space>
        );
      }
    },
    {
      title: "",
      width: 44,
      render: (_, record, index) => (
        <Button
          type="text"
          danger
          size="small"
          icon={<DeleteOutlined />}
          onClick={() => onRemoveOrder(index)}
          disabled={isBusyLocked || record.isAutoSplit}
        />
      )
    }
  ];

  return (
    <div
      style={{
        background: "#fcfcfd",
        border: "1px solid #eef2f7",
        borderRadius: 14,
        padding: 14,
        height: "100%"
      }}
    >
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", gap: 12, flexWrap: "wrap", marginBottom: 8 }}>
        <Space size={12} wrap>
          <Text strong>Danh sách order {layoutInfo ? `(${orders.length}/${maxTotal})` : `(${orders.length})`}</Text>
          {layoutTags.map((tag) => (
            <Tag key={tag.key} color={tag.color}>{tag.text}</Tag>
          ))}
          {layoutInfo ? <Tag>Sức chứa: {totalCapacity}</Tag> : null}
        </Space>

        <Button ref={addButtonRef} type="dashed" icon={<PlusOutlined />} onClick={onAddOrder} disabled={!canAddOrder}>
          Thêm order
        </Button>
      </div>

      <Table
        dataSource={tableRows}
        columns={columns}
        rowKey="key"
        onRow={(_, index) => ({
          onClick: () => onSelectOrder(index),
          style: {
            background: selectedOrderIndex === index ? "#fff1f2" : undefined,
            cursor: "pointer"
          }
        })}
        pagination={false}
        size="small"
        bordered
        scroll={{ x: 950, y: 540 }}
        locale={{
          emptyText: "Chưa có order. Nhấn \"Thêm order\" để bắt đầu."
        }}
      />
    </div>
  );
}

export default OrderEditorTable;
