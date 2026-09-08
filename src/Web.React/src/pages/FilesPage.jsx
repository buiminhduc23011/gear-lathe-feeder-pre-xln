import React, { useEffect, useMemo, useState } from "react";
import dayjs from "dayjs";
import {
  Button,
  DatePicker,
  Form,
  Input,
  Popconfirm,
  Space,
  Table,
  Tag,
  Typography
} from "antd";
import {
  ClearOutlined,
  DeleteOutlined,
  DownloadOutlined,
  FileExcelOutlined,
  SearchOutlined,
  UploadOutlined
} from "@ant-design/icons";
import AppModal from "../components/AppModal";
import PageHeader from "../components/ui/PageHeader";
import SectionCard from "../components/ui/SectionCard";
import { API_ENDPOINTS, apiClient, getApiErrorMessage } from "../config/api";
import { useAuth } from "../contexts/AuthContext";
import { showErrorMessage, showSuccessMessage } from "../utils/appMessage";

const { Text } = Typography;

function FilesPage() {
  const { currentUser } = useAuth();
  const [files, setFiles] = useState([]);
  const [loading, setLoading] = useState(false);
  const [uploading, setUploading] = useState(false);
  const [modalOpen, setModalOpen] = useState(false);
  const [selectedFile, setSelectedFile] = useState(null);
  const [nameKeyword, setNameKeyword] = useState("");
  const [fromDate, setFromDate] = useState(dayjs().startOf("day"));
  const [toDate, setToDate] = useState(dayjs().endOf("day"));
  const [form] = Form.useForm();

  const canManageFiles = currentUser && ["Admin", "Technician"].includes(currentUser.role);

  const loadFiles = async () => {
    setLoading(true);
    try {
      const response = await apiClient.get(API_ENDPOINTS.files);
      setFiles(response.data);
    } catch (error) {
      showErrorMessage(getApiErrorMessage(error, "Không thể tải danh sách file."));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadFiles();
  }, []);

  const filteredFiles = useMemo(
    () =>
      filterFiles(files, {
        nameKeyword,
        fromDate: fromDate ? fromDate.format("YYYY-MM-DD") : "",
        toDate: toDate ? toDate.format("YYYY-MM-DD") : ""
      }),
    [files, fromDate, nameKeyword, toDate]
  );

  const hasActiveFilters = Boolean(nameKeyword.trim() || fromDate || toDate);

  const summary = useMemo(() => {
    const totalSize = filteredFiles.reduce((sum, file) => sum + (file.size || 0), 0);
    return {
      total: files.length,
      displayed: filteredFiles.length,
      totalSize,
      latestUpload: files[0]?.uploadedAtUtc || null
    };
  }, [files, filteredFiles]);

  const closeModal = () => {
    setModalOpen(false);
    setUploading(false);
    setSelectedFile(null);
    form.resetFields();
  };

  const openModal = () => {
    form.resetFields();
    setSelectedFile(null);
    setModalOpen(true);
  };

  const handleUpload = async (values) => {
    if (!selectedFile) {
      showErrorMessage("Vui lòng chọn file cần upload.");
      return;
    }

    setUploading(true);
    try {
      const formData = new FormData();
      formData.append("file", selectedFile);
      if (values.machineName) {
        formData.append("machineName", values.machineName);
      }
      if (values.manufacturer) {
        formData.append("manufacturer", values.manufacturer);
      }
      formData.append("sentAtUtc", new Date().toISOString());

      await apiClient.post(`${API_ENDPOINTS.files}/upload`, formData, {
        headers: {
          "Content-Type": "multipart/form-data"
        }
      });

      showSuccessMessage("Đã upload file lên server.");
      closeModal();
      await loadFiles();
    } catch (error) {
      showErrorMessage(getApiErrorMessage(error, "Không thể upload file."));
    } finally {
      setUploading(false);
    }
  };

  const handleDownload = async (file) => {
    try {
      const response = await apiClient.get(`${API_ENDPOINTS.files}/${file.id}/download`, {
        responseType: "blob"
      });

      const blobUrl = window.URL.createObjectURL(new Blob([response.data]));
      const anchor = document.createElement("a");
      anchor.href = blobUrl;
      anchor.download = file.originalFileName;
      document.body.appendChild(anchor);
      anchor.click();
      anchor.remove();
      window.URL.revokeObjectURL(blobUrl);
    } catch (error) {
      showErrorMessage(getApiErrorMessage(error, "Không thể tải file."));
    }
  };

  const handleDelete = async (fileId) => {
    try {
      await apiClient.delete(`${API_ENDPOINTS.files}/${fileId}`);
      showSuccessMessage("Đã xóa file.");
      await loadFiles();
    } catch (error) {
      showErrorMessage(getApiErrorMessage(error, "Không thể xóa file."));
    }
  };

  const handleClearFilters = () => {
    setNameKeyword("");
    setFromDate(null);
    setToDate(null);
  };

  const handleExportExcel = () => {
    if (filteredFiles.length === 0) {
      showErrorMessage("Không có dữ liệu để xuất Excel.");
      return;
    }

    exportFilesToExcel(filteredFiles);
    showSuccessMessage(`Đã xuất dữ liệu ra file Excel.`);
  };

  const columns = [
    {
      title: "Tên file",
      dataIndex: "originalFileName",
      key: "originalFileName",
      render: (_, record) => (
        <div>
          <Text strong>{record.originalFileName}</Text>
          <div>
            <Text type="secondary" style={{ fontSize: 12 }}>
              {record.storedFileName}
            </Text>
          </div>
        </div>
      )
    },
    {
      title: "Dung lượng",
      dataIndex: "size",
      key: "size",
      render: (value) => formatBytes(value)
    },
    {
      title: "Máy tải lên",
      key: "machineName",
      render: (_, record) => (
        <div>
          <Text>{record.machineName || "—"}</Text>
          <div>
            <Text type="secondary" style={{ fontSize: 12 }}>
              {record.manufacturer || "Không rõ hãng"}
            </Text>
          </div>
        </div>
      )
    },
    {
      title: "Ngày SX",
      dataIndex: "sentAtUtc",
      key: "sentAtUtc",
      width: 180,
      render: (_, record) => formatDateTime(getProductionDateValue(record))
    },
    {
      title: "Tải lên lúc",
      dataIndex: "uploadedAtUtc",
      key: "uploadedAtUtc",
      render: (value) => formatDateTime(value)
    },
    {
      title: "Nguồn",
      dataIndex: "uploadSource",
      key: "uploadSource",
      render: (value) => (
        <Tag color={value === "WebManual" ? "blue" : "default"}>
          {value === "WebManual" ? "Thủ công" : "Thiết bị"}
        </Tag>
      )
    },
    {
      title: "Người tải",
      dataIndex: "uploadedByUsername",
      key: "uploadedByUsername",
      render: (value) => value || "—"
    },
    {
      title: "Thao tác",
      key: "actions",
      render: (_, record) => (
        <Space wrap>
          <Button type="link" icon={<DownloadOutlined />} onClick={() => handleDownload(record)}>
            Tải xuống
          </Button>
          {record.canDelete ? (
            <Popconfirm
              title="Xóa file này?"
              description="File sẽ bị xóa vĩnh viễn và không thể khôi phục lại."
              onConfirm={() => handleDelete(record.id)}
            >
              <Button type="link" danger icon={<DeleteOutlined />}>
                Xóa
              </Button>
            </Popconfirm>
          ) : null}
        </Space>
      )
    }
  ];

  return (
    <Space direction="vertical" size={14} style={{ display: "flex" }}>
      <PageHeader
        title="Tệp tải lên"
        description="Quản lý và theo dõi các tệp tin tải lên hệ thống."
        meta={(
          <Space wrap>
            <Tag>Tổng file: {summary.total}</Tag>
            <Tag>Số lượng: {summary.displayed}</Tag>
            <Tag>Tổng dung lượng: {formatBytes(summary.totalSize)}</Tag>
            <Tag>File mới nhất: {summary.latestUpload ? formatDateTime(summary.latestUpload) : "Chưa có file"}</Tag>
          </Space>
        )}
        actions={canManageFiles ? (
          <Button type="primary" icon={<UploadOutlined />} onClick={openModal}>
            Upload
          </Button>
        ) : null}
      />

      <SectionCard
        title="Danh sách file"
        description="Lịch sử tệp tin nhận được từ thiết bị và web."
        toolbar={(
          <Button icon={<FileExcelOutlined />} disabled={filteredFiles.length === 0} onClick={handleExportExcel}>
            Xuất Excel
          </Button>
        )}
      >
        <Space
          wrap
          style={{
            marginBottom: 20
          }}
        >
          <Input
            allowClear
            prefix={<SearchOutlined />}
            value={nameKeyword}
            onChange={(event) => setNameKeyword(event.target.value)}
            placeholder="Tên file / máy"
            style={{ width: 340, maxWidth: "100%" }}
          />

          <Space size={4}>
            <Text type="secondary">Từ:</Text>
            <DatePicker
              value={fromDate}
              onChange={setFromDate}
              format="DD/MM/YYYY"
              placeholder="Chọn ngày"
              style={{ width: 130 }}
            />
          </Space>

          <Space size={4}>
            <Text type="secondary">Đến:</Text>
            <DatePicker
              value={toDate}
              onChange={setToDate}
              format="DD/MM/YYYY"
              placeholder="Chọn ngày"
              style={{ width: 130 }}
            />
          </Space>

          <Button icon={<ClearOutlined />} disabled={!hasActiveFilters} onClick={handleClearFilters} style={{ marginLeft: 8 }}>
            Xóa
          </Button>
        </Space>

        <Table
          rowKey="id"
          columns={columns}
          dataSource={filteredFiles}
          loading={loading}
          size="small"
          pagination={{ pageSize: 10, showSizeChanger: false, size: "small" }}
          scroll={{ x: 1080 }}
        />
      </SectionCard>

      <AppModal
        title="Upload file thủ công"
        open={modalOpen}
        width={720}
        confirmLoading={uploading}
        onCancel={closeModal}
        onOk={() => form.submit()}
        okText="Upload file"
      >
        <Form form={form} layout="vertical" preserve={false} onFinish={handleUpload}>
          <Form.Item label="Tên máy" name="machineName" rules={[{ required: true, message: "Vui lòng nhập tên máy" }]}>
            <Input placeholder="Nhập tên máy" />
          </Form.Item>

          <Form.Item label="Hãng sản xuất" name="manufacturer" rules={[{ required: true, message: "Vui lòng nhập hãng sản xuất" }]}>
            <Input placeholder="Nhập hãng sản xuất" />
          </Form.Item>

          <Form.Item
            label="Chọn file"
            required
            extra="File sẽ được lưu qua FilesController và xuất hiện ngay trong danh sách sau khi upload thành công."
          >
            <input
              type="file"
              style={{ width: "100%" }}
              onChange={(event) => setSelectedFile(event.target.files?.[0] || null)}
            />
          </Form.Item>
        </Form>
      </AppModal>
    </Space>
  );
}

