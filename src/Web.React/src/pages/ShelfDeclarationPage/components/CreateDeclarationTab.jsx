import React, { useRef } from "react";
import { Button, Col, Form, Row } from "antd";
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
  layoutInfo,
  displayLayoutInfo,
  maxTotal,
  totalCapacity,
  canAddOrder,
  computedOrders,
  orderRows,
  orderQuantityCaps,
  preview,
  selectedOrderIndex,
  selectedOrderSlots,
  occupiedStagingSlots,
  selectedMachineStagingSlots,
  busyMachineSlots,
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
          onMachineSlotChange={onMachineSlotChange}
        />

        <Row gutter={[16, 16]}>
          <Col xs={24} xl={16} xxl={17}>
            <OrderEditorTable
              layoutInfo={layoutInfo}
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
              layoutInfo={displayLayoutInfo}
              preview={preview}
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
