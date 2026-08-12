import React, { useCallback, useEffect, useMemo, useRef, useState } from "react";
import {
  Button,
  Descriptions,
  Form,
  Input,
  InputNumber,
  Modal,
  Popconfirm,
  Select,
  Space,
  Switch,
  Table,
  Tag,
  Tooltip,
  Typography
} from "antd";
import {
  CheckCircleOutlined,
  CloudDownloadOutlined,
  CloudUploadOutlined,
  DeleteOutlined,
  EditOutlined,
  ExclamationCircleOutlined,
  HistoryOutlined,
  PlusOutlined,
  SearchOutlined,
  StopOutlined
} from "@ant-design/icons";
import AppModal from "../../components/AppModal";
import PageHeader from "../../components/ui/PageHeader";
import SectionCard from "../../components/ui/SectionCard";
import { API_ENDPOINTS, apiClient, getApiErrorMessage } from "../../config/api";
import { showErrorMessage, showSuccessMessage } from "../../utils/appMessage";

const { Paragraph, Text } = Typography;

const JIG_SUPPLY_TYPE_OPTIONS = [
  { value: 0, label: "0: Không xác định" },
  { value: 1, label: "1: Jig Phi 20" },
  { value: 2, label: "2: Jig Phi 30" },
  { value: 3, label: "3: Jig Phi 40" },
  { value: 4, label: "4: Jig có thể điều chỉnh" }
];

const ROBOT_FIELDS = [
  { key: "inputBlankThickness", label: "Độ dày Phôi đầu vào", type: "real" },
  { key: "outerFinishedDiameter", label: "Đường kính ngoài phôi thành phẩm", type: "real" },
  { key: "op1TurnedThickness", label: "Độ dày phôi sau tiện OP1", type: "real" },
  { key: "finishedThickness", label: "Độ dày Phôi thành phẩm", type: "real" },
  { key: "pickDropZOffset", label: "Ofset tọa độ Z gắp thả hàng", type: "real" },
  { key: "chuckStepDepth", label: "Chiều sâu bậc mâm cặp", type: "real" },
  { key: "innerFinishedDiameter", label: "Đường kính trong phôi thành phẩm", type: "real" },
  { key: "innerDiameterToGDiameterDistance", label: "KC đường kính trong đến G", type: "real" },
  { key: "magnetCount", label: "Số nam châm sử dụng", type: "int" },
  {
    key: "jigSupplyType",
    label: "Loại Jig cấp hàng",
    type: "select",
    options: JIG_SUPPLY_TYPE_OPTIONS
  }
];



export const MODEL_METADATA_FIELDS = [
  { key: "itemType", label: "Loại hàng", type: "text" },
  { key: "machiningProgram", label: "Chương trình gia công", type: "int" },
  { key: "spare1", label: "Spare 1", type: "text" },
  { key: "spare2", label: "Spare 2", type: "text" },
  {
    key: "orderInput",
    label: "Nhập order",
    type: "select",
    options: [
      { value: 0, label: "Không nhập" },
      { value: 1, label: "Nhập" }
    ]
  },
  { key: "inputBlankDiameter", label: "Đường kính phôi đầu vào", type: "real" },
  { key: "op2ChuckSleeveDepth", label: "Chiều sâu bạc mâm cặp OP2", type: "real" }
];

const EXCEL_VALIDATION_CODE = "MODEL_EXCEL_VALIDATION_FAILED";

const HIDDEN_MODEL_METADATA_KEYS = ["spare1", "spare2"];

const ACTION_COLORS = {
  Created: "green",
  Updated: "blue",
  Deleted: "red",
  Enabled: "cyan",
  Disabled: "orange"
};

const ACTION_LABELS = {
  Created: "Tạo mới",
  Updated: "Cập nhật",
  Deleted: "Xóa",
  Enabled: "Bật",
  Disabled: "Tắt"
};

function buildFieldDefaults(fields) {
  const defaults = {};
  fields.forEach((field) => {
    defaults[field.key] = field.type === "int" || field.type === "select" ? 0 : 0.0;
  });
  return defaults;
}

