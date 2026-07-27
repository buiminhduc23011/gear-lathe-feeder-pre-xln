import type {LineConfig, PlcDataType, PlcSnapshot, PlcTag} from '../types/plc';
import {DeltaPlcClient, type PlcTransportFactory} from '@sti/delta-plc';
import {ModbusTransport} from './ModbusTransport';
export type {PlcTransport, PlcTransportFactory} from '@sti/delta-plc';

export class PlcClient {
  private readonly tagLookup: Map<string, PlcTag>;
  private readonly client: DeltaPlcClient;
  private readonly cache: PlcSnapshot = {};

  constructor(
    line: LineConfig,
    private readonly tags: PlcTag[],
    transportFactory?: PlcTransportFactory<LineConfig>,
  ) {
    this.tagLookup = new Map(tags.map(tag => [tag.name.toLowerCase(), tag]));
    const normalizedLine = {...line, connectionType: line.connectionType ?? 'TcpAS'} as LineConfig;
    this.client = new DeltaPlcClient(
      {
        host: normalizedLine.host,
        port: normalizedLine.port,
        slaveId: normalizedLine.slaveId,
        connectionType: normalizedLine.connectionType,
      },
      transportFactory ? () => transportFactory(normalizedLine) : nextConfig => new ModbusTransport(nextConfig),
    );

    for (const tag of tags) {
      this.cache[tag.name] = defaultValue(tag.dataType);
    }
  }

  get isConnected(): boolean {
    return this.client.isConnected;
  }

  async connect(): Promise<void> {
    await this.client.connect();
  }

  disconnect(): void {
    this.client.disconnect();
  }

  getValue<T>(tagName: string, fallback: T): T {
    const value = this.cache[tagName];
    return value === undefined || value === null ? fallback : (value as T);
  }

  snapshot(): PlcSnapshot {
    return {...this.cache};
  }

  async readAll(): Promise<PlcSnapshot> {
    Object.assign(this.cache, await this.client.readTags(this.tags));

    return this.snapshot();
  }

  async write(tagName: string, value: unknown): Promise<void> {
    const tag = this.tagLookup.get(tagName.toLowerCase());
    if (!tag) {
      throw new Error(`Unknown PLC tag: ${tagName}`);
    }
    if (!tag.writable) {
      throw new Error(`PLC tag '${tagName}' is read-only and cannot be written.`);
    }

    const normalized = normalizeValue(tag, value);
    await this.client.writeTag(tag, normalized);
    this.cache[tag.name] = normalized;
  }
}

const normalizeValue = (tag: PlcTag, value: unknown): unknown => {
  switch (tag.dataType) {
    case 'Bool':
      if (typeof value === 'boolean') {
        return value;
      }
      if (typeof value === 'string') {
        return value === 'true' || value === '1';
      }
      return Number(value) !== 0;
    case 'Int16':
    case 'Int32':
    case 'Float':
      if (value === '' || value === null || value === undefined || Number.isNaN(Number(value))) {
        throw new Error(`Value '${String(value)}' is not valid for tag '${tag.name}'.`);
      }
      return Number(value);
    case 'String':
      return String(value).slice(0, tag.length ?? String(value).length);
    default:
      return value;
  }
};

const defaultValue = (dataType: PlcDataType): unknown => {
  switch (dataType) {
    case 'Bool':
      return false;
    case 'Int16':
    case 'Int32':
    case 'Float':
      return 0;
    case 'String':
      return '';
  }
};
