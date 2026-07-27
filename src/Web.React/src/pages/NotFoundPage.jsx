import React from "react";
import { Result } from "antd";

function NotFoundPage() {
  return (
    <div style={{ minHeight: "50vh", display: "grid", placeItems: "center" }}>
      <Result
        status="404"
        title="Không tìm thấy trang"
        subTitle="Đường dẫn bạn truy cập hiện chưa tồn tại trong web console."
      />
    </div>
  );
}

export default NotFoundPage;
