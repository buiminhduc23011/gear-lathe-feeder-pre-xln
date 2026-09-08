import React, { useCallback, useEffect, useMemo, useState } from "react";
import dayjs from "dayjs";
import {
  Button,
  DatePicker,
  Empty,
  Input,
  Select,
  Space,
  Spin,
  Table,
  Tag,
  Typography
} from "antd";
import { DownloadOutlined } from "@ant-design/icons";
import PageHeader from "../components/ui/PageHeader";
import SectionCard from "../components/ui/SectionCard";
import { API_ENDPOINTS, apiClient, getApiErrorMessage } from "../config/api";
import { showErrorMessage, showWarningMessage } from "../utils/appMessage";
import { useMachineContext } from "../contexts/MachineContext";

const { Text } = Typography;

const STATUS_OPTIONS = [
  { value: "", label: "Tất cả trạng thái" },
  { value: "Created", label: "Đã tạo" },
  { value: "AgvTaken", label: "AGV đã lấy" },
  { value: "Loaded", label: "Đã load" },
  { value: "InProduction", label: "Đang sản xuất" },
  { value: "Completed", label: "Hoàn thành" },
  { value: "Cleared", label: "Đã clear" },
  { value: "Cancelled", label: "Đã hủy" }
];

const SHELF_OPTIONS = [
  { value: 0, label: "Tất cả kệ" },
  { value: 1, label: "Kệ 1" }
];

