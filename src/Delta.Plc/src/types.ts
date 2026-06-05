export type PlcDataType = 'Bool' | 'Int16' | 'Int32' | 'Float' | 'String';
export type DeltaConnectionType = 'TcpAS' | 'TcpDVP';

export interface PlcTag {
  name: string;
  address: string;
  dataType: PlcDataType;
  description: string;
  length?: number;
  writable?: boolean;
}

export type PlcSnapshot = Record<string, unknown>;
