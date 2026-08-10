import {Buffer} from 'buffer';
import TcpSocket from 'react-native-tcp-socket';
import type {TcpPlcEndpoint} from '@sti/delta-plc';

interface NativeSocket {
  write(data: Buffer | string): void;
  destroy(): void;
  on(event: 'data', handler: (data: Buffer | string) => void): void;
  on(event: 'error', handler: (error: Error) => void): void;
  on(event: 'close', handler: () => void): void;
}

interface PendingRequest {
  resolve: (value: Buffer) => void;
  reject: (error: Error) => void;
  timer: ReturnType<typeof setTimeout>;
}

export class ModbusTransport {
  private socket: NativeSocket | null = null;
  private connectingSocket: NativeSocket | null = null;
  private connectPromise: Promise<void> | null = null;
  private rejectConnect: ((error: Error) => void) | null = null;
  private incoming = Buffer.alloc(0);
  private pending: PendingRequest | null = null;
  private queue: Promise<Buffer> = Promise.resolve(Buffer.alloc(0));

  constructor(private readonly line: TcpPlcEndpoint, private readonly timeoutMs = 1500) {}

  get isConnected(): boolean {
    return this.socket !== null;
  }

  connect(): Promise<void> {
    if (this.socket) {
      return Promise.resolve();
    }
    if (this.connectPromise) {
      return this.connectPromise;
    }

    this.connectPromise = new Promise((resolve, reject) => {
      let settled = false;
      const socket = TcpSocket.createConnection(
        {
          host: this.line.host,
          port: this.line.port,
          reuseAddress: true,
          connectTimeout: this.timeoutMs,
        },
        () => {
          if (this.connectingSocket !== socket) {
            this.destroySocket(socket as NativeSocket);
            return;
          }
          this.connectingSocket = null;
          this.socket = socket as NativeSocket;
          this.connectPromise = null;
          this.rejectConnect = null;
          settled = true;
          resolve();
        },
      ) as NativeSocket;

      this.connectingSocket = socket;
      const rejectConnect = (error: Error) => {
        if (settled) {
          return;
        }
        settled = true;
        if (this.connectingSocket === socket) {
          this.connectingSocket = null;
        }
        if (this.socket === socket) {
          this.socket = null;
        }
        this.connectPromise = null;
        this.rejectConnect = null;
        reject(error);
      };
      this.rejectConnect = rejectConnect;

      socket.on('data', data => this.handleData(Buffer.isBuffer(data) ? data : Buffer.from(data, 'binary')));
      socket.on('error', error => {
        this.failPending(error);
        rejectConnect(error);
      });
      socket.on('close', () => {
        if (this.connectingSocket === socket) {
          this.connectingSocket = null;
        }
        if (this.socket === socket) {
          this.socket = null;
        }
        this.connectPromise = null;
        this.failPending(new Error('Modbus TCP connection closed.'));
        rejectConnect(new Error('Modbus TCP connection closed.'));
      });
    });
    return this.connectPromise;
  }

  disconnect(): void {
    const connectingSocket = this.connectingSocket;
    const rejectConnect = this.rejectConnect;
    if (rejectConnect) {
      rejectConnect(new Error('Modbus TCP connection disconnected.'));
    } else {
      this.connectingSocket = null;
      this.connectPromise = null;
      this.rejectConnect = null;
    }
    if (connectingSocket) {
      this.destroySocket(connectingSocket);
    }
    if (this.socket) {
      this.destroySocket(this.socket);
      this.socket = null;
    }
    this.failPending(new Error('Modbus TCP connection disconnected.'));
    this.incoming = Buffer.alloc(0);
  }

  request(frame: Buffer): Promise<Buffer> {
    const run = async () => {
      await this.connect();
      return this.writeAndWait(frame);
    };
    this.queue = this.queue.then(run, run);
    return this.queue;
  }

  private writeAndWait(frame: Buffer): Promise<Buffer> {
    if (!this.socket) {
      return Promise.reject(new Error('Modbus TCP socket is not connected.'));
    }
    if (this.pending) {
      return Promise.reject(new Error('A Modbus request is already pending.'));
    }

    return new Promise((resolve, reject) => {
      const timer = setTimeout(() => {
        this.pending = null;
        reject(new Error(`Modbus request timed out after ${this.timeoutMs} ms.`));
      }, this.timeoutMs);

      this.pending = {resolve, reject, timer};
      this.socket?.write(frame);
    });
  }

  private handleData(chunk: Buffer): void {
    this.incoming = Buffer.concat([this.incoming, chunk]);
    while (this.incoming.length >= 7) {
      const length = this.incoming.readUInt16BE(4);
      const frameLength = 6 + length;
      if (this.incoming.length < frameLength) {
        return;
      }
      const frame = Buffer.from(this.incoming.subarray(0, frameLength));
      this.incoming = Buffer.from(this.incoming.subarray(frameLength));
      const pending = this.pending;
      if (!pending) {
        continue;
      }
      this.pending = null;
      clearTimeout(pending.timer);
      pending.resolve(frame);
    }
  }

  private failPending(error: Error): void {
    const pending = this.pending;
    if (!pending) {
      return;
    }
    this.pending = null;
    clearTimeout(pending.timer);
    pending.reject(error);
  }

  private destroySocket(socket: NativeSocket): void {
    try {
      socket.destroy();
    } catch {
      // Native socket teardown can race with failed connection setup.
    }
  }
}
