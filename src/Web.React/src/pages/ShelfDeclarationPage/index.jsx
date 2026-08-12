import React, { useCallback } from "react";
import { Select, Space, Tabs, Tag } from "antd";
import { SearchOutlined } from "@ant-design/icons";
import PageHeader from "../../components/ui/PageHeader";
import useShelfDeclarationData from "./hooks/useShelfDeclarationData";
import useShelfDeclarationForm from "./hooks/useShelfDeclarationForm";
import { DECLARATION_MODES } from "./constants";
import CreateDeclarationTab from "./components/CreateDeclarationTab";
import HistoryDeclarationTab from "./components/HistoryDeclarationTab";
import { showWarningMessage } from "../../utils/appMessage";
import "./types";

function buildSubmitPayload(formState) {
  return {
    mode: formState.mode,
    stagingSlotIndex: formState.mode === DECLARATION_MODES.AGV ? formState.stagingSlotIndex : null,
    machineSlotIndex: formState.mode === DECLARATION_MODES.MANUAL ? formState.machineSlotIndex : null,
    shelfLayoutType: 0,
    orders: formState.placementRows.map((order) => ({
      orderId: order.orderId?.trim() || null,
      modelName: order.modelName,
      reportModelName: order.reportModelName || null,
      quantity: order.quantity,
      cartPositionIndex: order.cartPositionIndex,
      jigType: order.jigType,
      inputThickness: order.inputThickness,
      jigHeightMm: order.jigHeightMm,
      jigCapacity: order.maxQty
    }))
  };
}

function ShelfDeclarationPage() {
  const data = useShelfDeclarationData();
  const form = useShelfDeclarationForm({
    selectedMachineId: data.selectedMachineId,
    machines: data.machines,
    models: data.models,
    history: data.history,
    slotStatuses: data.slotStatuses
  });

  const handleMachineChange = useCallback((machineId) => {
    data.handleMachineChange(machineId);
    form.resetMachineScopedState();
  }, [data, form]);

  const handleSubmit = useCallback(async () => {
    if (!form.ensureModelOrdersResolved()) {
      return;
    }

    if (!form.canSubmit) {
      showWarningMessage(form.validationErrors[0] ?? "Dữ liệu chưa hợp lệ.");
      return;
    }

    const didSubmit = await data.submitDeclaration(buildSubmitPayload(form));
    if (didSubmit) {
      form.resetAfterSubmit();
    }
  }, [data, form]);

  return (
    <Space direction="vertical" size={12} style={{ display: "flex" }}>
      <PageHeader
        title="Khai báo kệ"
        description="Tạo và quản lý các bản khai báo kệ gia công."
        meta={<Tag>{form.selectedMachineLabel}</Tag>}
        actions={(
          <Select
            placeholder="Chọn máy"
            value={data.selectedMachineId}
            onChange={handleMachineChange}
            suffixIcon={<SearchOutlined />}
            options={form.machineOptions}
            style={{ width: "100%", minWidth: 320 }}
          />
        )}
      />

      <Tabs
        defaultActiveKey="create"
        items={[
          {
            key: "create",
            label: "Tạo khai báo",
            children: (
              <CreateDeclarationTab
                mode={form.mode}
                stagingSlotIndex={form.stagingSlotIndex}
                machineSlotIndex={form.machineSlotIndex}
                orders={form.orders}
                maxTotal={form.maxTotal}
                totalCapacity={form.totalCapacity}
                canAddOrder={form.canAddOrder}
                computedOrders={form.displayComputedOrders}
                orderRows={form.displayComputedOrders}
                orderQuantityCaps={form.orderQuantityCaps}
                cartPreview={form.displayCartPreview}
                selectedOrderIndex={form.selectedOrderIndex}
                selectedOrderSlots={form.selectedOrderSlots}
                validationErrors={form.validationErrors}
                occupiedStagingSlots={form.occupiedStagingSlots}
                selectedMachineStagingSlots={form.selectedMachineStagingSlots}
                busyMachineSlots={form.busyMachineSlots}
                isBusyLocked={form.isBusyLocked}
                loadingSlots={data.loadingSlots}
                submitting={data.submitting}
                onRefresh={data.refreshAll}
                onModeChange={form.setMode}
                onSelectStagingSlot={form.setStagingSlotIndex}
                onMachineSlotChange={form.setMachineSlotIndex}
                onAddOrder={form.addOrder}
                onUpdateOrder={form.updateOrder}
                onResolveModelOrder={form.resolveModelOrder}
                onRemoveOrder={form.removeOrder}
                onSelectOrder={form.setSelectedOrderIndex}
                onSubmit={handleSubmit}
              />
            )
          },
          {
            key: "history",
            label: `Lịch sử khai báo (${data.history.length})`,
            children: (
              <HistoryDeclarationTab
                history={data.history}
                loadingHistory={data.loadingHistory}
                onCancel={data.cancelDeclaration}
              />
            )
          }
        ]}
      />
    </Space>
  );
}

export default ShelfDeclarationPage;
