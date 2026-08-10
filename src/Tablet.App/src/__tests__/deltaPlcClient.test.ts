import {Buffer} from 'buffer';
import {DeltaPlcClient, floatToWords} from '@sti/delta-plc';
import {PlcClient, type PlcTransport} from '../plc/PlcClient';
import type {LineConfig, PlcTag} from '../types/plc';

class FakeTransport implements PlcTransport {
  isConnected = true;
  frames: Buffer[] = [];
  connect = jest.fn(async () => undefined);
  disconnect = jest.fn();
  request = jest.fn(async (frame: Buffer) => {
    this.frames.push(frame);
    const handler = this.handlers.shift();
    if (!handler) {
      throw new Error('No fake Modbus response was queued.');
    }
    return handler(frame);
  });

  constructor(private readonly handlers: Array<(frame: Buffer) => Buffer>) {}
}

const response = (frame: Buffer, pdu: number[]) => {
  const header = Buffer.alloc(7);
  header.writeUInt16BE(frame.readUInt16BE(0), 0);
  header.writeUInt16BE(0, 2);
  header.writeUInt16BE(pdu.length + 1, 4);
  header.writeUInt8(frame.readUInt8(6), 6);
  return Buffer.concat([header, Buffer.from(pdu)]);
};

const holdingResponse = (words: number[]) => (frame: Buffer) => {
  const bytes: number[] = [0x03, words.length * 2];
  words.forEach(word => {
    bytes.push((word >> 8) & 0xff, word & 0xff);
  });
  return response(frame, bytes);
};

const bitResponse = (functionCode: number, values: boolean[]) => (frame: Buffer) => {
  const byteCount = Math.ceil(values.length / 8);
  const bytes = Array.from({length: byteCount}, () => 0);
  values.forEach((value, index) => {
    if (value) {
      bytes[Math.floor(index / 8)] |= 1 << (index % 8);
    }
  });
  return response(frame, [functionCode, byteCount, ...bytes]);
};

const writeAck = (frame: Buffer) => response(frame, Array.from(frame.subarray(7, 12)));

const tag = (name: string, address: string, dataType: PlcTag['dataType'] = 'Bool', writable = false): PlcTag => ({
  name,
  address,
  dataType,
  description: name,
  writable,
});

describe('DeltaPlcClient', () => {
  it('maps Delta DVP D registers before building Modbus requests', async () => {
    const transport = new FakeTransport([holdingResponse([123])]);
    const client = new DeltaPlcClient(
      {host: '127.0.0.1', port: 502, slaveId: 1, connectionType: 'TcpDVP'},
      () => transport,
    );

    await expect(client.readD(10, 1)).resolves.toEqual([123]);

    expect(transport.frames[0].toString('hex')).toBe('0001000000060103100a0001');
  });

  it('reads tag snapshots across coil, discrete, and holding ranges', async () => {
    const floatWords = floatToWords(12.5);
    const transport = new FakeTransport([
      bitResponse(0x01, [true]),
      bitResponse(0x02, [true]),
      holdingResponse([0b10, 0, floatWords[0], floatWords[1]]),
    ]);
    const client = new DeltaPlcClient(
      {host: '127.0.0.1', port: 502, slaveId: 1, connectionType: 'TcpAS'},
      () => transport,
    );

    const snapshot = await client.readTags([
      tag('coil.m1', 'M1'),
      tag('input.x2', 'X2'),
      tag('status.d10_1', 'D10.1'),
      tag('value.float', 'D12', 'Float'),
    ]);

    expect(snapshot).toMatchObject({
      'coil.m1': true,
      'input.x2': true,
      'status.d10_1': true,
      'value.float': 12.5,
    });
    expect(transport.frames.map(frame => frame.toString('hex'))).toEqual([
      '000100000006010100010001',
      '000200000006010260020001',
      '0003000000060103000a0004',
    ]);
  });

  it('writes D-bit tags by masking the current holding register word', async () => {
    const transport = new FakeTransport([
      holdingResponse([0b100]),
      writeAck,
    ]);
    const client = new DeltaPlcClient(
      {host: '127.0.0.1', port: 502, slaveId: 1, connectionType: 'TcpAS'},
      () => transport,
    );

    await client.writeTag(tag('status.d10_1', 'D10.1', 'Bool', true), true);

    expect(transport.frames.map(frame => frame.toString('hex'))).toEqual([
      '0001000000060103000a0001',
      '0002000000060106000a0006',
    ]);
  });

  it('supports Modbus discrete reads and multiple coil writes for Delta areas', async () => {
    const transport = new FakeTransport([
      bitResponse(0x02, [true, false, true]),
      writeAck,
    ]);
    const client = new DeltaPlcClient(
      {host: '127.0.0.1', port: 502, slaveId: 1, connectionType: 'TcpAS'},
      () => transport,
    );

    await expect(client.readX(0, 3)).resolves.toEqual([true, false, true]);
    await client.writeM(10, [true, false, true]);

    expect(transport.frames.map(frame => frame.toString('hex'))).toEqual([
      '000100000006010260000003',
      '000200000008010f000a00030105',
    ]);
  });

  it('exposes raw word and bit helpers for PC debug tools', async () => {
    const transport = new FakeTransport([
      holdingResponse([0xffff, 0x1234]),
      bitResponse(0x01, [true, false]),
      writeAck,
      writeAck,
    ]);
    const client = new DeltaPlcClient(
      {host: '127.0.0.1', port: 502, slaveId: 1, connectionType: 'TcpAS'},
      () => transport,
    );

    await expect(client.readWords('D', 10, 2)).resolves.toEqual([0xffff, 0x1234]);
    await expect(client.readBits('M', 1, 2)).resolves.toEqual([true, false]);
    await client.writeWords('D', 10, [0xffff, 0x1234]);
    await client.writeBits('Y', 5, [true, false, true]);

    expect(transport.frames.map(frame => frame.toString('hex'))).toEqual([
      '0001000000060103000a0002',
      '000200000006010100010002',
      '00030000000b0110000a000204ffff1234',
      '000400000008010fa00500030105',
    ]);
  });

  it('uses the Delta core mapping through the Manual-compatible PlcClient wrapper', async () => {
    const line: LineConfig = {
      id: 'line1',
      name: 'Line 1',
      host: '127.0.0.1',
      port: 502,
      slaveId: 1,
      pollIntervalMs: 100,
      connectionType: 'TcpDVP',
    };
    const transport = new FakeTransport([holdingResponse([42])]);
    const client = new PlcClient(line, [tag('value.d10', 'D10', 'Int16')], () => transport);

    await expect(client.readAll()).resolves.toMatchObject({'value.d10': 42});

    expect(transport.frames[0].toString('hex')).toBe('0001000000060103100a0001');
  });
});
