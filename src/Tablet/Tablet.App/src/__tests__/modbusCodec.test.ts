import {Buffer} from 'buffer';
import {
  buildReadCoilsRequest,
  buildReadDiscreteInputsRequest,
  buildReadHoldingRegistersRequest,
  buildWriteMultipleCoilsRequest,
  buildWriteMultipleRegistersRequest,
  buildWriteSingleCoilRequest,
  parseReadCoilsResponse,
  parseReadDiscreteInputsResponse,
  parseReadHoldingRegistersResponse,
  parseWriteResponse,
  floatToWords,
  stringToWords,
  wordsToFloat,
  wordsToString,
} from '@sti/delta-plc';

const response = (tx: number, unitId: number, pdu: number[]) => {
  const header = Buffer.alloc(7);
  header.writeUInt16BE(tx, 0);
  header.writeUInt16BE(0, 2);
  header.writeUInt16BE(pdu.length + 1, 4);
  header.writeUInt8(unitId, 6);
  return Buffer.concat([header, Buffer.from(pdu)]);
};

describe('Modbus codec', () => {
  it('builds read coil and holding-register requests', () => {
    expect(buildReadCoilsRequest(1, 2, 2000, 8).toString('hex')).toBe('000100000006020107d00008');
    expect(buildReadHoldingRegistersRequest(2, 1, 5150, 2).toString('hex')).toBe('0002000000060103141e0002');
    expect(buildReadDiscreteInputsRequest(8, 1, 0x6000, 4).toString('hex')).toBe('000800000006010260000004');
  });

  it('builds write coil requests', () => {
    expect(buildWriteSingleCoilRequest(3, 1, 2000, true).toString('hex')).toBe('000300000006010507d0ff00');
    expect(buildWriteMultipleCoilsRequest(9, 1, 10, [true, false, true, true, false, false, false, false, true]).toString('hex'))
      .toBe('000900000009010f000a0009020d01');
  });

  it('parses read coils and holding registers', () => {
    expect(parseReadCoilsResponse(response(4, 1, [0x01, 0x01, 0x05]), 3, 4)).toEqual([true, false, true]);
    expect(parseReadDiscreteInputsResponse(response(8, 1, [0x02, 0x01, 0x03]), 2, 8)).toEqual([true, true]);
    expect(parseReadHoldingRegistersResponse(response(5, 1, [0x03, 0x04, 0x00, 0x00, 0x3f, 0x80]), 2, 5)).toEqual([0, 0x3f80]);
  });

  it('round-trips Delta float and string register values', () => {
    const floatWords = floatToWords(12.5);
    expect(wordsToFloat(floatWords[0], floatWords[1])).toBeCloseTo(12.5);

    const stringWords = stringToWords('MODEL-A', 10);
    expect(wordsToString(stringWords, 10)).toBe('MODEL-A');
    expect(buildWriteMultipleRegistersRequest(6, 1, 5300, stringWords).readUInt8(7)).toBe(0x10);
  });

  it('parses write acknowledgements', () => {
    expect(parseWriteResponse(response(7, 1, [0x05, 0x07, 0xd0, 0xff, 0x00]), 7).functionCode).toBe(0x05);
    expect(parseWriteResponse(response(9, 1, [0x0f, 0x00, 0x0a, 0x00, 0x09]), 9).functionCode).toBe(0x0f);
  });
});
