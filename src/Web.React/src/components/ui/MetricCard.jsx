import React from "react";
import { Card, Typography } from "antd";

const { Paragraph, Text, Title } = Typography;

function MetricCard({
  icon,
  label,
  value,
  note,
  tone = "primary",
  className = ""
}) {
  const classes = ["metric-card", `tone-${tone}`, className].filter(Boolean).join(" ");

  return (
    <Card className={classes} variant="borderless">
      <div className="metric-card-shell">
        {icon ? <div className="metric-icon">{icon}</div> : null}
        <div className="metric-copy">
          <Text className="metric-label">{label}</Text>
          <Title level={3} className="metric-value">
            {value}
          </Title>
          {note ? <Paragraph className="metric-note">{note}</Paragraph> : null}
        </div>
      </div>
    </Card>
  );
}

export default MetricCard;
