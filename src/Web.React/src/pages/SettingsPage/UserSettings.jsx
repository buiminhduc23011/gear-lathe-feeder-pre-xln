import React, { useEffect, useMemo, useState } from "react";
import {
  Button,
  Form,
  Input,
  Popconfirm,
  Select,
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

const defaultValues = {
  username: "",
  password: "",
  fullName: "",
  email: "",
  role: "Operator",
  isActive: true,
  isSystemAccount: false
};

function normalizeUsername(value) {
  return (value ?? "").trim().toLowerCase();
}

export function isUsernameTaken(users, username, currentUserId = null) {
  const normalized = normalizeUsername(username);
  if (!normalized) {
    return false;
  }

  return (users ?? []).some((user) => (
    user.id !== currentUserId && normalizeUsername(user.username) === normalized
  ));
}

export function getUserFormValues(user) {
  if (!user) {
    return { ...defaultValues };
  }

  return {
    username: user.username || "",
    password: "",
    fullName: user.fullName || "",
    email: user.email || "",
    role: user.role || "Operator",
    isActive: user.isActive ?? true,
    isSystemAccount: user.isSystemAccount ?? false
  };
}

export function normalizeUserFormValues(values, includeSystemAccount) {
  const normalized = {
    ...values,
    username: (values.username ?? "").trim(),
    password: values.password ?? "",
    fullName: (values.fullName ?? "").trim(),
    email: (values.email ?? "").trim() || null,
    role: (values.role ?? "").trim(),
    isActive: values.isActive ?? true
  };

  if (includeSystemAccount) {
    normalized.isSystemAccount = values.isSystemAccount ?? false;
  } else {
    delete normalized.isSystemAccount;
  }

  return normalized;
}

function UserSettings() {
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [modalOpen, setModalOpen] = useState(false);
  const [editingUser, setEditingUser] = useState(null);
  const [form] = Form.useForm();

  const loadUsers = async () => {
    setLoading(true);
    try {
      const response = await apiClient.get(API_ENDPOINTS.users);
      setUsers(response.data);
    } catch (error) {
      showErrorMessage(getApiErrorMessage(error, "Không thể tải danh sách người dùng."));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadUsers();
  }, []);

  const metrics = useMemo(() => {
    const active = users.filter((item) => item.isActive).length;
    const systemAccounts = users.filter((item) => item.isSystemAccount).length;
    return {
      total: users.length,
      active,
      systemAccounts
    };
  }, [users]);

  const closeModal = () => {
    setModalOpen(false);
    setSaving(false);
  };

  const openCreate = () => {
    setEditingUser(null);
    setModalOpen(true);
    form.setFieldsValue(getUserFormValues(null));
  };

  const openEdit = (user) => {
    form.resetFields();
    setEditingUser(user);
    setModalOpen(true);
  };

  useEffect(() => {
    if (modalOpen) {
      const nextValues = getUserFormValues(editingUser);
      form.resetFields();
      form.setFieldsValue(nextValues);

      if (!editingUser) {
        const resetTimer = window.setTimeout(() => {
          form.setFieldsValue(getUserFormValues(null));
        }, 0);

        return () => window.clearTimeout(resetTimer);
      }
    }

    return undefined;
  }, [modalOpen, editingUser, form]);

  const formValues = useMemo(() => getUserFormValues(editingUser), [editingUser]);
  const usernameRules = useMemo(() => [
    { required: true, message: "Nhập tên đăng nhập." },
    {
      validator: (_, value) => (
        isUsernameTaken(users, value, editingUser?.id)
          ? Promise.reject(new Error("Tên đăng nhập đã tồn tại."))
          : Promise.resolve()
      )
    }
  ], [editingUser?.id, users]);

  const handleSubmit = async (values) => {
    setSaving(true);
    const submitValues = normalizeUserFormValues(values, !editingUser);

    try {
      if (editingUser) {
        await apiClient.put(`${API_ENDPOINTS.users}/${editingUser.id}`, submitValues);
        showSuccessMessage("Đã cập nhật người dùng.");
      } else {
        await apiClient.post(API_ENDPOINTS.users, submitValues);
        showSuccessMessage("Đã tạo người dùng mới.");
      }

      closeModal();
      await loadUsers();
    } catch (error) {
      showErrorMessage(getApiErrorMessage(error, "Không thể lưu người dùng."));
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (user) => {
    try {
      await apiClient.delete(`${API_ENDPOINTS.users}/${user.id}`);
      showSuccessMessage("Đã xóa người dùng.");
      await loadUsers();
    } catch (error) {
      showErrorMessage(getApiErrorMessage(error, "Không thể xóa người dùng."));
    }
  };

  const columns = [
    {
      title: "Tài khoản",
      dataIndex: "username",
      key: "username",
      render: (value) => <Tag>{value}</Tag>
    },
    { title: "Họ tên", dataIndex: "fullName", key: "fullName" },
    { title: "Email", dataIndex: "email", key: "email", render: (value) => value || "-" },
    {
      title: "Vai trò",
      dataIndex: "role",
      key: "role",
      render: (value) => <Tag color="blue">{value}</Tag>
    },
    {
      title: "Trạng thái",
      key: "status",
      render: (_, record) => (
        <Tag color={record.isActive ? "green" : "default"}>
          {record.isActive ? "Đang hoạt động" : "Đã khóa"}
        </Tag>
      )
    },
    {
      title: "Loại tài khoản",
      key: "system",
      render: (_, record) => (
        <Tag color={record.isSystemAccount ? "gold" : "default"}>
          {record.isSystemAccount ? "System" : "Normal"}
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
            title="Xóa người dùng này?"
            description="Tài khoản hệ thống sẽ không thể xóa."
            onConfirm={() => handleDelete(record)}
            disabled={record.isSystemAccount}
          >
            <Button type="link" danger disabled={record.isSystemAccount}>
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
        title="Cài đặt người dùng"
        description="Quản lý tài khoản người dùng và phân quyền hệ thống."
        meta={(
          <Space wrap>
            <Tag>Tổng tài khoản: {metrics.total}</Tag>
            <Tag color="green">Đang hoạt động: {metrics.active}</Tag>
            <Tag color="gold">Tài khoản hệ thống: {metrics.systemAccounts}</Tag>
          </Space>
        )}
        actions={(
          <Button type="primary" icon={<PlusOutlined />} onClick={openCreate}>
            Thêm người dùng
          </Button>
        )}
      />

      <SectionCard
        title="Danh sách người dùng"
        description="Danh sách chi tiết tài khoản người dùng."
      >
        <Table
          rowKey="id"
          columns={columns}
          dataSource={users}
          loading={loading}
          size="small"
          pagination={{ pageSize: 8, showSizeChanger: false, size: "small" }}
          scroll={{ x: 920 }}
        />
      </SectionCard>

      <AppModal
        title={editingUser ? "Cập nhật người dùng" : "Tạo người dùng mới"}
        open={modalOpen}
        width={620}
        confirmLoading={saving}
        onCancel={closeModal}
        onOk={() => form.submit()}
        okText={editingUser ? "Lưu thay đổi" : "Tạo người dùng"}
      >
        <Form
          key={editingUser?.id ?? "new-user"}
          form={form}
          layout="vertical"
          initialValues={formValues}
          autoComplete="off"
          preserve={false}
          onFinish={handleSubmit}
        >
          <Form.Item
            label="Tên đăng nhập"
            name="username"
            rules={usernameRules}
            validateTrigger={["onChange", "onBlur"]}
          >
            <Input
              autoComplete={editingUser ? "username" : "off"}
              onBlur={(event) => {
                form.setFieldsValue({ username: event.target.value.trim() });
                form.validateFields(["username"]).catch(() => {});
              }}
            />
          </Form.Item>
          <Form.Item
            label="Mật khẩu"
            name="password"
            rules={editingUser
              ? []
              : [
                  { required: true, message: "Nhập mật khẩu." },
                  { min: 6, message: "Mật khẩu phải có ít nhất 6 ký tự." }
                ]}
          >
            <Input.Password
              autoComplete="new-password"
              placeholder={editingUser ? "Để trống nếu không đổi mật khẩu" : ""}
            />
          </Form.Item>
          <Form.Item
            label="Họ tên"
            name="fullName"
            rules={[{ required: true, message: "Nhập họ tên." }]}
          >
            <Input />
          </Form.Item>
          <Form.Item label="Email" name="email">
            <Input />
          </Form.Item>
          <Form.Item
            label="Vai trò"
            name="role"
            rules={[{ required: true, message: "Chọn vai trò." }]}
          >
            <Select
              options={[
                { label: "Admin", value: "Admin" },
                { label: "Technician", value: "Technician" },
                { label: "Operator", value: "Operator" },
                { label: "Viewer", value: "Viewer" }
              ]}
            />
          </Form.Item>
          <Form.Item label="Kích hoạt" name="isActive" valuePropName="checked">
            <Switch />
          </Form.Item>
          {!editingUser ? (
            <Form.Item label="Tài khoản hệ thống" name="isSystemAccount" valuePropName="checked">
              <Switch />
            </Form.Item>
          ) : null}
        </Form>
      </AppModal>
    </Space>
  );
}

export default UserSettings;
