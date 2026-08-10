import React, { useState } from "react";
import { Alert, Card, Button, Form, Input, Space, Typography } from "antd";
import { LockOutlined, UserOutlined } from "@ant-design/icons";
import { useLocation, useNavigate } from "react-router-dom";
import { getApiErrorMessage } from "../config/api";
import { useAuth } from "../contexts/AuthContext";
import { showSuccessMessage } from "../utils/appMessage";

const { Paragraph, Text, Title } = Typography;

function LoginPage() {
  const [loading, setLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");
  const navigate = useNavigate();
  const location = useLocation();
  const { login } = useAuth();
  const logoSrc = `${process.env.PUBLIC_URL || ""}/Logo.png`;

  const redirectTo = location.state?.from?.pathname || "/";

  const handleSubmit = async (values) => {
    setLoading(true);
    setErrorMessage("");

    try {
      await login(values.username, values.password);
      showSuccessMessage("Đăng nhập thành công.");
      navigate(redirectTo, { replace: true });
    } catch (error) {
      setErrorMessage(getApiErrorMessage(error, "Không thể đăng nhập."));
    } finally {
      setLoading(false);
    }
  };

  return (
    <div
      style={{
        minHeight: "100vh",
        display: "grid",
        placeItems: "center",
        padding: 24,
        background: "#f5f7fb"
      }}
    >
      <Card style={{ width: "100%", maxWidth: 460 }} styles={{ body: { padding: 32 } }}>
        <Space direction="vertical" size={16} style={{ display: "flex" }}>
          <Space align="center" size={16}>
            <img src={logoSrc} alt="Gear Lathe Feeder Pre-XLN" style={{ width: 56, height: 56, objectFit: "contain" }} />
            <div>
              <Text type="secondary">Gear Lathe Feeder Pre-XLN</Text>
              <Title level={3} style={{ margin: 0 }}>
                Đăng nhập hệ thống
              </Title>
            </div>
          </Space>

          <Paragraph type="secondary" style={{ margin: 0 }}>
            Phần mềm quản trị hệ thống Gear Lathe Feeder Pre-XLN.
          </Paragraph>

          {errorMessage ? (
            <Alert
              type="error"
              showIcon
              message="Đăng nhập thất bại"
              description={errorMessage}
            />
          ) : null}

          <Form layout="vertical" onFinish={handleSubmit}>
            <Form.Item
              label="Tên đăng nhập"
              name="username"
              rules={[{ required: true, message: "Vui lòng nhập tên đăng nhập." }]}
            >
              <Input prefix={<UserOutlined />} size="large" />
            </Form.Item>

            <Form.Item
              label="Mật khẩu"
              name="password"
              rules={[{ required: true, message: "Vui lòng nhập mật khẩu." }]}
            >
              <Input.Password prefix={<LockOutlined />} size="large" />
            </Form.Item>

            <Button type="primary" htmlType="submit" loading={loading} block size="large">
              Đăng nhập
            </Button>
          </Form>
        </Space>
      </Card>
    </div>
  );
}

export default LoginPage;