function buildFieldData(fields, values = {}) {
  const data = buildFieldDefaults(fields);
  fields.forEach((field) => {
    if (Object.prototype.hasOwnProperty.call(values, field.key)) {
      data[field.key] = values[field.key];
    }
  });
  return data;
}

function ModelFormField({ field, name }) {
  const rules = field.key === "jigSupplyType"
    ? [{
        validator: (_, value) => Number(value) > 0
          ? Promise.resolve()
          : Promise.reject(new Error("Vui lòng chọn loại Jig cấp hàng."))
      }]
    : undefined;

  return (
    <Form.Item label={field.label} name={name} rules={rules}>
      {field.type === "select" ? (
        <Select size="small" options={field.options} />
      ) : field.type === "text" ? (
        <Input size="small" />
      ) : (
        <InputNumber
          size="small"
          style={{ width: "100%" }}
          precision={field.type === "real" ? 3 : 0}
          step={field.type === "real" ? 0.001 : 1}
        />
      )}
    </Form.Item>
  );
}

function MetadataFormItems() {
  const op2ChuckSleeveDepthField = MODEL_METADATA_FIELDS.find((field) => field.key === "op2ChuckSleeveDepth");
  const visibleFields = MODEL_METADATA_FIELDS.filter(
    (field) => !HIDDEN_MODEL_METADATA_KEYS.includes(field.key) && field.key !== "op2ChuckSleeveDepth"
  );
  const topFields = visibleFields.filter((field) => field.key !== "inputBlankDiameter");
  const inputBlankDiameterField = visibleFields.find((field) => field.key === "inputBlankDiameter");

  return (
    <>
      {HIDDEN_MODEL_METADATA_KEYS.map((key) => (
        <Form.Item name={key} key={key} hidden>
          <Input />
        </Form.Item>
      ))}
      <div className="model-profile-fields-grid model-profile-metadata-grid">
        {topFields.map((field) => (
          <ModelFormField field={field} name={field.key} key={field.key} />
        ))}
        <div className="model-profile-fields-break" aria-hidden="true" />
        {inputBlankDiameterField ? (
          <ModelFormField field={inputBlankDiameterField} name="inputBlankDiameter" key="inputBlankDiameter" />
        ) : null}
        {ROBOT_FIELDS.map((field) => (
          <ModelFormField field={field} name={["robotData", field.key]} key={field.key} />
        ))}
        {op2ChuckSleeveDepthField ? (
          <ModelFormField field={op2ChuckSleeveDepthField} name="op2ChuckSleeveDepth" key="op2ChuckSleeveDepth" />
        ) : null}
      </div>
    </>
  );
}

function SnapshotDataView({ data, fields }) {
  if (!data || Object.keys(data).length === 0) {
    return <Text type="secondary" italic>Không có dữ liệu</Text>;
  }

  return (
    <Descriptions column={3} size="small" bordered>
      {fields.map((field) => {
        const value = data[field.key];
        let displayValue = value ?? "-";
        if (field.type === "select" && field.options) {
          const option = field.options.find((item) => item.value === value);
          displayValue = option ? option.label : displayValue;
        } else if (typeof value === "number") {
          displayValue = field.type === "real" ? Number(value).toFixed(3) : value;
        }

        return (
          <Descriptions.Item label={field.label} key={field.key}>
            {displayValue}
          </Descriptions.Item>
        );
      })}
    </Descriptions>
  );
}

function formatOrderInput(value) {
  if (value == null) return "Không nhập";
  if (value === 0 || value === "0") return "Không nhập";
  if (value === 1 || value === "1") return "Nhập";
  return "-";
}

function columnTitle(...lines) {
  return (
    <span style={{ display: "inline-block", lineHeight: 1.15, whiteSpace: "normal" }}>
      {lines.map((line, index) => (
        <React.Fragment key={line}>
          {index > 0 ? <br /> : null}
          {line}
        </React.Fragment>
      ))}
    </span>
  );
}

