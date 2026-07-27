import React from "react";
import { Button, Modal } from "antd";

function AppModal({
  open,
  title,
  width = 640,
  okText = "Lưu",
  cancelText = "Hủy",
  confirmLoading = false,
  onCancel,
  onOk,
  children
}) {
  return (
    <Modal
      open={open}
      title={title}
      width={width}
      onCancel={onCancel}
      footer={[
        <Button key="cancel" onClick={onCancel} disabled={confirmLoading}>
          {cancelText}
        </Button>,
        <Button key="ok" type="primary" onClick={onOk} loading={confirmLoading}>
          {okText}
        </Button>
      ]}
      destroyOnHidden
      maskClosable={!confirmLoading}
      keyboard={!confirmLoading}
    >
      {children}
    </Modal>
  );
}

export default AppModal;
