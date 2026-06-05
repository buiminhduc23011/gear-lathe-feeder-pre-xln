import {Buffer} from 'buffer';

export interface ModbusResponse {
  transactionId: number;
  unitId: number;
  functionCode: number;
  payload: Buffer;
}

const buildFrame = (transactionId: number, unitId: number, pdu: Buffer): Buffer => {
  const header = Buffer.alloc(7);
  header.writeUInt16BE(transactionId & 0xffff, 0);
  header.writeUInt16BE(0, 2);
  header.writeUInt16BE(pdu.length + 1, 4);
  header.writeUInt8(unitId & 0xff, 6);
  return Buffer.concat([header, pdu]);
};

export const buildReadCoilsRequest = (
  transactionId: number,
  unitId: number,
  startAddress: number,
  quantity: number,
): Buffer => {
  const pdu = Buffer.alloc(5);
  pdu.writeUInt8(0x01, 0);
  pdu.writeUInt16BE(startAddress, 1);
  pdu.writeUInt16BE(quantity, 3);
  return buildFrame(transactionId, unitId, pdu);
};

export const buildReadHoldingRegistersRequest = (
  transactionId: number,
  unitId: number,
  startAddress: number,
  quantity: number,
): Buffer => {
  const pdu = Buffer.alloc(5);
  pdu.writeUInt8(0x03, 0);
  pdu.writeUInt16BE(startAddress, 1);
  pdu.writeUInt16BE(quantity, 3);
  return buildFrame(transactionId, unitId, pdu);
};

export const buildReadDiscreteInputsRequest = (
  transactionId: number,
  unitId: number,
  startAddress: number,
  quantity: number,
): Buffer => {
  const pdu = Buffer.alloc(5);
  pdu.writeUInt8(0x02, 0);
  pdu.writeUInt16BE(startAddress, 1);
  pdu.writeUInt16BE(quantity, 3);
  return buildFrame(transactionId, unitId, pdu);
};

export const buildWriteSingleCoilRequest = (
  transactionId: number,
  unitId: number,
  address: number,
  value: boolean,
): Buffer => {
  const pdu = Buffer.alloc(5);
  pdu.writeUInt8(0x05, 0);
  pdu.writeUInt16BE(address, 1);
  pdu.writeUInt16BE(value ? 0xff00 : 0x0000, 3);
  return buildFrame(transactionId, unitId, pdu);
};

export const buildWriteMultipleCoilsRequest = (
  transactionId: number,
  unitId: number,
  address: number,
  values: boolean[],
): Buffer => {
  const byteCount = Math.ceil(values.length / 8);
  const pdu = Buffer.alloc(6 + byteCount);
  pdu.writeUInt8(0x0f, 0);
  pdu.writeUInt16BE(address, 1);
  pdu.writeUInt16BE(values.length, 3);
  pdu.writeUInt8(byteCount, 5);
  values.forEach((value, index) => {
    if (value) {
      const offset = 6 + Math.floor(index / 8);
      pdu.writeUInt8(pdu.readUInt8(offset) | (1 << (index % 8)), offset);
    }
  });
  return buildFrame(transactionId, unitId, pdu);
};

export const buildWriteSingleRegisterRequest = (
  transactionId: number,
  unitId: number,
  address: number,
  value: number,
): Buffer => {
  const pdu = Buffer.alloc(5);
  pdu.writeUInt8(0x06, 0);
  pdu.writeUInt16BE(address, 1);
  pdu.writeUInt16BE(value & 0xffff, 3);
  return buildFrame(transactionId, unitId, pdu);
};

export const buildWriteMultipleRegistersRequest = (
  transactionId: number,
  unitId: number,
  address: number,
  values: number[],
): Buffer => {
  const byteCount = values.length * 2;
  const pdu = Buffer.alloc(6 + byteCount);
  pdu.writeUInt8(0x10, 0);
  pdu.writeUInt16BE(address, 1);
  pdu.writeUInt16BE(values.length, 3);
  pdu.writeUInt8(byteCount, 5);
  values.forEach((word, index) => pdu.writeUInt16BE(word & 0xffff, 6 + index * 2));
  return buildFrame(transactionId, unitId, pdu);
};