export function buildModelFormValues(model) {
  if (!model) {
    return {
      modelName: "",
      itemType: undefined,
      machiningProgram: 0,
      spare1: undefined,
      spare2: undefined,
      outerShaftDiameter: 0,
      diameterOp1: 0,
      diameterOp2: 0,
      inputBlankDiameter: 0,
      op2ChuckSleeveDepth: 0,
      trayType: 0,
      orderInput: 1,
      robotData: buildFieldDefaults(ROBOT_FIELDS)
    };
  }

  return {
    modelName: model.modelName,
    itemType: model.itemType ?? undefined,
    machiningProgram: model.machiningProgram ?? 0,
    spare1: model.spare1 ?? undefined,
    spare2: model.spare2 ?? undefined,
    outerShaftDiameter: model.outerShaftDiameter ?? 0,
    diameterOp1: model.diameterOp1 ?? 0,
    diameterOp2: model.diameterOp2 ?? 0,
    inputBlankDiameter: model.inputBlankDiameter ?? 0,
    op2ChuckSleeveDepth: model.op2ChuckSleeveDepth ?? 0,
    trayType: model.trayType ?? 0,
    orderInput: model.orderInput ?? 1,
    robotData: buildFieldData(ROBOT_FIELDS, model.robotData)
  };
}

export function buildModelPayload(values, modelName) {
  return {
    modelName,
    itemType: values.itemType || null,
    machiningProgram: values.machiningProgram ?? 0,
    spare1: values.spare1 || null,
    spare2: values.spare2 || null,
    outerShaftDiameter: values.outerShaftDiameter ?? 0,
    diameterOp1: values.diameterOp1 ?? 0,
    diameterOp2: values.diameterOp2 ?? 0,
    inputBlankDiameter: values.inputBlankDiameter ?? 0,
    op2ChuckSleeveDepth: values.op2ChuckSleeveDepth ?? 0,
    trayType: values.trayType ?? 0,
    orderInput: values.orderInput ?? 1,
    robotData: buildFieldData(ROBOT_FIELDS, values.robotData),
    line1Data: {},
    line2Data: {}
  };
}

function hasPositiveNumber(value) {
  const numericValue = Number(value);
  return Number.isFinite(numericValue) && numericValue > 0;
}

function getActivationMissingFields(model) {
  const missingFields = [];
  if (!hasPositiveNumber(model?.inputBlankDiameter)) {
    missingFields.push("Đường kính phôi đầu vào");
  }
  if (!hasPositiveNumber(model?.robotData?.inputBlankThickness)) {
    missingFields.push("Độ dày Phôi đầu vào");
  }

  return missingFields;
}

function buildActivationValidationMessage(model) {
  const missingFields = getActivationMissingFields(model);
  if (missingFields.length === 0) {
    return "";
  }

  return `Chỉ có thể Active khi đã nhập đủ và > 0 cho: ${missingFields.join(", ")}.`;
}

function isExcelValidationError(error) {
  return error?.response?.data?.code === EXCEL_VALIDATION_CODE && Array.isArray(error.response.data.errors);
}

function showExcelValidationModal(payload) {
  const errors = payload.errors || [];
  Modal.error({
    title: payload.message || "File Excel có dữ liệu model không hợp lệ.",
    width: 980,
    okText: "Đã hiểu",
    content: (
      <Table
        size="small"
        rowKey={(record, index) => `${record.articleId || "-"}-${record.field || "-"}-${index}`}
        dataSource={errors}
        pagination={{ pageSize: 6, showSizeChanger: false, size: "small" }}
        scroll={{ x: 900 }}
        expandable={{
          rowExpandable: (record) => Array.isArray(record.values) && record.values.length > 0,
          expandedRowRender: (record) => (
            <Table
              size="small"
              rowKey={(item, index) => `${item.sheet}-${item.row}-${item.column}-${index}`}
              dataSource={record.values || []}
              pagination={false}
              columns={[
                { title: "Sheet", dataIndex: "sheet", key: "sheet" },
                { title: "Dòng", dataIndex: "row", key: "row", width: 80 },
                { title: "Cột", dataIndex: "column", key: "column", width: 80 },
                { title: "Giá trị", dataIndex: "value", key: "value" },
                { title: "Sau chuẩn hóa", dataIndex: "normalizedValue", key: "normalizedValue" }
              ]}
            />
          )
        }}
        columns={[
          { title: "Article ID", dataIndex: "articleId", key: "articleId", width: 140, render: (value) => value || "-" },
          { title: "Field", dataIndex: "field", key: "field", width: 170 },
          { title: "Sheet", dataIndex: "sheet", key: "sheet", width: 150, render: (value) => value || "-" },
          { title: "Dòng", dataIndex: "row", key: "row", width: 80, render: (value) => value || "-" },
          { title: "Cột", dataIndex: "column", key: "column", width: 80, render: (value) => value || "-" },
          { title: "Giá trị", dataIndex: "value", key: "value", width: 160, render: (value) => value || "-" },
          { title: "Lỗi", dataIndex: "message", key: "message" }
        ]}
      />
    )
  });
}

