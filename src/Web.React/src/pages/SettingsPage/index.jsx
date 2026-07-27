import React from "react";
import { Tabs, Typography } from "antd";
import MachineSettings from "./MachineSettings";
import UserSettings from "./UserSettings";
import { useAuth } from "../../contexts/AuthContext";

const { Paragraph, Title } = Typography;

function SettingsPage() {
  const { currentUser } = useAuth();

  const items = [
    {
      key: "machines",
      label: "Machines",
      children: <MachineSettings />
    }
  ];

  if (currentUser?.role === "Admin") {
    items.push({
      key: "users",
      label: "Users",
      children: <UserSettings />
    });
  }

  return (
    <div>
      <Title level={3}>System Settings</Title>
      <Paragraph type="secondary">
        Quản lý máy và người dùng trên SQL Server thông qua các API mới của `Server.Api`.
      </Paragraph>
      <Tabs defaultActiveKey="machines" items={items} />
    </div>
  );
}

export default SettingsPage;
