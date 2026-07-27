import { App } from "antd";
import { useEffect } from "react";
import { setMessageApi } from "../utils/appMessage";

/**
 * Bridge component: phải được mount BÊN TRONG <App> của antd.
 * Nó lấy context-aware message API qua App.useApp() rồi đăng ký
 * cho module appMessage, giúp mọi nơi trong app (kể cả code không
 * phải React component) gọi được showSuccessMessage / showErrorMessage
 * với message tự tắt đúng cách.
 */
export default function AppMessageBridge() {
  const { message } = App.useApp();

  useEffect(() => {
    setMessageApi(message);
  }, [message]);

  return null;
}
