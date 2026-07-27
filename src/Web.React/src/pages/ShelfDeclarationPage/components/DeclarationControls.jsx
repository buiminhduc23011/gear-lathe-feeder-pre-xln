import React from "react";
import { Col, Form, Radio, Row } from "antd";
import {
  DECLARATION_MODES,
  MACHINE_SLOT_OPTIONS
} from "../constants";
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
  onMachineSlotChange
}) {
  return (
    <Row gutter={[12, 8]} align="bottom">
      <Col xs={24} lg={mode === DECLARATION_MODES.AGV ? 4 : 6}>
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
        <Col xs={24} lg={6}>
          <Form.Item label="Machine slot">
            <Radio.Group
              value={machineSlotIndex}
              onChange={(event) => onMachineSlotChange(event.target.value)}
            >
              {MACHINE_SLOT_OPTIONS.map((slot) => (
                <Radio.Button key={slot} value={slot}>
                  Slot {slot}
                </Radio.Button>
              ))}
            </Radio.Group>
          </Form.Item>
        </Col>
      )}

    </Row>
  );
}

export default DeclarationControls;
