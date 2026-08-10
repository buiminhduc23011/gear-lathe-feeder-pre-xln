import TcpSocket from 'react-native-tcp-socket';
import {ModbusTransport} from '../plc/ModbusTransport';
import type {LineConfig} from '../types/plc';

class FakeSocket {
  handlers = new Map<string, Array<(value?: unknown) => void>>();
  destroy = jest.fn();
  write = jest.fn();

  on(event: string, handler: (value?: unknown) => void): void {
    this.handlers.set(event, [...(this.handlers.get(event) ?? []), handler]);
  }

  emit(event: string, value?: unknown): void {
    for (const handler of this.handlers.get(event) ?? []) {
      handler(value);
    }
  }
}

const line: LineConfig = {
  id: 'line1',
  name: 'Line 1',
  host: '127.0.0.1',
  port: 503,
  slaveId: 1,
  pollIntervalMs: 100,
};

const createConnection = TcpSocket.createConnection as jest.Mock;

describe('ModbusTransport connection lifecycle', () => {
  beforeEach(() => {
    createConnection.mockReset();
  });

  it('reuses the in-flight connection attempt', async () => {
    const socket = new FakeSocket();
    let onConnect: (() => void) | undefined;
    createConnection.mockImplementation((_, callback) => {
      onConnect = callback;
      return socket;
    });

    const transport = new ModbusTransport(line);
    const firstConnect = transport.connect();
    const secondConnect = transport.connect();

    expect(createConnection).toHaveBeenCalledTimes(1);
    onConnect?.();
    await expect(Promise.all([firstConnect, secondConnect])).resolves.toEqual([undefined, undefined]);
  });

  it('does not force the native module to preselect a Wi-Fi interface', () => {
    createConnection.mockReturnValue(new FakeSocket());

    const transport = new ModbusTransport(line);
    void transport.connect();

    expect(createConnection).toHaveBeenCalledWith(
      expect.not.objectContaining({interface: expect.any(String)}),
      expect.any(Function),
    );
  });

  it('allows a new attempt after a connection error', async () => {
    const firstSocket = new FakeSocket();
    const secondSocket = new FakeSocket();
    createConnection
      .mockReturnValueOnce(firstSocket)
      .mockReturnValueOnce(secondSocket);

    const transport = new ModbusTransport(line);
    const firstConnect = transport.connect();
    firstSocket.emit('error', new Error('ECONNREFUSED'));

    await expect(firstConnect).rejects.toThrow('ECONNREFUSED');
    void transport.connect();

    expect(createConnection).toHaveBeenCalledTimes(2);
  });
});
