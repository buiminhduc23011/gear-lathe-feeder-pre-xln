# @sti/delta-plc

> **[ARCHIVE / BẢN LƯU TRỮ]**: Thư mục Tablet và package này chỉ giữ lại làm tài liệu lưu trữ, không còn được phát triển hay sử dụng chính thức.

Reusable Delta PLC Modbus utilities extracted from the tablet HMI app.

The package contains the protocol-independent core:

- Delta AS/DVP Modbus address mapping
- Modbus TCP frame codecs
- Delta PLC read/write client
- PLC tag binding and read-range helpers
- Delta word codecs for Int16, Int32, Float, and String values

Transport is injected through `PlcTransportFactory`, so each app can provide its own TCP, simulator, or test transport.

```ts
import {DeltaPlcClient, type PlcTransportFactory} from '@sti/delta-plc';

const transportFactory: PlcTransportFactory = config => createYourTransport(config);

const client = new DeltaPlcClient(
  {host: '192.168.1.10', port: 502, slaveId: 1, connectionType: 'TcpAS'},
  transportFactory,
);

const values = await client.readD(5150, 2);
```

Build or pack the library before sharing it with another project:

```sh
npm install
npm run build
npm pack
```

## PC Debug CLI

The package includes a Node.js Modbus TCP transport and CLI for PowerShell debugging against a real PLC.
It uses the same `DeltaPlcClient`, address mapping, codecs, and word conversion logic as app consumers.

```powershell
cd src\Delta.Plc
npm install
npm run build
```

List supported Delta AS/DVP areas:

```powershell
npm run debug:profiles
```

Read/write Delta AS:

```powershell
npm run debug:read -- --host 192.168.1.10 --connection TcpAS --address D5150 --count 2 --trace
npm run debug:write -- --host 192.168.1.10 --connection TcpAS --address M2000 --value true --trace
```

Read/write Delta DVP:

```powershell
npm run debug:read -- --host 192.168.1.20 --connection TcpDVP --address D10 --data-type int16
npm run debug:write -- --host 192.168.1.20 --connection TcpDVP --address D10 --values 123,456
```

Common data types:

- `word`: raw 16-bit registers with decimal and hex output
- `int16`: signed 16-bit values
- `int32`: two-word signed values at `D<n>` or `DI<n>`
- `float`: two-word float values at `D<n>` or `R<n>`
- `string`: string values, use `--length`
- `bcd`: BCD encoded `D` register
- `bool`: bit/coils such as `M`, `Y`, `S`, `T`, `C`, plus `D<n>.<bit>`

`X` inputs are read-only. Unsupported areas for a connection type are rejected by the same Delta mapping rules used by the app.