function formatBytes(size) {
  if (!size) {
    return "0 B";
  }

  const units = ["B", "KB", "MB", "GB", "TB"];
  const unitIndex = Math.min(Math.floor(Math.log(size) / Math.log(1024)), units.length - 1);
  const value = size / 1024 ** unitIndex;
  const displayValue =
    unitIndex === 0 || Number.isInteger(value) || value >= 100
      ? value.toFixed(0)
      : value.toFixed(1);

  return `${displayValue} ${units[unitIndex]}`;
}

function formatDateTime(value) {
  if (!value) {
    return "—";
  }

  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return "—";
  }

  return date.toLocaleString("vi-VN", {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
    second: "2-digit"
  });
}

function filterFiles(files, filters) {
  const nameKeyword = normalizeText(filters.nameKeyword);

  return files.filter((file) => {
    const nameMatched =
      !nameKeyword ||
      [
        file.originalFileName,
        file.storedFileName,
        file.machineName,
        file.manufacturer,
        file.uploadedByUsername
      ].some((value) => normalizeText(value).includes(nameKeyword));

    return nameMatched && isDateInRange(getProductionDateValue(file), filters.fromDate, filters.toDate);
  });
}

function normalizeText(value) {
  return `${value || ""}`.trim().toLowerCase();
}

