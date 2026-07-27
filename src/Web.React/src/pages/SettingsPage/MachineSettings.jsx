import React, { useEffect, useMemo, useState } from "react";
import {
  Button,
  Checkbox,
  Col,
  Form,
  Input,
  Popconfirm,
  Row,
  Space,
  Switch,
  Table,
  Tag
} from "antd";
import { PlusOutlined } from "@ant-design/icons";
import AppModal from "../../components/AppModal";
import PageHeader from "../../components/ui/PageHeader";
import SectionCard from "../../components/ui/SectionCard";
import { API_ENDPOINTS, apiClient, getApiErrorMessage } from "../../config/api";
import { showErrorMessage, showSuccessMessage } from "../../utils/appMessage";

const STAGING_SLOT_OPTIONS = [1, 2, 3, 4];

const defaultValues = {
  machineCode: "",
  machineName: "",
  manufacturer: "",
  description: "",
  model: "",
  serialNumber: "",
  location: "",
  stagingSlotIndices: [],
  isActive: true
};

export function normalizeStagingSlotIndices(stagingSlotIndices) {
  return Array.from(new Set((stagingSlotIndices ?? []).map(Number)))
    .filter((slot) => Number.isFinite(slot) && slot >= 1 && slot <= 4)
    .sort((left, right) => left - right);
}

function getMachineFormValues(machine) {
  if (!machine) {
    return defaultValues;
  }

  return {
    machineCode: machine.machineCode || "",
    machineName: machine.machineName || "",
    manufacturer: machine.manufacturer || "",
    description: machine.description || "",
    model: machine.model || "",
    serialNumber: machine.serialNumber || "",
    location: machine.location || "",
    stagingSlotIndices: normalizeStagingSlotIndices(machine.stagingSlotIndices),
    isActive: machine.isActive ?? true
  };
}

export function buildReservedSlotMap(machines, editingMachineId) {
  const reserved = new Map();

  machines.forEach((machine) => {
    if (machine.machineId === editingMachineId) {
      return;
    }

    normalizeStagingSlotIndices(machine.stagingSlotIndices).forEach((slot) => {
      reserved.set(slot, machine.machineName || machine.machineCode || `Machine ${machine.machineId}`);
    });
  });

  return reserved;
}

function renderAssignedSlots(stagingSlotIndices) {
  const slots = normalizeStagingSlotIndices(stagingSlotIndices);
  if (slots.length === 0) {
    return <Tag>Chưa gán</Tag>;
  }

  return (
    <Space wrap size={4}>
      {slots.map((slot) => (
        <Tag key={slot} color="blue">
          Slot {slot}
        </Tag>
      ))}
    </Space>
  );
}

