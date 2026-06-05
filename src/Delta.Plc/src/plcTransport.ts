import type {Buffer} from 'buffer';

export interface TcpPlcEndpoint {
  host: string;
  port: number;
}

export interface PlcTransport {
  readonly isConnected: boolean;
  connect(): Promise<void>;
  disconnect(): void;
  request(frame: Buffer): Promise<Buffer>;
}

export type PlcTransportFactory<TConfig extends TcpPlcEndpoint = TcpPlcEndpoint> = (config: TConfig) => PlcTransport;