function getProductionDateValue(file) {
  return file.sentAtUtc || file.uploadedAtUtc;
}

function isDateInRange(value, fromDate, toDate) {
  if (!fromDate && !toDate) {
    return true;
  }

  const timestamp = new Date(value).getTime();
  if (Number.isNaN(timestamp)) {
    return false;
  }

  const start = fromDate ? getLocalDayStart(fromDate) : Number.NEGATIVE_INFINITY;
  const end = toDate ? getLocalDayEnd(toDate) : Number.POSITIVE_INFINITY;
  return timestamp >= start && timestamp <= end;
}

function getLocalDayStart(dateText) {
  const [year, month, day] = dateText.split("-").map(Number);
  return new Date(year, month - 1, day, 0, 0, 0, 0).getTime();
}

function getLocalDayEnd(dateText) {
  const [year, month, day] = dateText.split("-").map(Number);
  return new Date(year, month - 1, day, 23, 59, 59, 999).getTime();
}

function exportFilesToExcel(files) {
  const headers = [
    "STT",
    "Tên file",
    "Dung lượng",
    "Máy tải lên",
    "Hãng sản xuất",
    "Ngày sản xuất",
    "Tải lên lúc",
    "Nguồn",
    "Người tải"
  ];

  const rows = files.map((file, index) => [
    index + 1,
    file.originalFileName,
    formatBytes(file.size),
    file.machineName || "—",
    file.manufacturer || "—",
    formatDateTime(getProductionDateValue(file)),
    formatDateTime(file.uploadedAtUtc),
    formatUploadSource(file.uploadSource),
    file.uploadedByUsername || "—"
  ]);

  const tableRows = [headers, ...rows]
    .map((row, rowIndex) => {
      const cellTag = rowIndex === 0 ? "th" : "td";
      return `<tr>${row.map((value) => `<${cellTag}>${escapeHtml(value)}</${cellTag}>`).join("")}</tr>`;
    })
    .join("");

  const html = `<!doctype html>
<html>
<head>
  <meta charset="UTF-8" />
  <style>
    table { border-collapse: collapse; font-family: Arial, sans-serif; font-size: 12px; }
    th, td { border: 1px solid #b7c4d1; padding: 6px 8px; mso-number-format: "\\@"; }
    th { background: #e8eef7; font-weight: 700; }
  </style>
</head>
<body>
  <table>${tableRows}</table>
</body>
</html>`;

  const blob = new Blob(["\ufeff", html], {
    type: "application/vnd.ms-excel;charset=utf-8"
  });
  const blobUrl = window.URL.createObjectURL(blob);
  const anchor = document.createElement("a");
  anchor.href = blobUrl;
  anchor.download = `danh-sach-file-${formatFileTimestamp(new Date())}.xls`;
  document.body.appendChild(anchor);
  anchor.click();
  anchor.remove();
  window.URL.revokeObjectURL(blobUrl);
}

function formatUploadSource(value) {
  return value === "WebManual" ? "Thủ công" : "Thiết bị";
}

function escapeHtml(value) {
  return `${value ?? ""}`
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;")
    .replace(/"/g, "&quot;")
    .replace(/'/g, "&#39;");
}

function formatFileTimestamp(date) {
  const pad = (value) => `${value}`.padStart(2, "0");
  return [
    date.getFullYear(),
    pad(date.getMonth() + 1),
    pad(date.getDate()),
    "-",
    pad(date.getHours()),
    pad(date.getMinutes()),
    pad(date.getSeconds())
  ].join("");
}

export default FilesPage;
