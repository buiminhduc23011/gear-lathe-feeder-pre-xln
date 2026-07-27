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
  Typography,
  message
} from "antd";
import { DownloadOutlined } from "@ant-design/icons";
import PageHeader from "../components/ui/PageHeader";
import SectionCard from "../components/ui/SectionCard";
import { API_ENDPOINTS, apiClient, getApiErrorMessage } from "../config/api";

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
  const [machines, setMachines] = useState([]);
  const [machineId, setMachineId] = useState(null);
  const [status, setStatus] = useState("");
  const [shelfIndex, setShelfIndex] = useState(0);
  const [searchOrder, setSearchOrder] = useState("");
  const [searchModel, setSearchModel] = useState("");
  const [startDate, setStartDate] = useState(dayjs().startOf("day"));
  const [endDate, setEndDate] = useState(dayjs().endOf("day"));
  const [report, setReport] = useState(null);
  const [loading, setLoading] = useState(false);

  const fetchMachines = useCallback(async () => {
    const response = await apiClient.get(API_ENDPOINTS.machines);
    const items = response.data ?? [];
    setMachines(items);
    if (items.length > 0) {
      setMachineId((previous) => previous ?? items[0].machineId);
    }
  }, []);

  const fetchReport = useCallback(async (isManual = false) => {
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
      message.error(getApiErrorMessage(error, fallback));
    } finally {
      setLoading(false);
    }
  }, [machineId, shelfIndex, status, startDate, endDate]);

  useEffect(() => {
    fetchMachines().catch((error) => {
      message.error(getApiErrorMessage(error, "Không tải được danh sách máy."));
    });
  }, [fetchMachines]);

  useEffect(() => {
    fetchReport().catch((error) => {
      message.error(getApiErrorMessage(error, "Không tải được báo cáo sản xuất."));
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
        title: "Trạng thái",
        dataIndex: "status",
        width: 130,
        render: (_, row) => (
          <Space wrap size={4}>
            <Tag color={mapStatusColor(row.status)} style={{ marginInlineEnd: 0 }}>
              {getStatusLabel(row.status)}
            </Tag>
            {row.isLoadingParameters ? (
              <Tag color="gold" style={{ marginInlineEnd: 0 }}>
                Loading
              </Tag>
            ) : null}
          </Space>
        )
      },
      {
        title: "Timeline",
        key: "timeline",
        width: 190,
        render: (_, row) => {
          const items = [];
          if (row.createdAtUtc) {
            items.push(
              <Text key="c" type="secondary" style={{ fontSize: "12px", display: "block" }}>
                Tạo: {formatDateTimeShort(row.createdAtUtc)}
              </Text>
            );
          }
          if (row.agvTakenAtUtc) {
            items.push(
              <Text key="a" type="secondary" style={{ fontSize: "12px", display: "block" }}>
                AGV lấy: {formatDateTimeShort(row.agvTakenAtUtc)}
              </Text>
            );
          }
          if (row.productionStartedAtUtc) {
            items.push(
              <Text key="p" type="secondary" style={{ fontSize: "12px", display: "block" }}>
                Bắt đầu SX: {formatDateTimeShort(row.productionStartedAtUtc)}
              </Text>
            );
          }
          const endUtc = row.completedAtUtc ?? row.clearedAtUtc;
          if (endUtc) {
            items.push(
              <Text key="e" type="secondary" style={{ fontSize: "12px", display: "block" }}>
                Kết thúc: {formatDateTimeShort(endUtc)}
              </Text>
            );
          }

          return items.length === 0 ? (
            "-"
          ) : (
            <div style={{ display: "flex", flexDirection: "column", gap: "2px" }}>
              {items}
            </div>
          );
        }
      },
      {
        title: "KPI",
        key: "kpi",
        width: 220,
        render: (_, row) => (
          <Space direction="vertical" size={0}>
            <Text>Mode: {row.mode?.toLowerCase() === "manualload" ? "Manual" : "AGV"}</Text>
            {row.mode?.toLowerCase() === "agv" && (
              <Text>AGV lấy: {formatSeconds(row.agvPickupDurationSeconds)}</Text>
            )}
            <Text>Sản xuất: {formatSeconds(row.productionDurationSeconds)}</Text>
            <Text>Tổng thời gian: {formatSeconds(row.lifecycleDurationSeconds)}</Text>
          </Space>
        )
      },
      {
        title: "Order con",
        key: "orders",
        render: (_, row) =>
          (row.orders ?? []).length === 0 ? (
            "-"
          ) : (
            <Space direction="vertical" size={4}>
              {row.orders.map((order) => {
                const reportModel = order.reportModelName || "";
                const articleName = order.modelName || "";
                const isDuplicate =
                  !reportModel ||
                  !articleName ||
                  reportModel.trim().toLowerCase() === articleName.trim().toLowerCase();

                const modelArticleText = isDuplicate
                  ? `Article: ${articleName || reportModel || "-"}`
                  : `Article: ${articleName} | Model: ${reportModel}`;

                return (
                  <Tag key={`${row.declarationId}-${order.orderSequence}`} style={{ marginInlineEnd: 0 }}>
                    #{order.orderSequence} | Order: {order.orderId || "-"} | {modelArticleText} | Qty: {order.quantity ?? "-"} | {getStatusLabel(order.status)}
                  </Tag>
                );
              })}
            </Space>
          )
      }
    ],
    []
  );

  const filteredItems = useMemo(() => {
    const items = report?.items ?? [];
    const orderKeyword = searchOrder.trim().toLowerCase();
    const modelKeyword = searchModel.trim().toLowerCase();

    if (!orderKeyword && !modelKeyword) {
      return items;
    }

    return items
      .map((item) => {
        const matchedOrders = (item.orders ?? []).filter((order) => {
          const orderId = (order.orderId || "").toLowerCase();
          const articleId = (order.modelName || "").toLowerCase();
          const reportModelName = (order.reportModelName || "").toLowerCase();

          const orderMatched = !orderKeyword || orderId.includes(orderKeyword);
          const modelMatched = !modelKeyword || articleId.includes(modelKeyword) || reportModelName.includes(modelKeyword);
          return orderMatched && modelMatched;
        });

        return {
          ...item,
          orders: matchedOrders
        };
      })
      .filter((item) => item.orders.length > 0);
  }, [report, searchOrder, searchModel]);

  const quickStats = useMemo(() => {
    const orders = filteredItems.flatMap((item) => item.orders ?? []);
    const totalOrders = orders.length;
    const totalProducts = orders.reduce((sum, order) => sum + (order.quantity || 0), 0);
    const runningProducts = filteredItems
      .filter((item) => item.status === "InProduction")
      .flatMap((item) => item.orders ?? [])
      .reduce((sum, order) => sum + (order.quantity || 0), 0);
    const completedProducts = filteredItems
      .filter((item) => item.status === "Completed")
      .flatMap((item) => item.orders ?? [])
      .reduce((sum, order) => sum + (order.quantity || 0), 0);
    const clearedProducts = filteredItems
      .filter((item) => item.status === "Cleared")
      .flatMap((item) => item.orders ?? [])
      .reduce((sum, order) => sum + (order.quantity || 0), 0);
    const cancelledProducts = filteredItems
      .filter((item) => item.status === "Cancelled")
      .flatMap((item) => item.orders ?? [])
      .reduce((sum, order) => sum + (order.quantity || 0), 0);

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
    const items = filteredItems ?? [];
    if (items.length === 0) {
      message.warning("Không có dữ liệu để xuất.");
      return;
    }

    const headers = [
      "Mã kệ",
      "Kệ số",
      "Chế độ nạp",
      "Trạng thái kệ",
      "Thời gian tạo",
      "Thời gian AGV lấy",
      "Thời gian Bắt đầu SX",
      "Thời gian Kết thúc",
      "KPI AGV lấy (giây)",
      "KPI Sản xuất (giây)",
      "KPI Tổng thời gian (giây)",
      "STT Order con",
      "Mã Order con",
      "Article ID",
      "Model Name",
      "Số lượng",
      "Trạng thái Order con"
    ];

    const rows = [];

    items.forEach((item) => {
      const endUtc = item.completedAtUtc ?? item.clearedAtUtc;
      const modeText = item.mode?.toLowerCase() === "manualload" ? "Manual" : "AGV";
      const statusText = getStatusLabel(item.status);
      const createdAt = formatDateTime(item.createdAtUtc);
      const agvTakenAt = formatDateTime(item.agvTakenAtUtc);
      const productionStartedAt = formatDateTime(item.productionStartedAtUtc);
      const endAt = formatDateTime(endUtc);

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
            onChange={setMachineId}
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