function ProductionReportPage() {
  const machineContext = useMachineContext();
  const [localMachines, setLocalMachines] = useState([]);
  const [localMachineId, setLocalMachineId] = useState(null);
  const [status, setStatus] = useState("");
  const [shelfIndex, setShelfIndex] = useState(0);
  const [searchOrder, setSearchOrder] = useState("");
  const [searchModel, setSearchModel] = useState("");
  const [startDate, setStartDate] = useState(dayjs().startOf("day"));
  const [endDate, setEndDate] = useState(dayjs().endOf("day"));
  const [report, setReport] = useState(null);
  const [loading, setLoading] = useState(false);

  const machines = machineContext?.machines?.length > 0 ? machineContext.machines : localMachines;
  const machineId = machineContext?.currentMachineId ?? localMachineId;

  const handleMachineChange = useCallback((id) => {
    setLocalMachineId(id);
    if (machineContext?.changeMachine) {
      machineContext.changeMachine(id);
    }
  }, [machineContext]);

  const fetchMachines = useCallback(async () => {
    try {
      const response = await apiClient.get(API_ENDPOINTS.machines);
      const items = response.data ?? [];
      setLocalMachines(items);
      if (items.length > 0) {
        setLocalMachineId((previous) => previous ?? items[0].machineId);
      }
    } catch {
      setLocalMachines([]);
    }
  }, []);

  const fetchReport = useCallback(async () => {
    if (!machineId) {
      setReport(null);
      return;
    }

    setLoading(true);

    try {
      const fromUtc = startDate ? startDate.startOf("day").toISOString() : undefined;
      const toUtc = endDate ? endDate.endOf("day").toISOString() : undefined;

      const response = await apiClient.get(API_ENDPOINTS.productionLifecycleReport, {
        params: {
          machineId,
          status: status || undefined,
          shelfIndex: shelfIndex || undefined,
          fromUtc,
          toUtc
        }
      });
      setReport(response.data);
    } catch (error) {
      const fallback = "Không tải được báo cáo sản xuất.";
      showErrorMessage(getApiErrorMessage(error, fallback));
    } finally {
      setLoading(false);
    }
  }, [machineId, shelfIndex, status, startDate, endDate]);

  useEffect(() => {
    if (!machineContext?.machines?.length) {
      fetchMachines().catch((error) => {
        showErrorMessage(getApiErrorMessage(error, "Không tải được danh sách máy."));
      });
    }
  }, [fetchMachines, machineContext?.machines]);

  useEffect(() => {
    fetchReport().catch((error) => {
      showErrorMessage(getApiErrorMessage(error, "Không tải được báo cáo sản xuất."));
    });
  }, [fetchReport]);

  const machineOptions = useMemo(
    () =>
      machines.map((machine) => ({
        value: machine.machineId,
        label: `${machine.machineCode} - ${machine.machineName}`
      })),
    [machines]
  );

  const columns = useMemo(
    () => [
      {
        title: "Declaration",
        dataIndex: "declarationId",
        width: 120,
        render: (value) => <Text strong>#{value}</Text>
      },
      {
        title: "Kệ",
        dataIndex: "shelfIndex",
        width: 90,
        render: (value) => value ?? "-"
      },
      {
        title: "Chế độ",
        dataIndex: "mode",
        width: 110,
        render: (value, row) => (
          <div>
            <Tag color="cyan">{value || "Manual"}</Tag>
            {value === "AGV" && row.stagingSlotIndex ? (
              <div style={{ fontSize: "11px", color: "#666" }}>Staging #{row.stagingSlotIndex}</div>
            ) : null}
            {value === "Manual" && row.machineSlotIndex ? (
              <div style={{ fontSize: "11px", color: "#666" }}>Machine #{row.machineSlotIndex}</div>
            ) : null}
          </div>
        )
      },
      {
        title: "Trạng thái",
        dataIndex: "status",
        width: 130,
        render: (value) => <Tag color={mapStatusColor(value)}>{getStatusLabel(value)}</Tag>
      },
      {
        title: "Tiến độ order",
        width: 320,
        render: (_, record) => {
          const orders = record.orders ?? [];
          if (orders.length === 0) {
            return <Text type="secondary">Không có order</Text>;
          }

          return (
            <Space direction="vertical" size={2} style={{ width: "100%" }}>
              {orders.map((order) => (
                <div
                  key={order.orderSequence}
                  style={{
                    display: "flex",
                    justifyContent: "space-between",
                    gap: 8,
                    fontSize: 12,
                    padding: "2px 0",
                    borderBottom: "1px dashed #f0f0f0"
                  }}
                >
                  <Text strong={order.status === "InProduction"}>
                    #{order.orderSequence} {order.orderId || "(No ID)"} - {order.modelName}
                  </Text>
                  <Space size={4}>
                    <Text type="secondary">{order.quantity} pcs</Text>
                    <Tag
                      color={mapStatusColor(order.status)}
                      style={{ fontSize: 10, lineHeight: "16px", padding: "0 4px", margin: 0 }}
                    >
                      {getStatusLabel(order.status)}
                    </Tag>
                  </Space>
                </div>
              ))}
            </Space>
          );
        }
      },
      {
        title: "Thời điểm lifecycle",
        width: 220,
        render: (_, record) => (
          <div style={{ fontSize: 12, lineHeight: 1.4 }}>
            <div>Tạo: {formatDateTimeShort(record.createdAtUtc)}</div>
            <div>AGV lấy: {formatDateTimeShort(record.agvTakenAtUtc) || "-"}</div>
            <div>SX: {formatDateTimeShort(record.productionStartedAtUtc) || "-"}</div>
            <div>Kết thúc: {formatDateTimeShort(record.endedAtUtc) || "-"}</div>
          </div>
        )
      },
      {
        title: "Thời gian chu kỳ",
        width: 160,
        render: (_, record) => (
          <div style={{ fontSize: 12, lineHeight: 1.4 }}>
            <div>Chờ AGV: {formatSeconds(record.agvPickupDurationSeconds)}</div>
            <div>Sản xuất: {formatSeconds(record.productionDurationSeconds)}</div>
            <div>Tổng vòng đời: {formatSeconds(record.lifecycleDurationSeconds)}</div>
          </div>
        )
      }
    ],
    []
  );

  const filteredItems = useMemo(() => {
    const items = report?.items ?? [];
    if (!searchOrder && !searchModel) {
      return items;
    }

    const lowerOrder = searchOrder.toLowerCase().trim();
    const lowerModel = searchModel.toLowerCase().trim();

    return items.filter((item) => {
      const orders = item.orders ?? [];
      const matchOrder =
        !searchOrder ||
        orders.some((ord) => (ord.orderId ?? "").toLowerCase().includes(lowerOrder));
      const matchModel =
        !searchModel ||
        orders.some(
          (ord) =>
            (ord.modelName ?? "").toLowerCase().includes(lowerModel) ||
            (ord.reportModelName ?? "").toLowerCase().includes(lowerModel)
        );

      return matchOrder && matchModel;
    });
  }, [report, searchOrder, searchModel]);

  const quickStats = useMemo(() => {
    let totalOrders = 0;
    let totalProducts = 0;
    let runningProducts = 0;
    let completedProducts = 0;
    let clearedProducts = 0;
    let cancelledProducts = 0;

    filteredItems.forEach((item) => {
      const orders = item.orders ?? [];
      totalOrders += orders.length;

      orders.forEach((ord) => {
        const qty = ord.quantity || 0;
        totalProducts += qty;

        switch (ord.status) {
          case "InProduction":
            runningProducts += qty;
            break;
          case "Completed":
            completedProducts += qty;
            break;
          case "Cleared":
            clearedProducts += qty;
            break;
          case "Cancelled":
            cancelledProducts += qty;
            break;
          default:
            break;
        }
      });
    });

    return {
      totalOrders,
      totalProducts,
      runningProducts,
      completedProducts,
      clearedProducts,
      cancelledProducts
    };
  }, [filteredItems]);

  const exportToExcel = useCallback(() => {
    if (!filteredItems || filteredItems.length === 0) {
      showWarningMessage("Không có dữ liệu để xuất Excel.");
      return;
    }

    const headers = [
      "Declaration ID",
      "Kệ",
      "Chế độ",
      "Trạng thái Declaration",
      "Thời gian tạo",
      "Thời gian AGV lấy",
      "Thời gian bắt đầu SX",
      "Thời gian kết thúc",
      "Thời gian chờ AGV lấy (s)",
      "Thời gian sản xuất (s)",
      "Tổng thời gian vòng đời (s)",
      "Order STT",
      "Order ID",
      "Model",
      "Report Model",
      "Số lượng",
      "Trạng thái Order"
    ];

    const rows = [];

    filteredItems.forEach((item) => {
      const modeText = item.mode === "AGV" 
        ? `AGV (Slot ${item.stagingSlotIndex ?? "-"})` 
        : `Manual (Slot ${item.machineSlotIndex ?? "-"})`;
      const statusText = getStatusLabel(item.status);
      const createdAt = formatDateTime(item.createdAtUtc);
      const agvTakenAt = formatDateTime(item.agvTakenAtUtc);
      const productionStartedAt = formatDateTime(item.productionStartedAtUtc);
      const endAt = formatDateTime(item.endedAtUtc);

      const orders = item.orders ?? [];
      if (orders.length === 0) {
        rows.push([
          item.declarationId,
          item.shelfIndex ?? "-",
          modeText,
          statusText,
          createdAt,
          agvTakenAt,
          productionStartedAt,
          endAt,
          item.agvPickupDurationSeconds ?? "",
          item.productionDurationSeconds ?? "",
          item.lifecycleDurationSeconds ?? "",
          "",
          "",
          "",
          "",
          "",
          ""
        ]);
      } else {
        orders.forEach((order) => {
          rows.push([
            item.declarationId,
            item.shelfIndex ?? "-",
            modeText,
            statusText,
            createdAt,
            agvTakenAt,
            productionStartedAt,
            endAt,
            item.agvPickupDurationSeconds ?? "",
            item.productionDurationSeconds ?? "",
            item.lifecycleDurationSeconds ?? "",
            order.orderSequence,
            order.orderId || "-",
            order.modelName || "-",
            order.reportModelName || "-",
            order.quantity ?? "",
            getStatusLabel(order.status)
          ]);
        });
      }
    });

    const csvContent =
      "\uFEFF" +
      [
        headers.join(","),
        ...rows.map((row) =>
          row.map((val) => `"${String(val).replace(/"/g, '""')}"`).join(",")
        )
      ].join("\n");

    const blob = new Blob([csvContent], { type: "text/csv;charset=utf-8;" });
    const url = URL.createObjectURL(blob);
    const link = document.createElement("a");
    const dateStr = dayjs().format("YYYYMMDD_HHmmss");
    link.setAttribute("href", url);
    link.setAttribute("download", `BaoCaoSanXuat_${dateStr}.csv`);
    link.style.visibility = "hidden";
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  }, [filteredItems]);

  return (
    <div className="production-report-container">
      <style>{`
        .production-report-container {
          display: flex;
          flex-direction: column;
          height: calc(100vh - 125px);
          overflow: hidden;
          gap: 12px;
        }
        .production-report-table-card {
          flex: 1;
          min-height: 0;
          display: flex;
          flex-direction: column;
          overflow: hidden;
        }
        .production-report-table-card .ant-card-body {
          display: flex;
          flex-direction: column;
          flex: 1;
          min-height: 0;
          overflow: hidden;
          padding: 16px !important;
        }
        .production-report-table-card .ant-table-wrapper,
        .production-report-table-card .ant-spin-nested-loading,
        .production-report-table-card .ant-spin-container,
        .production-report-table-card .ant-table,
        .production-report-table-card .ant-table-container {
          display: flex;
          flex-direction: column;
          flex: 1;
          min-height: 0;
          overflow: hidden;
        }
        .production-report-table-card .ant-table-body {
          flex: 1;
          overflow: auto !important;
        }
        .production-report-table-card .ant-pagination {
          margin: 12px 0 0 0 !important;
          flex-shrink: 0;
        }
      `}</style>

      <PageHeader
        title="Báo cáo sản xuất"
        description="Theo dõi trạng thái declaration/order và tiến trình load thông số robot."
        actions={(
          <Select
            value={machineId}
            onChange={handleMachineChange}
            options={machineOptions}
            style={{ minWidth: 320 }}
            placeholder="Chọn máy"
          />
        )}
      />

      <SectionCard 
        className="production-report-table-card"
      >
        {/* Filters Row */}
        <div style={{ display: "flex", flexWrap: "wrap", justifyContent: "space-between", alignItems: "center", gap: "12px", marginBottom: "12px", flexShrink: 0 }}>
          <Space wrap>
            <Space size={4}>
              <Text type="secondary">Từ:</Text>
              <DatePicker
                value={startDate}
                onChange={setStartDate}
                format="DD/MM/YYYY"
                placeholder="Chọn ngày"
                style={{ width: 130 }}
              />
            </Space>
            <Space size={4}>
              <Text type="secondary">Đến:</Text>
              <DatePicker
                value={endDate}
                onChange={setEndDate}
                format="DD/MM/YYYY"
                placeholder="Chọn ngày"
                style={{ width: 130 }}
              />
            </Space>
            <Select value={status} onChange={setStatus} options={STATUS_OPTIONS} style={{ width: 180 }} />
            <Select value={shelfIndex} onChange={setShelfIndex} options={SHELF_OPTIONS} style={{ width: 140 }} />
            <Input
              allowClear
              value={searchOrder}
              onChange={(event) => setSearchOrder(event.target.value)}
              placeholder="Tìm theo Order ID"
              style={{ width: 180 }}
            />
            <Input
              allowClear
              value={searchModel}
              onChange={(event) => setSearchModel(event.target.value)}
              placeholder="Tìm model/Article ID"
              style={{ width: 180 }}
            />
          </Space>
          <Button
            icon={<DownloadOutlined />}
            type="primary"
            style={{ backgroundColor: "#217346", borderColor: "#217346" }}
            onClick={exportToExcel}
          >
            Xuất Excel
          </Button>
        </div>

        {/* Quick Stats Row */}
        <div style={{ marginBottom: "12px", flexShrink: 0 }}>
          <Text strong>Thống kê nhanh: </Text>
          <Text>Tổng order: {quickStats.totalOrders} | </Text>
          <Text>Tổng sản phẩm: {quickStats.totalProducts} | </Text>
          <Text>Đang chạy: {quickStats.runningProducts} | </Text>
          <Text>Hoàn thành: {quickStats.completedProducts} | </Text>
          <Text>Đã clear: {quickStats.clearedProducts} | </Text>
          <Text>Đã hủy: {quickStats.cancelledProducts}</Text>
        </div>

        <div style={{ height: "1px", background: "#f0f0f0", marginBottom: "16px", flexShrink: 0 }} />

        {loading ? (
          <div style={{ flex: 1, display: "flex", alignItems: "center", justifyContent: "center" }}>
            <Spin />
          </div>
        ) : filteredItems.length === 0 ? (
          <div style={{ flex: 1, display: "flex", alignItems: "center", justifyContent: "center" }}>
            <Empty description="Chưa có dữ liệu lifecycle theo bộ lọc hiện tại" />
          </div>
        ) : (
          <Table
            rowKey={(row) => row.declarationId}
            columns={columns}
            dataSource={filteredItems}
            pagination={{ pageSize: 10, size: "small" }}
            scroll={{ x: 1200, y: "100%" }}
          />
        )}
      </SectionCard>
    </div>
  );
}

