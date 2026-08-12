import React, { useRef } from "react";
import { Button, Col, Form, Row, Typography } from "antd";
import { ReloadOutlined, SendOutlined } from "@ant-design/icons";
import SectionCard from "../../../components/ui/SectionCard";
import DeclarationControls from "./DeclarationControls";
import OrderEditorTable from "./OrderEditorTable";
import TrayPreviewPanel from "./TrayPreviewPanel";

function CreateDeclarationTab({
  mode,
  stagingSlotIndex,
  machineSlotIndex,
  orders,
  maxTotal,
  totalCapacity,
  canAddOrder,
  computedOrders,
  orderRows,
  orderQuantityCaps,
  cartPreview,
  selectedOrderIndex,
  selectedOrderSlots,
  occupiedStagingSlots,
  selectedMachineStagingSlots,
  busyMachineSlots,
  validationErrors,
  isBusyLocked,
  loadingSlots,
  submitting,
  onRefresh,
  onModeChange,
  onSelectStagingSlot,
  onMachineSlotChange,
  onAddOrder,
  onUpdateOrder,
  onResolveModelOrder,
  onRemoveOrder,
  onSelectOrder,
  onSubmit
}) {
  const submitButtonRef = useRef(null);
  const { Text } = Typography;

  return (
    <SectionCard
      title="Tạo khai báo mới"
      description="Nhập liệu thông tin kệ cho bản khai báo mới."
      toolbar={(
        <Button icon={<ReloadOutlined />} onClick={() => onRefresh()}>
          Làm mới
        </Button>
      )}
    >
      <Form layout="vertical">
        <DeclarationControls
          mode={mode}
          onModeChange={onModeChange}
          loadingSlots={loadingSlots}
          occupiedStagingSlots={occupiedStagingSlots}
          selectedMachineStagingSlots={selectedMachineStagingSlots}
          stagingSlotIndex={stagingSlotIndex}
          onSelectStagingSlot={onSelectStagingSlot}
          machineSlotIndex={machineSlotIndex}
          busyMachineSlots={busyMachineSlots}
          onMachineSlotChange={onMachineSlotChange}
        />

        {validationErrors?.length ? (
          <div
            role="status"
            style={{
              color: "#8a5a00",
              fontSize: 13,
              lineHeight: 1.45,
              marginBottom: 16,
              padding: "8px 10px",
              borderLeft: "3px solid #d89614",
              background: "#fffaf0"
            }}
          >
            <Text strong style={{ color: "inherit" }}>Chưa thể khai báo: </Text>
            <Text style={{ color: "inherit" }}>{validationErrors.join(" ")}</Text>
          </div>
        ) : null}

        <Row gutter={[16, 16]}>
          <Col xs={24} xl={16} xxl={17}>
            <OrderEditorTable
              orders={orders}
              maxTotal={maxTotal}
              totalCapacity={totalCapacity}
              canAddOrder={canAddOrder}
              computedOrders={computedOrders}
              orderRows={orderRows}
              orderQuantityCaps={orderQuantityCaps}
              selectedOrderIndex={selectedOrderIndex}
              isBusyLocked={isBusyLocked}
              onAddOrder={onAddOrder}
              onUpdateOrder={onUpdateOrder}
              onResolveModelOrder={onResolveModelOrder}
              onRemoveOrder={onRemoveOrder}
              onSelectOrder={onSelectOrder}
              onFocusSubmit={() => submitButtonRef.current?.focus?.()}
            />
          </Col>

          <Col xs={24} xl={8} xxl={7}>
            <TrayPreviewPanel
              mode={mode}
              machineSlotIndex={machineSlotIndex}
              busyMachineSlots={busyMachineSlots}
              cartPreview={cartPreview}
              selectedOrderSlots={selectedOrderSlots}
            />
          </Col>
        </Row>

        <div style={{ display: "flex", justifyContent: "flex-end", marginTop: 12 }}>
          <Button
            ref={submitButtonRef}
            type="primary"
            icon={<SendOutlined />}
            onClick={onSubmit}
            loading={submitting}
            disabled={isBusyLocked}
          >
            Khai báo
          </Button>
        </div>
      </Form>
    </SectionCard>
  );
}

export default CreateDeclarationTab;
