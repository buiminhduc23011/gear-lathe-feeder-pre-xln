import React, { useMemo } from "react";
import { Button, Popconfirm, Space, Table, Tag } from "antd";
import SectionCard from "../../../components/ui/SectionCard";
import {
  DECLARATION_MODES,
  STATUS_COLORS,
  STATUS_LABELS
} from "../constants";
import { formatDateTime, formatDuration } from "../utils";

function canCancelDeclaration(record) {
  return record.status === "Created"
    && !record.pickedByAgvId
    && !record.pickedByAgvName
    && !record.agvTakenAtUtc;
}

function createHistoryColumns({ onCancel }) {
  return [
    { title: "#", dataIndex: "id", width: 60, sorter: (a, b) => a.id - b.id },
    {
      title: "Mode",
      dataIndex: "mode",
      width: 110,
      render: (value) => (
        <Tag color={value === DECLARATION_MODES.AGV ? "geekblue" : "green"}>
          {value === DECLARATION_MODES.AGV ? "AGV" : "Thủ Công"}
        </Tag>
      )
    },
    {
      title: "Slot",
      width: 130,
      render: (_, record) => (
        record.mode === DECLARATION_MODES.AGV
          ? `Staging ${record.stagingSlotIndex ?? "-"}`
          : `Machine ${record.machineSlotIndex ?? "-"}`
      )
    },
    { title: "Loại kệ", dataIndex: "shelfLayoutName", width: 180, ellipsis: true },
    { title: "Orders", dataIndex: "orderCount", width: 70, align: "center" },
    {
      title: "Trạng thái",
      dataIndex: "status",
      width: 140,
      render: (value) => <Tag color={STATUS_COLORS[value] || "default"}>{STATUS_LABELS[value] || value}</Tag>
    },
    { title: "Người tạo", dataIndex: "createdByUsername", width: 120 },
    {
      title: "AGV lấy",
      width: 140,
      render: (_, record) => record.pickedByAgvName ? `${record.pickedByAgvName} (${record.pickedByAgvId})` : "-"
    },
    {
      title: "Tạo lúc",
      dataIndex: "createdAtUtc",
      width: 150,
      render: formatDateTime
    },
    {
      title: "Thời lượng SX",
      dataIndex: "productionDurationSeconds",
      width: 110,
      render: formatDuration
    },
    {
      title: "Người kết thúc",
      width: 130,
      render: (_, record) => record.cancelledByUsername || record.clearedByUsername || "-"
    },
    {
      title: "Kết thúc lúc",
      width: 150,
      render: (_, record) => formatDateTime(record.cancelledAtUtc || record.clearedAtUtc)
    },
    {
      title: "",
      width: 120,
      render: (_, record) => (
        <Space size={8}>
          {canCancelDeclaration(record) ? (
            <Popconfirm
              title="Hủy khai báo này?"
              okText="Hủy"
              cancelText="Không"
              onConfirm={() => onCancel(record.id)}
            >
              <Button size="small" danger>
                Hủy
              </Button>
            </Popconfirm>
          ) : null}
        </Space>
      )
    }
  ];
}

function HistoryDeclarationTab({ history, loadingHistory, onCancel }) {
  const columns = useMemo(
    () => createHistoryColumns({ onCancel }),
    [onCancel]
  );

  return (
    <SectionCard
      title="Lịch sử khai báo"
      description="Lịch sử các bản khai báo kệ đã thực hiện."
    >
      <Table
        dataSource={history}
        columns={columns}
        rowKey="id"
        loading={loadingHistory}
        size="small"
        pagination={{ pageSize: 8, showSizeChanger: false, size: "small" }}
        scroll={{ x: 1400 }}
      />
    </SectionCard>
  );
}

export default HistoryDeclarationTab;
