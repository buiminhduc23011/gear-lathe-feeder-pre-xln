import {
  DeltaPlcClient as CoreDeltaPlcClient,
  type DeltaTcpClientConfig,
  type PlcTransportFactory,
} from '@sti/delta-plc';
import {ModbusTransport} from './ModbusTransport';

export type {DeltaTcpClientConfig} from '@sti/delta-plc';

export class DeltaPlcClient extends CoreDeltaPlcClient {
  constructor(
    config: DeltaTcpClientConfig,
    transportFactory: PlcTransportFactory<DeltaTcpClientConfig> = nextConfig => new ModbusTransport(nextConfig),
  ) {
    super(config, transportFactory);
  }
}
