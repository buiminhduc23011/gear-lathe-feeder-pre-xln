import React, { useEffect } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { Spin } from "antd";
import { useMachine } from "../contexts/MachineContext";

function MachineRedirect({ to = "files" }) {
  const { currentMachineCode, loadingMachines } = useMachine();
  const location = useLocation();
  const navigate = useNavigate();

  useEffect(() => {
    if (!loadingMachines && currentMachineCode) {
      const cleanTo = to.replace(/^\//, "");
      const targetUrl = `/m/${encodeURIComponent(currentMachineCode)}/${cleanTo}${location.search}${location.hash}`;
      navigate(targetUrl, { replace: true });
    }
  }, [loadingMachines, currentMachineCode, to, location.search, location.hash, navigate]);

  return (
    <div
      style={{
        display: "grid",
        placeItems: "center",
        minHeight: "60vh",
        width: "100%"
      }}
    >
      <Spin size="large" tip="Đang tải dữ liệu máy..." />
    </div>
  );
}

export default MachineRedirect;
