import React from "react";
import { Col, Form, Radio, Row } from "antd";
import {
  DECLARATION_MODES,
} from "../constants";
import MachineSlotGrid from "./MachineSlotGrid";
import StagingSlotGrid from "./StagingSlotGrid";

function DeclarationControls({
  mode,
  onModeChange,
  loadingSlots,
  occupiedStagingSlots,
  selectedMachineStagingSlots,
  stagingSlotIndex,
  onSelectStagingSlot,
  machineSlotIndex,
  busyMachineSlots,
  onMachineSlotChange
}) {
  return (
    <Row gutter={[12, 8]} align="bottom">
      <Col xs={24} lg={4}>
        <Form.Item label="Mode">
          <Radio.Group value={mode} onChange={(event) => onModeChange(event.target.value)}>
            <Radio.Button value={DECLARATION_MODES.AGV}>AGV</Radio.Button>
            <Radio.Button value={DECLARATION_MODES.MANUAL}>Thủ công</Radio.Button>
          </Radio.Group>
        </Form.Item>
      </Col>

      {mode === DECLARATION_MODES.AGV ? (
        <Col xs={24} lg={20} order={3}>
          <Form.Item label=" ">
            <StagingSlotGrid
              loadingSlots={loadingSlots}
              occupiedStagingSlots={occupiedStagingSlots}
              selectedMachineStagingSlots={selectedMachineStagingSlots}
              stagingSlotIndex={stagingSlotIndex}
              onSelectStagingSlot={onSelectStagingSlot}
            />
          </Form.Item>
        </Col>
      ) : (
        <Col xs={24} lg={20} order={3}>
          <Form.Item label=" ">
            <MachineSlotGrid
              machineSlotIndex={machineSlotIndex}
              busyMachineSlots={busyMachineSlots}
              onSelectMachineSlot={onMachineSlotChange}
            />
          </Form.Item>
        </Col>
      )}

    </Row>
  );
}

export default DeclarationControls;
