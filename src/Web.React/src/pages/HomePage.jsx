import React from "react";
import { Badge, Card, Col, Row, Typography } from "antd";
import {
  DatabaseOutlined,
  FileTextOutlined,
  SettingOutlined,
  UserOutlined
} from "@ant-design/icons";
import { useAuth } from "../contexts/AuthContext";

const { Paragraph, Text, Title } = Typography;

function HomePage() {
  const { currentUser } = useAuth();

  const metrics = [
    {
      title: "Đăng nhập hiện tại",
      value: currentUser ? currentUser.fullName : "Chưa đăng nhập",
      note: currentUser ? `Vai trò: ${currentUser.role}` : "Cần đăng nhập để sử dụng web",
      icon: <UserOutlined />
    },
    {
      title: "Dữ liệu web",
      value: "SQL Server",
      note: "Máy, người dùng và metadata file upload được lưu tập trung tại đây",
      icon: <DatabaseOutlined />
    },
    {
      title: "Phạm vi của web",
      value: "3 nhóm dữ liệu",
      note: "Cài đặt máy, cài đặt người dùng và file upload",
      icon: <SettingOutlined />
    }
  ];

  return (
    <div className="page-shell">
      <section className="page-hero">
        <div className="page-hero-main">
          <Text className="page-kicker">PINION ROBOT</Text>
          <Title className="page-title">Web quản lý máy, người dùng và file tải lên</Title>
          <Paragraph className="page-description">
            Web này quản lý danh mục máy, tài khoản đăng nhập và toàn bộ file đã đi qua `FilesController`. Các thông
            số vận hành cục bộ của máy như PLC, scan rate hoặc địa chỉ kết nối vẫn được lưu trong desktop app tại
            từng máy.
          </Paragraph>
        </div>

        <div className="page-hero-side">
          <Card className="highlight-card" bordered={false}>
            <div className="highlight-line">
              <Text className="highlight-label">Người dùng hiện tại</Text>
              <Text strong>{currentUser?.fullName || "Chưa đăng nhập"}</Text>
            </div>
            <div className="highlight-line">
              <Text className="highlight-label">Vai trò</Text>
              <Text strong>{currentUser?.role || "Guest"}</Text>
            </div>
            <div className="highlight-line">
              <Text className="highlight-label">Trạng thái web</Text>
              <Badge status="processing" text="Sẵn sàng quản lý dữ liệu web" />
            </div>
          </Card>
        </div>
      </section>

      <Row gutter={[16, 16]} className="section-grid">
        {metrics.map((metric) => (
          <Col xs={24} md={8} key={metric.title}>
            <Card className="metric-card" bordered={false}>
              <div className="metric-icon">{metric.icon}</div>
              <Text className="metric-title">{metric.title}</Text>
              <Title level={3} className="metric-value">
                {metric.value}
              </Title>
              <Paragraph className="metric-note">{metric.note}</Paragraph>
            </Card>
          </Col>
        ))}
      </Row>

      <Row gutter={[16, 16]} className="section-grid">
        <Col xs={24} lg={12}>
          <Card className="data-card" bordered={false}>
            <Text className="section-kicker">Cài đặt máy</Text>
            <Title level={4} className="section-title">
              Dữ liệu máy quản lý trên web
            </Title>
            <Paragraph className="section-description">
              Web lưu và chỉnh các thông tin như mã máy, tên máy, hãng sản xuất, model, serial, vị trí và trạng thái
              kích hoạt của máy.
            </Paragraph>
          </Card>
        </Col>

        <Col xs={24} lg={12}>
          <Card className="data-card" bordered={false}>
            <Text className="section-kicker">Cài đặt người dùng</Text>
            <Title level={4} className="section-title">
              Dữ liệu người dùng quản lý trên web
            </Title>
            <Paragraph className="section-description">
              Web lưu tài khoản đăng nhập, họ tên, email, vai trò và trạng thái hoạt động của người dùng. Mật khẩu
              được hash ở backend.
            </Paragraph>
          </Card>
        </Col>
      </Row>

      <Card className="data-card" bordered={false}>
        <Text className="section-kicker">File upload</Text>
        <Title level={4} className="section-title">
          File đi qua FilesController
        </Title>
        <Paragraph className="section-description">
          Web hiển thị file đã upload lên server, cho phép tải xuống và xóa mềm theo quyền của từng tài khoản.
        </Paragraph>
        <div className="metric-icon">
          <FileTextOutlined />
        </div>
      </Card>

      <Card className="data-card" bordered={false}>
        <Text className="section-kicker">Lưu ý phạm vi</Text>
        <Title level={4} className="section-title">
          Những gì không cấu hình trên web
        </Title>
        <Paragraph className="section-description">
          Web này không thay thế trang cài đặt trong desktop app. Các thông số PLC, kết nối chạy máy, scan rate và
          cấu hình áp dụng trực tiếp cho máy vẫn được lưu cục bộ trong ứng dụng desktop.
        </Paragraph>
      </Card>
    </div>
  );
}

export default HomePage;
