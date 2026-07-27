import React from "react";
import { Card, Space, Typography } from "antd";

const { Paragraph, Text, Title } = Typography;

function SectionCard({ eyebrow, title, description, toolbar, children, style, ...cardProps }) {
  return (
    <Card
      variant="borderless"
      style={{
        border: "1px solid #e5e7eb",
        boxShadow: "0 2px 8px rgba(15, 23, 42, 0.03)",
        ...style
      }}
      styles={{ body: { padding: 16 } }}
      {...cardProps}
    >
      {eyebrow || title || description || toolbar ? (
        <div
          style={{
            display: "flex",
            justifyContent: "space-between",
            alignItems: "flex-start",
            gap: 12,
            flexWrap: "wrap",
            marginBottom: 12
          }}
        >
          <Space direction="vertical" size={1} style={{ maxWidth: 760 }}>
            {eyebrow ? (
              <Text type="secondary" style={{ fontSize: 12, textTransform: "uppercase", letterSpacing: "0.08em" }}>
                {eyebrow}
              </Text>
            ) : null}
            {title ? (
              <Title level={4} style={{ margin: 0, lineHeight: 1.15 }}>
                {title}
              </Title>
            ) : null}
            {description ? (
              <Paragraph type="secondary" style={{ margin: 0, fontSize: 14, lineHeight: 1.5 }}>
                {description}
              </Paragraph>
            ) : null}
          </Space>

          {toolbar ? <div>{toolbar}</div> : null}
        </div>
      ) : null}

      {children}
    </Card>
  );
}

export default SectionCard;