function formatDateTime(value) {
  if (!value) {
    return "-";
  }

  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return "-";
  }

  return date.toLocaleString("vi-VN", {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
    second: "2-digit",
    hour12: false
  });
}

function formatDateTimeShort(value) {
  if (!value) return "";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "";

  const pad = (num) => String(num).padStart(2, "0");
  const d = pad(date.getDate());
  const m = pad(date.getMonth() + 1);
  const y = date.getFullYear();
  const h = pad(date.getHours());
  const min = pad(date.getMinutes());

  return `${d}/${m}/${y} ${h}:${min}`;
}

function formatSeconds(value) {
  if (value === null || value === undefined) {
    return "-";
  }

  if (value < 60) {
    return `${value}s`;
  }

  const seconds = value % 60;
  const totalMinutes = Math.floor(value / 60);
  const minutes = totalMinutes % 60;
  const totalHours = Math.floor(totalMinutes / 60);
  const hours = totalHours % 24;
  const days = Math.floor(totalHours / 24);

  const parts = [];
  if (days > 0) parts.push(`${days} ngày`);
  if (hours > 0 || days > 0) parts.push(`${hours}h`);
  if (minutes > 0 || hours > 0 || days > 0) parts.push(`${minutes}p`);
  if (seconds > 0 || parts.length === 0) parts.push(`${seconds}s`);

  return parts.join(" ");
}

function mapStatusColor(status) {
  switch (status) {
    case "Completed":
    case "Cleared":
      return "green";
    case "Cancelled":
      return "red";
    case "InProduction":
      return "blue";
    case "Loaded":
    case "AgvTaken":
      return "gold";
    default:
      return "default";
  }
}

function getStatusLabel(status) {
  switch (status) {
    case "Created":
      return "Đã tạo";
    case "AgvTaken":
      return "AGV đã lấy";
    case "Loaded":
      return "Đã load";
    case "InProduction":
      return "Đang sản xuất";
    case "Completed":
      return "Hoàn thành";
    case "Cleared":
      return "Đã clear";
    case "Cancelled":
      return "Đã hủy";
    default:
      return status || "Không xác định";
  }
}

export default ProductionReportPage;
