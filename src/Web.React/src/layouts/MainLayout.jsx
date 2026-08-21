import React, { useEffect, useMemo, useState } from "react";
import { Button, Drawer, Grid, Layout, Menu, Select, Space, Tag, Typography } from "antd";
import {
  AppstoreOutlined,
  ControlOutlined,
  FileTextOutlined,
  InboxOutlined,
  LoginOutlined,
  LogoutOutlined,
  MenuFoldOutlined,
  MenuOutlined,
  MenuUnfoldOutlined,
  SettingOutlined,
  TeamOutlined
} from "@ant-design/icons";
import { NavLink, Outlet, useLocation, useNavigate } from "react-router-dom";
import { getConfig } from "../config/api";
import {
  canAccessProductionReport,
  canAccessShelfDeclaration
} from "../config/access";
import { useAuth } from "../contexts/AuthContext";
import { useMachine } from "../contexts/MachineContext";
import packageJson from "../../package.json";

const { Header, Content, Footer, Sider } = Layout;
const { Text } = Typography;
const { useBreakpoint } = Grid;

function MainLayout() {
  const screens = useBreakpoint();
  const isMobile = !screens.lg;
  const [collapsed, setCollapsed] = useState(false);
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const [openKeys, setOpenKeys] = useState([]);
  const location = useLocation();
  const navigate = useNavigate();
  const { currentUser, logout } = useAuth();
  const { machines, loadingMachines, currentMachine, currentMachineCode, changeMachine } = useMachine();
  const config = getConfig();
  const logoSrc = `${process.env.PUBLIC_URL || ""}/Logo.png`;

  const menuItems = useMemo(() => {
    const code = currentMachineCode || "default";
    const items = [
      {
        key: "/files",
        icon: <FileTextOutlined />,
        label: <NavLink to={`/m/${encodeURIComponent(code)}/files`}>Tệp tải lên</NavLink>
      }
    ];

    if (currentUser && canAccessShelfDeclaration(currentUser.role)) {
      items.push({
        key: "/shelf-declaration",
        icon: <InboxOutlined />,
        label: <NavLink to={`/m/${encodeURIComponent(code)}/shelf-declaration`}>Khai báo kệ</NavLink>
      });
    }

    if (currentUser && canAccessProductionReport(currentUser.role)) {
      items.push({
        key: "/production-report",
        icon: <FileTextOutlined />,
        label: <NavLink to={`/m/${encodeURIComponent(code)}/production-report`}>Báo cáo sản xuất</NavLink>
      });
    }

    if (currentUser && ["Admin", "Technician"].includes(currentUser.role)) {
      const settingChildren = [
        {
          key: "/settings/models",
          icon: <ControlOutlined />,
          label: <NavLink to={`/m/${encodeURIComponent(code)}/settings/models`}>Quản lý model</NavLink>
        },
        {
          key: "/settings/machines",
          icon: <AppstoreOutlined />,
          label: <NavLink to={`/m/${encodeURIComponent(code)}/settings/machines`}>Cài đặt máy</NavLink>
        }
      ];

      if (currentUser.role === "Admin") {
        settingChildren.push({
          key: "/settings/users",
          icon: <TeamOutlined />,
          label: <NavLink to={`/m/${encodeURIComponent(code)}/settings/users`}>Cài đặt người dùng</NavLink>
        });
      }

      items.push({
        key: "/settings",
        icon: <SettingOutlined />,
        label: "Cài đặt",
        children: settingChildren
      });
    }

    return items;
  }, [currentUser, currentMachineCode]);

  const selectedKey = useMemo(() => {
    const subPath = location.pathname.replace(/^\/m\/[^/]+/, "");
    if (subPath.startsWith("/settings/users")) {
      return "/settings/users";
    }
    if (subPath.startsWith("/settings/models")) {
      return "/settings/models";
    }
    if (subPath.startsWith("/settings/machines")) {
      return "/settings/machines";
    }
    if (subPath.startsWith("/shelf-declaration")) {
      return "/shelf-declaration";
    }
    if (subPath.startsWith("/production-report")) {
      return "/production-report";
    }
    if (subPath.startsWith("/files")) {
      return "/files";
    }
    return subPath || "/files";
  }, [location.pathname]);

  useEffect(() => {
    if (isMobile) {
      setMobileMenuOpen(false);
      return;
    }

    if (selectedKey.startsWith("/settings")) {
      setOpenKeys(["/settings"]);
      return;
    }

    setOpenKeys([]);
  }, [isMobile, selectedKey]);

  const handleToggleNav = () => {
    if (isMobile) {
      setMobileMenuOpen(true);
      return;
    }
    setCollapsed((value) => !value);
  };

  const handleLogout = () => {
    logout(true);
    navigate("/login");
  };

  const navigation = (
    <div style={{ height: "100%", display: "flex", flexDirection: "column" }}>
      <div
        style={{
          display: "flex",
          alignItems: "center",
          justifyContent: "center",
          gap: 12,
          padding: collapsed && !isMobile ? "16px 12px" : "16px 18px 12px"
        }}
      >
        <img 
          src={logoSrc} 
          alt="Gear Lathe Feeder Pre-XLN"
          style={{ 
            width: collapsed && !isMobile ? 40 : 48, 
            height: "auto",
            objectFit: "contain"
          }} 
        />
      </div>

      <Menu
        mode="inline"
        selectedKeys={[selectedKey]}
        openKeys={openKeys}
        onOpenChange={setOpenKeys}
        items={menuItems}
        style={{ flex: 1, borderInlineEnd: 0 }}
      />
    </div>
  );

  const userPanel = currentUser ? (
    <div
      style={{
        display: "flex",
        alignItems: "center",
        justifyContent: "flex-end",
        gap: 12,
        flexWrap: isMobile ? "wrap" : "nowrap",
        width: "100%"
      }}
    >
      <Text
        strong
        style={{
          whiteSpace: isMobile ? "normal" : "nowrap",
          textAlign: isMobile ? "left" : "right"
        }}
      >
        {currentUser.fullName}
      </Text>
      <Tag color="blue" style={{ marginInlineEnd: 0 }}>
        {currentUser.role}
      </Tag>
      <Button icon={<LogoutOutlined />} onClick={handleLogout}>
        Logout
      </Button>
    </div>
  ) : (
    <Button type="primary" icon={<LoginOutlined />} onClick={() => navigate("/login")}>
      Login
    </Button>
  );

  return (
    <Layout style={{ minHeight: "100vh", background: "#f5f7fb" }}>
      {isMobile ? (
        <Drawer
          placement="left"
          open={mobileMenuOpen}
          onClose={() => setMobileMenuOpen(false)}
          closable={false}
          width={280}
          styles={{ body: { padding: 0 }, content: { background: "#fff" } }}
        >
          {navigation}
        </Drawer>
      ) : (
        <Sider
          trigger={null}
          collapsible
          collapsed={collapsed}
          width={256}
          collapsedWidth={88}
          style={{ background: "#fff", borderRight: "1px solid #e5e7eb" }}
        >
          {navigation}
        </Sider>
      )}

      <Layout>
        <Header
          style={{
            background: "#fff",
            borderBottom: "1px solid #e5e7eb",
            padding: isMobile ? "8px 12px" : "0 16px",
            height: isMobile ? "auto" : 64,
            display: "flex",
            alignItems: "center",
            justifyContent: "space-between",
            gap: 16,
            flexWrap: isMobile ? "wrap" : "nowrap"
          }}
        >
          <Space size={12} style={{ minWidth: 0, flexWrap: "wrap" }}>
            <Button
              type="text"
              icon={
                isMobile
                  ? <MenuOutlined />
                  : collapsed
                    ? <MenuUnfoldOutlined />
                    : <MenuFoldOutlined />
              }
              onClick={handleToggleNav}
              aria-label="Toggle navigation"
            />
            <Text strong style={{ whiteSpace: "nowrap" }}>{config.APP_NAME}</Text>

            {machines.length > 0 ? (
              <Select
                value={currentMachine?.machineCode || currentMachineCode}
                onChange={(val) => changeMachine(val)}
                loading={loadingMachines}
                aria-label="Chọn máy"
                style={{ minWidth: isMobile ? 130 : 240, maxWidth: 300 }}
                popupMatchSelectWidth={false}
                options={machines.map((m) => ({
                  value: m.machineCode,
                  label: `${m.machineCode} - ${m.machineName || m.machineCode}`
                }))}
              />
            ) : null}
          </Space>
          <div
            style={{
              minWidth: 0,
              flex: isMobile ? "1 1 100%" : "0 1 auto",
              marginLeft: isMobile ? 0 : "auto"
            }}
          >
            {userPanel}
          </div>
        </Header>

        <Content
          style={{
            display: "flex",
            minHeight: 0,
            padding: isMobile ? 10 : 12
          }}
        >
          <div style={{ width: "100%", minHeight: 0, margin: "0 auto" }}>
            <Outlet />
          </div>
        </Content>

        <Footer style={{ textAlign: "center", color: "#667085", background: "transparent", padding: "4px 12px 10px" }}>
          Gear Lathe Feeder Pre-XLN Web | Designed by STI.Automation Version {packageJson.version}
        </Footer>
      </Layout>
    </Layout>
  );
}

export default MainLayout;