function MachineSettings() {
  const [machines, setMachines] = useState([]);
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [modalOpen, setModalOpen] = useState(false);
  const [editingMachine, setEditingMachine] = useState(null);
  const [form] = Form.useForm();

  const loadMachines = async () => {
    setLoading(true);
    try {
      const response = await apiClient.get(API_ENDPOINTS.machines);
      setMachines(response.data);
    } catch (error) {
      showErrorMessage(getApiErrorMessage(error, "Không thể tải danh sách máy."));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadMachines();
  }, []);

  const metrics = useMemo(() => {
    const active = machines.filter((item) => item.isActive).length;
    return {
      total: machines.length,
      active,
      inactive: machines.length - active
    };
  }, [machines]);

  const reservedSlotMap = useMemo(
    () => buildReservedSlotMap(machines, editingMachine?.machineId ?? null),
    [editingMachine?.machineId, machines]
  );

  const selectedSlots = Form.useWatch("stagingSlotIndices", form) || [];

  const stagingSlotCheckboxOptions = useMemo(
    () => STAGING_SLOT_OPTIONS.map((slot) => {
      const isReserved = reservedSlotMap.has(slot);
      const isSelected = selectedSlots.includes(slot);
      const limitReached = selectedSlots.length >= 2;

      return {
        label: isReserved
          ? `Slot ${slot} • ${reservedSlotMap.get(slot)}`
          : `Slot ${slot}`,
        value: slot,
        disabled: isReserved || (limitReached && !isSelected)
      };
    }),
    [reservedSlotMap, selectedSlots]
  );

  const closeModal = () => {
    setModalOpen(false);
    setSaving(false);
  };

  const openCreate = () => {
    form.resetFields();
    setEditingMachine(null);
    setModalOpen(true);
  };

  const openEdit = (machine) => {
    form.resetFields();
    setEditingMachine(machine);
    setModalOpen(true);
  };

  useEffect(() => {
    if (modalOpen) {
      if (editingMachine) {
        form.setFieldsValue(getMachineFormValues(editingMachine));
      } else {
        form.resetFields();
      }
    }
  }, [modalOpen, editingMachine, form]);

  const formValues = useMemo(() => getMachineFormValues(editingMachine), [editingMachine]);

  const handleSubmit = async (values) => {
    const stagingSlotIndices = normalizeStagingSlotIndices(values.stagingSlotIndices);
    if (stagingSlotIndices.length === 0 || stagingSlotIndices.length > 2) {
      showErrorMessage("Mỗi máy phải được gán tối đa 2 staging slot và ít nhất 1 slot.");
      return;
    }

    const overlappedSlots = stagingSlotIndices.filter((slot) => reservedSlotMap.has(slot));
    if (overlappedSlots.length > 0) {
      showErrorMessage(`Slot ${overlappedSlots.join(", ")} đã được gán cho máy khác.`);
      return;
    }

    const payload = {
      ...values,
      stagingSlotIndices
    };

    setSaving(true);
    try {
      if (editingMachine) {
        await apiClient.put(`${API_ENDPOINTS.machines}/${editingMachine.machineId}`, payload);
        showSuccessMessage("Đã cập nhật máy.");
      } else {
        await apiClient.post(API_ENDPOINTS.machines, payload);
        showSuccessMessage("Đã tạo máy mới.");
      }

      closeModal();
      await loadMachines();
    } catch (error) {
      showErrorMessage(getApiErrorMessage(error, "Không thể lưu cài đặt máy."));
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (machineId) => {
    try {
      await apiClient.delete(`${API_ENDPOINTS.machines}/${machineId}`);
      showSuccessMessage("Đã xóa máy.");
      await loadMachines();
    } catch (error) {
      showErrorMessage(getApiErrorMessage(error, "Không thể xóa máy."));
    }
  };

  const columns = [
    {
      title: "Mã máy",
      dataIndex: "machineCode",
      key: "machineCode",
      render: (value) => <Tag>{value}</Tag>
    },
    { title: "Tên máy", dataIndex: "machineName", key: "machineName" },
    {
      title: "Staging slot",
      dataIndex: "stagingSlotIndices",
      key: "stagingSlotIndices",
      render: renderAssignedSlots
    },
    { title: "Hãng", dataIndex: "manufacturer", key: "manufacturer" },
    { title: "Model", dataIndex: "model", key: "model", render: (value) => value || "-" },
    { title: "Vị trí", dataIndex: "location", key: "location", render: (value) => value || "-" },
    {
      title: "Trạng thái",
      key: "isActive",
      render: (_, record) => (
        <Tag color={record.isActive ? "green" : "default"}>
          {record.isActive ? "Đang hoạt động" : "Tạm ngưng"}
        </Tag>
      )
    },
    {
      title: "Thao tác",
      key: "actions",
      render: (_, record) => (
        <Space>
          <Button type="link" onClick={() => openEdit(record)}>
            Chỉnh sửa
          </Button>
          <Popconfirm
            title="Xóa máy này?"
            description="Bản ghi máy sẽ bị xóa khỏi cơ sở dữ liệu web."
            onConfirm={() => handleDelete(record.machineId)}
          >
            <Button type="link" danger>
              Xóa
            </Button>
          </Popconfirm>
        </Space>
      )
    }
  ];

  return (
    <Space direction="vertical" size={14} style={{ display: "flex" }}>
      <PageHeader
        title="Cài đặt máy"
        description="Quản lý danh mục và cấu hình hoạt động của máy."
        meta={(
          <Space wrap>
            <Tag>Tổng máy: {metrics.total}</Tag>
            <Tag color="green">Đang hoạt động: {metrics.active}</Tag>
            <Tag>Tạm ngưng: {metrics.inactive}</Tag>
          </Space>
        )}
        actions={(
          <Button type="primary" icon={<PlusOutlined />} onClick={openCreate}>
            Thêm máy
          </Button>
        )}
      />

      <SectionCard
        title="Danh sách máy"
        description="Danh sách máy và phân bổ staging slot."
      >
        <Table
          rowKey="machineId"
          columns={columns}
          dataSource={machines}
          loading={loading}
          size="small"
          pagination={{ pageSize: 8, showSizeChanger: false, size: "small" }}
          scroll={{ x: 1080 }}
        />
      </SectionCard>

      <AppModal
        title={editingMachine ? "Cập nhật máy" : "Tạo máy mới"}
        open={modalOpen}
        width={920}
        confirmLoading={saving}
        onCancel={closeModal}
        onOk={() => form.submit()}
        okText={editingMachine ? "Lưu thay đổi" : "Tạo máy"}
      >
        <Form
          key={editingMachine?.machineId ?? "new-machine"}
          form={form}
          layout="vertical"
          initialValues={formValues}
          preserve={false}
          onFinish={handleSubmit}
        >
          <Row gutter={16}>
            <Col xs={24} md={8}>
              <Form.Item
                label="Mã máy"
                name="machineCode"
                rules={[{ required: true, message: "Nhập mã máy." }]}
              >
                <Input />
              </Form.Item>
            </Col>
            <Col xs={24} md={8}>
              <Form.Item
                label="Tên máy"
                name="machineName"
                rules={[{ required: true, message: "Nhập tên máy." }]}
              >
                <Input />
              </Form.Item>
            </Col>
            <Col xs={24} md={8}>
              <Form.Item
                label="Hãng sản xuất"
                name="manufacturer"
                rules={[{ required: true, message: "Nhập hãng sản xuất." }]}
              >
                <Input />
              </Form.Item>
            </Col>
            <Col xs={24} md={12}>
              <Form.Item
                label="Staging slot"
                name="stagingSlotIndices"
                extra="Chọn đúng 2 slot. Slot đã thuộc máy khác sẽ bị khóa."
              >
                <Checkbox.Group options={stagingSlotCheckboxOptions} />
              </Form.Item>
            </Col>
            <Col xs={24} md={12}>
              <Form.Item label="Model" name="model">
                <Input />
              </Form.Item>
            </Col>
            <Col xs={24} md={12}>
              <Form.Item label="Số serial" name="serialNumber">
                <Input />
              </Form.Item>
            </Col>
            <Col xs={24} md={12}>
              <Form.Item label="Vị trí" name="location">
                <Input />
              </Form.Item>
            </Col>
            <Col xs={24} md={12}>
              <Form.Item label="Mô tả" name="description">
                <Input />
              </Form.Item>
            </Col>
            <Col xs={24} md={6}>
              <Form.Item label="Kích hoạt" name="isActive" valuePropName="checked">
                <Switch />
              </Form.Item>
            </Col>
          </Row>
        </Form>
      </AppModal>
    </Space>
  );
}

export default MachineSettings;
