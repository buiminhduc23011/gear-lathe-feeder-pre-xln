import {Buffer} from 'buffer';
import net from 'node:net';
import type {PlcTransport, TcpPlcEndpoint} from './plcTransport';

interface PendingRequest {
  resolve: (value: Buffer) => void;
  reject: (error: Error) => void;
  timer: ReturnType<typeof setTimeout>;
}

export interface NodeTcpTransportOptions {
  timeoutMs?: number;
  onFrame?: (direction: 'tx' | 'rx', frame: Buffer) => void;
}

export class NodeTcpTransport implements PlcTransport {
  private socket: net.Socket | null = null;
  private connectPromise: Promise<void> | null = null;
  private incoming = Buffer.alloc(0);
  private pending: PendingRequest | null = null;
  private queue: Promise<Buffer> = Promise.resolve(Buffer.alloc(0));
  private readonly timeoutMs: number;

  constructor(
    private readonly endpoint: TcpPlcEndpoint,
    private readonly options: NodeTcpTransportOptions = {},
  ) {
    this.timeoutMs = options.timeoutMs ?? 1500;
  }

  get isConnected(): boolean {
    return this.socket !== null && !this.socket.destroyed;
  }

  connect(): Promise<void> {
    if (this.isConnected) {
      return Promise.resolve();
    }
    if (this.connectPromise) {
      return this.connectPromise;
    }

    this.connectPromise = new Promise((resolve, reject) => {
      const socket = new net.Socket();
      let settled = false;
      const settleReject = (error: Error) => {
        if (settled) {
          return;
        }
        settled = true;
        clearTimeout(timer);
        this.connectPromise = null;
        if (this.socket === socket) {
          this.socket = null;
        }
        socket.destroy();
        reject(error);
      };
      const timer = setTimeout(
        () => settleReject(new Error(`Modbus TCP connect timed out after ${this.timeoutMs} ms.`)),
        this.timeoutMs,
      );

      socket.on('connect', () => {
        if (settled) {
          return;
        }
        settled = true;
        clearTimeout(timer);
        this.socket = socket;
        this.connectPromise = null;
        resolve();
      });
      socket.on('data', chunk => this.handleData(Buffer.isBuffer(chunk) ? chunk : Buffer.from(chunk)));
      socket.on('error', error => {
        this.failPending(error);
        settleReject(error);
      });
      socket.on('close', () => {
        if (this.socket === socket) {
          this.socket = null;
        }
        this.connectPromise = null;
        this.failPending(new Error('Modbus TCP connection closed.'));
        settleReject(new Error('Modbus TCP connection closed.'));
      });

      socket.connect(this.endpoint.port, this.endpoint.host);
    });

    return this.connectPromise;
  }

  disconnect(): void {
    if (this.socket) {
      this.socket.destroy();
      this.socket = null;
    }
    this.connectPromise = null;
    this.incoming = Buffer.alloc(0);
    this.failPending(new Error('Modbus TCP connection disconnected.'));
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
    if (!this.socket || this.socket.destroyed) {
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
      this.options.onFrame?.('tx', frame);
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
      this.options.onFrame?.('rx', frame);
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
}