export const parseModbusResponse = (frame: Buffer, expectedTransactionId?: number): ModbusResponse => {
  if (frame.length < 8) {
    throw new Error('Modbus response is too short.');
  }

  const transactionId = frame.readUInt16BE(0);
  const protocolId = frame.readUInt16BE(2);
  const length = frame.readUInt16BE(4);
  const expectedLength = 6 + length;

  if (protocolId !== 0) {
    throw new Error(`Unexpected Modbus protocol id ${protocolId}.`);
  }
  if (expectedTransactionId !== undefined && transactionId !== expectedTransactionId) {
    throw new Error(`Unexpected Modbus transaction id ${transactionId}; expected ${expectedTransactionId}.`);
  }
  if (frame.length < expectedLength) {
    throw new Error('Modbus response frame is incomplete.');
  }

  const unitId = frame.readUInt8(6);
  const functionCode = frame.readUInt8(7);
  const payload = Buffer.from(frame.subarray(8, expectedLength));

  if ((functionCode & 0x80) !== 0) {
    const exceptionCode = payload.length > 0 ? payload.readUInt8(0) : -1;
    throw new Error(`Modbus exception ${exceptionCode} for function ${functionCode & 0x7f}.`);
  }

  return {transactionId, unitId, functionCode, payload};
};

const parseReadBitsResponse = (
  frame: Buffer,
  expectedQuantity: number,
  expectedFunctionCode: number,
  expectedFunctionName: string,
  expectedTransactionId?: number,
): boolean[] => {
  const response = parseModbusResponse(frame, expectedTransactionId);
  if (response.functionCode !== expectedFunctionCode) {
    throw new Error(`Unexpected function ${response.functionCode}; expected ${expectedFunctionName}.`);
  }
  const byteCount = response.payload.readUInt8(0);
  const values: boolean[] = [];
  for (let byteIndex = 0; byteIndex < byteCount; byteIndex += 1) {
    const byteValue = response.payload.readUInt8(1 + byteIndex);
    for (let bit = 0; bit < 8 && values.length < expectedQuantity; bit += 1) {
      values.push((byteValue & (1 << bit)) !== 0);
    }
  }
  return values;
};

export const parseReadCoilsResponse = (
  frame: Buffer,
  expectedQuantity: number,
  expectedTransactionId?: number,
): boolean[] => parseReadBitsResponse(frame, expectedQuantity, 0x01, 'read coils', expectedTransactionId);

export const parseReadDiscreteInputsResponse = (
  frame: Buffer,
  expectedQuantity: number,
  expectedTransactionId?: number,
): boolean[] => parseReadBitsResponse(frame, expectedQuantity, 0x02, 'read discrete inputs', expectedTransactionId);

export const parseReadHoldingRegistersResponse = (
  frame: Buffer,
  expectedQuantity: number,
  expectedTransactionId?: number,
): number[] => {
  const response = parseModbusResponse(frame, expectedTransactionId);
  if (response.functionCode !== 0x03) {
    throw new Error(`Unexpected function ${response.functionCode}; expected read holding registers.`);
  }
  const byteCount = response.payload.readUInt8(0);
  if (byteCount !== expectedQuantity * 2) {
    throw new Error(`Unexpected holding register byte count ${byteCount}.`);
  }
  const words: number[] = [];
  for (let index = 0; index < expectedQuantity; index += 1) {
    words.push(response.payload.readUInt16BE(1 + index * 2));
  }
  return words;
};

export const parseWriteResponse = (frame: Buffer, expectedTransactionId?: number): ModbusResponse => {
  const response = parseModbusResponse(frame, expectedTransactionId);
  if (![0x05, 0x06, 0x0f, 0x10].includes(response.functionCode)) {
    throw new Error(`Unexpected function ${response.functionCode}; expected write response.`);
  }
  return response;
};
