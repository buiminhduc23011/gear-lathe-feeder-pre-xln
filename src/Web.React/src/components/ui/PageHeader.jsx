import React from "react";
import { Space, Typography } from "antd";

const { Paragraph, Text, Title } = Typography;

function PageHeader({ eyebrow, title, description, actions, meta }) {
  return (
    <div
      style={{
        display: "flex",
        justifyContent: "space-between",
        alignItems: "stretch",
        gap: 16,
        flexWrap: "wrap",
        padding: "16px 18px",
        background: "#ffffff",
        border: "1px solid #e5e7eb",
        borderRadius: 16
      }}
    >
      <div style={{ minWidth: 0, flex: "1 1 560px" }}>
        <Space direction="vertical" size={6} style={{ width: "100%" }}>
          {eyebrow ? (
            <Text type="secondary" style={{ fontSize: 11, textTransform: "uppercase", letterSpacing: "0.1em" }}>
              {eyebrow}
            </Text>
          ) : null}
          <Title level={1} style={{ margin: 0, fontSize: 32, lineHeight: 1.05 }}>
            {title}
          </Title>
          {description ? (
            <Paragraph
              type="secondary"
              style={{ margin: 0, maxWidth: 860, fontSize: 14, lineHeight: 1.5 }}
            >
              {description}
            </Paragraph>
          ) : null}
          {meta ? (
            <div style={{ display: "flex", flexWrap: "wrap", gap: 8, paddingTop: 2 }}>
              {meta}
            </div>
          ) : null}
        </Space>
      </div>

      {actions ? (
        <div
          style={{
            minWidth: 0,
            flex: "0 1 auto",
            display: "flex",
            alignItems: "center",
            justifyContent: "flex-end"
          }}
        >
          {actions}
        </div>
      ) : null}
    </div>
  );
}

export default PageHeader;