function formatDateTime(value) {
  if (!value) {
    return "-";
  }
  return new Date(value).toLocaleString("vi-VN");
}

function ModelProfiles() {
  const [machines, setMachines] = useState([]);
  const [selectedMachineId, setSelectedMachineId] = useState(null);
  const [models, setModels] = useState([]);
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [modalOpen, setModalOpen] = useState(false);
  const [editingModel, setEditingModel] = useState(null);
  const [historyModalOpen, setHistoryModalOpen] = useState(false);
  const [snapshots, setSnapshots] = useState([]);
  const [historyLoading, setHistoryLoading] = useState(false);
  const [historyModelName, setHistoryModelName] = useState("");
  const [historySearch, setHistorySearch] = useState("");
  const [showDeleted, setShowDeleted] = useState(false);
  const [updatingExcel, setUpdatingExcel] = useState(false);
  const [importingExcel, setImportingExcel] = useState(false);
  const [filterName, setFilterName] = useState("");
  const [filterNameDebounced, setFilterNameDebounced] = useState("");
  const nameTimerRef = useRef(null);
  const updateExcelInputRef = useRef(null);
  const importExcelInputRef = useRef(null);
  const [form] = Form.useForm();

  const handleFilterName = (value) => {
    setFilterName(value);
    clearTimeout(nameTimerRef.current);
    nameTimerRef.current = setTimeout(() => setFilterNameDebounced(value), 500);
  };

  const filteredModels = useMemo(() => {
    return models.filter((model) => {
      if (filterNameDebounced && !model.modelName.toLowerCase().includes(filterNameDebounced.toLowerCase())) {
        return false;
      }
      return true;
    });
  }, [filterNameDebounced, models]);

  useEffect(() => {
    (async () => {
      try {
        const response = await apiClient.get(API_ENDPOINTS.machines);
        setMachines(response.data);
        if (response.data.length > 0) {
          setSelectedMachineId(response.data[0].machineId);
        }
      } catch (error) {
        showErrorMessage(getApiErrorMessage(error, "Không thể tải danh sách máy."));
      }
    })();
  }, []);

  const loadModels = useCallback(async () => {
    if (!selectedMachineId) {
      return;
    }

    setLoading(true);
    try {
      const params = showDeleted ? { includeDeleted: true } : {};
      const response = await apiClient.get(API_ENDPOINTS.machineModels(selectedMachineId), { params });
      setModels(response.data);
    } catch (error) {
      showErrorMessage(getApiErrorMessage(error, "Không thể tải danh sách model."));
    } finally {
      setLoading(false);
    }
  }, [selectedMachineId, showDeleted]);

  useEffect(() => {
    loadModels();
  }, [loadModels]);

  const machineOptions = useMemo(() => (
    machines.map((machine) => ({
      value: machine.machineId,
      label: `${machine.machineName} (${machine.machineCode})`
    }))
  ), [machines]);

  const selectedMachineName = useMemo(() => (
    machineOptions.find((option) => option.value === selectedMachineId)?.label ?? ""
  ), [machineOptions, selectedMachineId]);

  const metrics = useMemo(() => {
    const active = models.filter((model) => !model.isDeleted && model.isEnabled).length;
    const deleted = models.filter((model) => model.isDeleted).length;
    return { total: models.length, active, deleted };
  }, [models]);

  const closeModal = () => {
    setModalOpen(false);
    setSaving(false);
  };

  const openCreate = () => {
    form.resetFields();
    setEditingModel(null);
    setModalOpen(true);
  };

  const openEdit = (model) => {
    form.resetFields();
    setEditingModel(model);
    setModalOpen(true);
  };

  useEffect(() => {
    if (modalOpen && editingModel) {
      form.setFieldsValue(buildModelFormValues(editingModel));
    } else if (modalOpen && !editingModel) {
      form.resetFields();
    }
  }, [modalOpen, editingModel, form]);

  const formValues = useMemo(() => {
    return buildModelFormValues(editingModel);
  }, [editingModel]);

  const handleSubmit = async (values) => {
    setSaving(true);
    try {
      const payload = buildModelPayload(values, editingModel ? editingModel.modelName : values.modelName);

      const baseUrl = API_ENDPOINTS.machineModels(selectedMachineId);
      if (editingModel) {
        await apiClient.put(`${baseUrl}/${editingModel.id}`, payload);
        showSuccessMessage("Đã cập nhật model.");
      } else {
        await apiClient.post(baseUrl, payload);
        showSuccessMessage("Đã tạo model mới.");
      }

      closeModal();
      await loadModels();
    } catch (error) {
      showErrorMessage(getApiErrorMessage(error, "Không thể lưu model."));
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (modelId) => {
    try {
      await apiClient.delete(`${API_ENDPOINTS.machineModels(selectedMachineId)}/${modelId}`);
      showSuccessMessage("Đã xóa model.");
      await loadModels();
    } catch (error) {
      showErrorMessage(getApiErrorMessage(error, "Không thể xóa model."));
    }
  };

  const handleToggleEnabled = async (model, isEnabled) => {
    if (isEnabled) {
      const validationMessage = buildActivationValidationMessage(model);
      if (validationMessage) {
        showErrorMessage(validationMessage);
        return;
      }
    }

    try {
      await apiClient.patch(`${API_ENDPOINTS.machineModels(selectedMachineId)}/${model.id}/enabled`, { isEnabled });
      showSuccessMessage(isEnabled ? "Đã bật model." : "Đã tắt model.");
      await loadModels();
    } catch (error) {
      showErrorMessage(getApiErrorMessage(error, "Không thể cập nhật trạng thái."));
    }
  };

  const openHistory = async (model) => {
    setHistoryModelName(model.modelName);
    setHistoryModalOpen(true);
    setHistoryLoading(true);
    try {
      const response = await apiClient.get(`${API_ENDPOINTS.machineModels(selectedMachineId)}/${model.id}/history`);
      setSnapshots(response.data);
    } catch (error) {
      showErrorMessage(getApiErrorMessage(error, "Không thể tải lịch sử."));
    } finally {
      setHistoryLoading(false);
    }
  };

  const handleExport = async () => {
    try {
      const response = await apiClient.get(`${API_ENDPOINTS.machineModels(selectedMachineId)}/export`, { responseType: "blob" });
      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement("a");
      link.href = url;
      link.setAttribute("download", `ModelProfiles_Machine${selectedMachineId}.xlsx`);
      document.body.appendChild(link);
      link.click();
      link.remove();
      window.URL.revokeObjectURL(url);
      showSuccessMessage("Đã xuất file Excel.");
    } catch (error) {
      showErrorMessage(getApiErrorMessage(error, "Không thể xuất Excel."));
    }
  };

  const handleExcelFileChange = async (event, mode) => {
    const file = event.target.files?.[0];
    event.target.value = "";
    if (!file) {
      return;
    }

    const isReplaceMode = mode === "replace";
    if (isReplaceMode) {
      setImportingExcel(true);
    } else {
      setUpdatingExcel(true);
    }

    try {
      const formData = new FormData();
      formData.append("file", file);
      const action = isReplaceMode ? "import" : "update-excel";
      await apiClient.post(`${API_ENDPOINTS.machineModels(selectedMachineId)}/${action}`, formData, {
        headers: { "Content-Type": "multipart/form-data" }
      });
      showSuccessMessage(
        isReplaceMode
          ? "Đã import Excel và thay thế toàn bộ danh sách model."
          : "Đã update Excel: thêm/sửa các model có trong file."
      );
      await loadModels();
    } catch (error) {
      if (isExcelValidationError(error)) {
        showExcelValidationModal(error.response.data);
      } else {
        showErrorMessage(getApiErrorMessage(error, isReplaceMode ? "Không thể import Excel." : "Không thể update Excel."));
      }
    } finally {
      setImportingExcel(false);
      setUpdatingExcel(false);
    }
  };

  const confirmImportExcel = () => {
    Modal.confirm({
      title: "Import thay thế toàn bộ model",
      icon: <ExclamationCircleOutlined style={{ color: "#dc2626" }} />,
      content: (
        <Space direction="vertical" size={4}>
          <Text>File Excel phải là danh sách đầy đủ.</Text>
          <Text type="danger">Model không có trong file sẽ bị xóa.</Text>
        </Space>
      ),
      centered: true,
      width: 420,
      okText: "Chọn file import",
      cancelText: "Hủy",
      okButtonProps: { danger: true },
      onOk: () => importExcelInputRef.current?.click()
    });
  };

  const columns = [
    {
      title: "Article ID",
      dataIndex: "modelName",
      key: "modelName",
      fixed: "left",
      width: 170,
      render: (value) => <Text strong>{value}</Text>
    },
    {
      title: "Loại hàng",
      dataIndex: "itemType",
      key: "itemType",
      width: 130,
      render: (value) => value || "-"
    },
    {
      title: columnTitle("Chương trình", "gia công"),
      dataIndex: "machiningProgram",
      key: "machiningProgram",
      width: 130,
      render: (value) => value ?? 0
    },
    {
      title: "Nhập order",
      dataIndex: "orderInput",
      key: "orderInput",
      width: 120,
      render: (value) => formatOrderInput(value)
    },
    {
      title: columnTitle("Đường kính", "phôi đầu vào"),
      dataIndex: "inputBlankDiameter",
      key: "inputBlankDiameter",
      width: 140,
      render: (value) => value ?? 0
    },
    {
      title: columnTitle("Chiều sâu", "bạc OP2"),
      dataIndex: "op2ChuckSleeveDepth",
      key: "op2ChuckSleeveDepth",
      width: 135,
      render: (value) => value ?? 0
    },
    {
      title: "Người tạo",
      dataIndex: "createdByUsername",
      key: "createdByUsername",
      render: (value) => value || "-"
    },
    {
      title: columnTitle("Lần sửa", "cuối"),
      key: "updatedAtUtc",
      width: 170,
      render: (_, record) => (
        <span>
          {formatDateTime(record.updatedAtUtc)}
          {record.updatedByUsername ? (
            <Text type="secondary" style={{ marginLeft: 6 }}>
              bởi {record.updatedByUsername}
            </Text>
          ) : null}
        </span>
      )
    },
    {
      title: "Trạng thái",
      key: "isEnabled",
      width: 150,
      render: (_, record) => (
        record.isDeleted ? null : (
          <Space size={6}>
            <Switch size="small" checked={record.isEnabled} onChange={(checked) => handleToggleEnabled(record, checked)} />
            {record.isEnabled ? (
              <Tag icon={<CheckCircleOutlined />} color="success">Active</Tag>
            ) : (
              <Tag icon={<StopOutlined />} color="default">Deactive</Tag>
            )}
          </Space>
        )
      )
    },
    {
      title: "Thao tác",
      key: "actions",
      width: 128,
      render: (_, record) => (
        record.isDeleted ? (
          <Space size={4}>
            <Tag color="red">Đã xóa</Tag>
            <Tooltip title="Lịch sử">
              <Button type="text" size="small" icon={<HistoryOutlined />} onClick={() => openHistory(record)} />
            </Tooltip>
          </Space>
        ) : (
          <Space size={4}>
            <Tooltip title="Chỉnh sửa">
              <Button type="text" size="small" icon={<EditOutlined />} onClick={() => openEdit(record)} />
            </Tooltip>
            <Tooltip title="Lịch sử">
              <Button type="text" size="small" icon={<HistoryOutlined />} onClick={() => openHistory(record)} />
            </Tooltip>
            <Popconfirm
              title="Xóa model này?"
              description="Model sẽ bị ẩn khỏi danh sách. Dữ liệu vẫn được lưu lại để tracking."
              onConfirm={() => handleDelete(record.id)}
            >
              <Button type="text" size="small" danger icon={<DeleteOutlined />} />
            </Popconfirm>
          </Space>
        )
      )
    }
  ];

  return (
    <Space direction="vertical" size={14} style={{ display: "flex" }}>
      <PageHeader
        title="Quản lý model"
        description="Thiết lập cấu hình tham số cho từng model sản phẩm."
        meta={(
          <Space wrap>
            {selectedMachineName ? <Tag>{selectedMachineName}</Tag> : null}
            <Tag>Tổng model: {metrics.total}</Tag>
            <Tag color="green">Active: {metrics.active}</Tag>
            <Tag color="red">Đã xóa: {metrics.deleted}</Tag>
          </Space>
        )}
        actions={(
          <Select
            id="machine-select"
            style={{ width: "100%", minWidth: 300 }}
            placeholder="Chọn máy"
            value={selectedMachineId}
            onChange={setSelectedMachineId}
            options={machineOptions}
          />
        )}
      />

      {selectedMachineId ? (
        <>
          <SectionCard
            title="Danh sách model"
            description="Danh sách và trạng thái các model theo từng máy."
            toolbar={(
              <Space wrap>
                <Space size={6}>
                  <Switch size="small" checked={showDeleted} onChange={setShowDeleted} />
                  <Text type="secondary">Đã xóa</Text>
                </Space>
                <Input
                  prefix={<SearchOutlined />}
                  placeholder="Tìm theo Article ID..."
                  allowClear
                  value={filterName}
                  onChange={(event) => handleFilterName(event.target.value)}
                  style={{ width: 260 }}
                />
                <input
                  ref={updateExcelInputRef}
                  type="file"
                  accept=".xlsx,.xls"
                  style={{ display: "none" }}
                  onChange={(event) => handleExcelFileChange(event, "update")}
                />
                <input
                  ref={importExcelInputRef}
                  type="file"
                  accept=".xlsx,.xls"
                  style={{ display: "none" }}
                  onChange={(event) => handleExcelFileChange(event, "replace")}
                />
                <Button
                  danger
                  type="primary"
                  icon={<ExclamationCircleOutlined />}
                  loading={importingExcel}
                  onClick={confirmImportExcel}
                >
                  Import Excel
                </Button>
                <Button
                  icon={<CloudUploadOutlined />}
                  loading={updatingExcel}
                  onClick={() => updateExcelInputRef.current?.click()}
                >
                  Update Excel
                </Button>
                
                <Button icon={<CloudDownloadOutlined />} onClick={handleExport}>
                  Export Excel
                </Button>
                <Button type="primary" icon={<PlusOutlined />} onClick={openCreate}>
                  Thêm Model
                </Button>
              </Space>
            )}
          >
            <Table
              rowKey="id"
              columns={columns}
              dataSource={filteredModels}
              loading={loading}
              size="small"
              pagination={{ pageSize: 8, showSizeChanger: false, size: "small" }}
              rowClassName={(record) => (record.isDeleted ? "ant-table-row-selected" : "")}
              scroll={{ x: 1260 }}
            />
          </SectionCard>

          <AppModal
            title={editingModel ? `Sửa Article ID: ${editingModel.modelName}` : "Tạo model mới"}
            open={modalOpen}
            width={1100}
            className="model-profile-modal"
            confirmLoading={saving}
            onCancel={closeModal}
            onOk={() => form.submit()}
            okText={editingModel ? "Lưu thay đổi" : "Tạo model"}
          >
            <Form
              key={editingModel?.id ?? "new-model"}
              form={form}
              layout="vertical"
              className="model-profile-form"
              initialValues={formValues}
              preserve={false}
              onFinish={handleSubmit}
            >
              <Form.Item
                label="Article ID"
                name="modelName"
                rules={[
                  { required: true, message: "Nhập Article ID." },
                  {
                    warningOnly: true,
                    validator: (_, value) => {
                      if (
                        !editingModel &&
                        value &&
                        models.some((model) => !model.isDeleted && model.modelName.toLowerCase() === value.trim().toLowerCase())
                      ) {
                        return Promise.reject(new Error("Tên model này đã tồn tại!"));
                      }
                      return Promise.resolve();
                    }
                  }
                ]}
                style={{ maxWidth: 320, marginBottom: 8 }}
              >
                <Input size="small" placeholder="VD: AN1147G" disabled={!!editingModel} />
              </Form.Item>

              <MetadataFormItems />
            </Form>
          </AppModal>

          <Modal
            title={`Lịch sử thay đổi — ${historyModelName}`}
            open={historyModalOpen}
            onCancel={() => {
              setHistoryModalOpen(false);
              setHistorySearch("");
            }}
            footer={null}
            width={1200}
          >
            <Input
              prefix={<SearchOutlined />}
              placeholder="Tìm theo ID…"
              allowClear
              value={historySearch}
              onChange={(event) => setHistorySearch(event.target.value)}
              style={{ marginBottom: 12, maxWidth: 240 }}
            />
            <Table
              rowKey="id"
              loading={historyLoading}
              dataSource={snapshots.filter((snapshot) => (historySearch ? String(snapshot.id).includes(historySearch) : true))}
              pagination={{ pageSize: 10, showSizeChanger: true, pageSizeOptions: [10, 20, 50], size: "small" }}
              size="small"
              scroll={{ x: 980 }}
              expandable={{
                expandedRowRender: (record) => (
                  <Space direction="vertical" style={{ display: "flex" }}>
                    <Descriptions column={2} size="small" bordered>
                      <Descriptions.Item label="Đường kính phôi đầu vào">
                        {record.inputBlankDiameter ?? 0}
                      </Descriptions.Item>
                      <Descriptions.Item label="Chiều sâu bạc mâm cặp OP2">
                        {record.op2ChuckSleeveDepth ?? 0}
                      </Descriptions.Item>
                    </Descriptions>
                    <SnapshotDataView data={record.robotData} fields={ROBOT_FIELDS} />
                  </Space>
                )
              }}
              columns={[
                {
                  title: "ID",
                  dataIndex: "id",
                  key: "id",
                  width: 80,
                  sorter: (a, b) => a.id - b.id,
                  render: (value) => <Text code>{value}</Text>
                },
                {
                  title: "Hành động",
                  dataIndex: "changeAction",
                  key: "changeAction",
                  width: 120,
                  filters: [
                    { text: "Tạo mới", value: "Created" },
                    { text: "Cập nhật", value: "Updated" },
                    { text: "Xóa", value: "Deleted" }
                  ],
                  onFilter: (value, record) => record.changeAction === value,
                  render: (value) => <Tag color={ACTION_COLORS[value]}>{ACTION_LABELS[value] || value}</Tag>
                },
                { title: "Article ID", dataIndex: "modelName", key: "modelName", ellipsis: true },
                { title: "Người thực hiện", dataIndex: "performedByUsername", key: "performedByUsername", width: 150 },
                {
                  title: "Thời gian",
                  dataIndex: "performedAtUtc",
                  key: "performedAtUtc",
                  width: 180,
                  defaultSortOrder: "descend",
                  sorter: (a, b) => new Date(a.performedAtUtc) - new Date(b.performedAtUtc),
                  render: (value) => formatDateTime(value)
                }
              ]}
            />
          </Modal>
        </>
      ) : (
        <SectionCard title="Chưa có máy để cấu hình" description="Vui lòng tạo máy trước khi quản lý model profile.">
          <Paragraph type="secondary" style={{ margin: 0 }}>
            Sau khi có ít nhất một máy trong danh mục, màn hình này sẽ cho phép bạn tạo và quản lý bộ tham số model.
          </Paragraph>
        </SectionCard>
      )}
    </Space>
  );
}

export default ModelProfiles;
