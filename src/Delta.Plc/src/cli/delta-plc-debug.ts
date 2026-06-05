#!/usr/bin/env node
import {DeltaPlcClient} from '../DeltaPlcClient';
import type {DeltaBitArea, DeltaWritableBitArea, DeltaWordArea} from '../DeltaPlcClient';
import type {DeltaConnectionType, PlcTag} from '../types';
import {toSignedInt16} from '../wordCodecs';
import {NodeTcpTransport} from '../nodeTcpTransport';

type Command = 'read' | 'write' | 'profiles' | 'help';
type DataType = 'word' | 'int16' | 'int32' | 'float' | 'string' | 'bcd' | 'bool';

interface ParsedArgs {
  command: Command;
  options: Record<string, string | boolean>;
  values: string[];
}

interface DeltaAddress {
  area: string;
  index: number;
  bitIndex?: number;
}

const usage = `Delta PLC debug CLI

Usage:
  delta-plc-debug profiles
  delta-plc-debug read  --host <ip> --connection TcpAS  --address D5150 --count 2 [--data-type word] [--trace]
  delta-plc-debug write --host <ip> --connection TcpDVP --address M2000 --values true,false [--data-type bool] [--trace]

Connection options:
  --host <ip>                 PLC IP address
  --port <port>               Modbus TCP port, default 502
  --slave <id>                Modbus unit/slave id, default 1
  --connection <TcpAS|TcpDVP> Delta PLC line mapping, default TcpAS
  --timeout <ms>              Connect/request timeout, default 1500
  --trace                     Print Modbus TCP request/response frames as hex

Address and data options:
  --address <addr>            Delta address: D5150, M2000, X0, Y0, S0, T0, C0, SR0, HC0, E0, DI100, R100, D5032.0
  --count <n>                 Number of values to read, default 1
  --data-type <type>          word, int16, int32, float, string, bcd, bool
  --values <a,b,c>            Values to write
  --value <v>                 Single value to write
  --length <chars>            String length for string reads/writes

Examples:
  npm run debug:read -- --host 192.168.1.10 --connection TcpAS --address D5150 --count 2 --trace
  npm run debug:write -- --host 192.168.1.10 --connection TcpAS --address M2000 --value true
  npm run debug:read -- --host 192.168.1.20 --connection TcpDVP --address D10 --data-type int16
  npm run debug:write -- --host 192.168.1.20 --connection TcpDVP --address D10 --values 123,456
`;

const parseArgs = (argv: string[]): ParsedArgs => {
  const command = (argv[0] ?? 'help') as Command;
  const options: Record<string, string | boolean> = {};
  const values: string[] = [];

  for (let index = 1; index < argv.length; index += 1) {
    const token = argv[index];
    if (!token.startsWith('--')) {
      values.push(token);
      continue;
    }
    const key = token.slice(2);
    const next = argv[index + 1];
    if (!next || next.startsWith('--')) {
      options[key] = true;
      continue;
    }
    options[key] = next;
    index += 1;
  }

  return {
    command: ['read', 'write', 'profiles', 'help'].includes(command) ? command : 'help',
    options,
    values,
  };
};

const option = (args: ParsedArgs, ...keys: string[]): string | undefined => {
  for (const key of keys) {
    const value = args.options[key];
    if (typeof value === 'string') {
      return value;
    }
  }
  return undefined;
};

const flag = (args: ParsedArgs, key: string): boolean => args.options[key] === true;

const intOption = (args: ParsedArgs, fallback: number, ...keys: string[]): number => {
  const raw = option(args, ...keys);
  if (raw === undefined) {
    return fallback;
  }
  const value = Number.parseInt(raw, 10);
  if (!Number.isInteger(value)) {
    throw new Error(`Option --${keys[0]} must be an integer.`);
  }
  return value;
};

const requireOption = (args: ParsedArgs, ...keys: string[]): string => {
  const value = option(args, ...keys);
  if (!value) {
    throw new Error(`Missing required option --${keys[0]}.`);
  }
  return value;
};

const parseAddress = (address: string): DeltaAddress => {
  const match = /^([A-Z]+)(\d+)(?:\.(\d+))?$/.exec(address.trim().toUpperCase());
  if (!match) {
    throw new Error(`Address '${address}' is invalid.`);
  }
  return {
    area: match[1],
    index: Number.parseInt(match[2], 10),
    bitIndex: match[3] === undefined ? undefined : Number.parseInt(match[3], 10),
  };
};

const normalizeConnection = (value: string | undefined): DeltaConnectionType => {
  const normalized = value ?? 'TcpAS';
  if (normalized !== 'TcpAS' && normalized !== 'TcpDVP') {
    throw new Error('--connection must be TcpAS or TcpDVP.');
  }
  return normalized;
};

const normalizeDataType = (raw: string | undefined, address: DeltaAddress): DataType => {
  if (!raw) {
    if (address.bitIndex !== undefined) {
      return 'bool';
    }
    if (address.area === 'DI') {
      return 'int32';
    }
    if (address.area === 'R') {
      return 'float';
    }
    if (['M', 'X', 'Y', 'S'].includes(address.area)) {
      return 'bool';
    }
    return 'word';
  }

  const normalized = raw.toLowerCase();
  if (['word', 'raw', 'uint16', 'uint'].includes(normalized)) {
    return 'word';
  }
  if (['int16', 'int'].includes(normalized)) {
    return 'int16';
  }
  if (['int32', 'dint', 'di'].includes(normalized)) {
    return 'int32';
  }
  if (['float', 'real', 'r'].includes(normalized)) {
    return 'float';
  }
  if (['string', 'str'].includes(normalized)) {
    return 'string';
  }
  if (normalized === 'bcd') {
    return 'bcd';
  }
  if (['bool', 'bit', 'coil'].includes(normalized)) {
    return 'bool';
  }
  throw new Error(`Unsupported data type '${raw}'.`);
};

const wordArea = (area: string): DeltaWordArea => {
  if (['D', 'T', 'C', 'SR', 'HC', 'E'].includes(area)) {
    return area as DeltaWordArea;
  }
  throw new Error(`Area ${area} is not a word/register area.`);
};

const bitArea = (area: string): DeltaBitArea => {
  if (['M', 'X', 'Y', 'S', 'T', 'C'].includes(area)) {
    return area as DeltaBitArea;
  }
  throw new Error(`Area ${area} is not a bit area.`);
};

const writableBitArea = (area: string): DeltaWritableBitArea => {
  if (area === 'X') {
    throw new Error('X discrete inputs are read-only.');
  }
  return bitArea(area) as DeltaWritableBitArea;
};

const parseValues = (args: ParsedArgs): string[] => {
  const fromOption = option(args, 'values') ?? option(args, 'value');
  const rawValues = fromOption ? fromOption.split(',') : args.values;
  const values = rawValues.map(value => value.trim()).filter(Boolean);
  if (values.length === 0) {
    throw new Error('Write command requires --value, --values, or positional values.');
  }
  return values;
};

const numberValues = (args: ParsedArgs): number[] =>
  parseValues(args).map(value => {
    const next = Number(value);
    if (Number.isNaN(next)) {
      throw new Error(`Value '${value}' is not a number.`);
    }
    return next;
  });

const boolValues = (args: ParsedArgs): boolean[] =>
  parseValues(args).map(value => ['true', '1', 'on', 'yes'].includes(value.toLowerCase()));

const hexWord = (value: number): string => `0x${(value & 0xffff).toString(16).padStart(4, '0')}`;

const createClient = (args: ParsedArgs): DeltaPlcClient => {
  const host = requireOption(args, 'host');
  const port = intOption(args, 502, 'port');
  const slaveId = intOption(args, 1, 'slave', 'slaveId', 'unit');
  const timeoutMs = intOption(args, 1500, 'timeout');
  const connectionType = normalizeConnection(option(args, 'connection', 'type', 'plc'));

  return new DeltaPlcClient(
    {host, port, slaveId, connectionType},
    config => new NodeTcpTransport(config, {
      timeoutMs,
      onFrame: flag(args, 'trace')
        ? (direction, frame) => console.error(`[${direction}] ${frame.toString('hex')}`)
        : undefined,
    }),
  );
};

const readString = async (client: DeltaPlcClient, address: string, length: number): Promise<string> => {
  const tag: PlcTag = {name: 'debug.string', address, dataType: 'String', description: address, length};
  const snapshot = await client.readTags([tag]);
  return String(snapshot[tag.name] ?? '');
};

const writeString = async (client: DeltaPlcClient, address: string, value: string, length: number): Promise<void> => {
  const tag: PlcTag = {name: 'debug.string', address, dataType: 'String', description: address, length, writable: true};
  await client.writeTag(tag, value);
};

const readDbit = async (client: DeltaPlcClient, address: string): Promise<boolean> => {
  const tag: PlcTag = {name: 'debug.bit', address, dataType: 'Bool', description: address};
  const snapshot = await client.readTags([tag]);
  return Boolean(snapshot[tag.name]);
};

const writeDbit = async (client: DeltaPlcClient, address: string, value: boolean): Promise<void> => {
  const tag: PlcTag = {name: 'debug.bit', address, dataType: 'Bool', description: address, writable: true};
  await client.writeTag(tag, value);
};

const runRead = async (args: ParsedArgs): Promise<void> => {
  const addressText = requireOption(args, 'address');
  const address = parseAddress(addressText);
  const dataType = normalizeDataType(option(args, 'data-type', 'dataType', 'format'), address);
  const count = intOption(args, 1, 'count');
  const length = intOption(args, Math.max(1, count * 2), 'length');
  const client = createClient(args);

  try {
    let values: unknown;
    if (dataType === 'bool') {
      values = address.bitIndex !== undefined
        ? [await readDbit(client, addressText)]
        : await client.readBits(bitArea(address.area), address.index, count);
    } else if (dataType === 'word') {
      const words = await client.readWords(wordArea(address.area), address.index, count);
      values = words.map((value, index) => ({
        address: `${address.area}${address.index + index}`,
        value,
        hex: hexWord(value),
      }));
    } else if (dataType === 'int16') {
      values = (await client.readWords(wordArea(address.area), address.index, count)).map(toSignedInt16);
    } else if (dataType === 'int32') {
      values = await client.readDIntArray(address.area === 'DI' ? address.index : address.index, count);
    } else if (dataType === 'float') {
      values = await client.readFloatArray(address.area === 'R' ? address.index : address.index, count);
    } else if (dataType === 'bcd') {
      values = [await client.readBCD(address.index)];
    } else {
      values = [await readString(client, addressText, length)];
    }

    console.log(JSON.stringify({
      operation: 'read',
      connection: option(args, 'connection', 'type', 'plc') ?? 'TcpAS',
      address: addressText.toUpperCase(),
      dataType,
      count,
      values,
    }, null, 2));
  } finally {
    client.disconnect();
  }
};

const runWrite = async (args: ParsedArgs): Promise<void> => {
  const addressText = requireOption(args, 'address');
  const address = parseAddress(addressText);
  const dataType = normalizeDataType(option(args, 'data-type', 'dataType', 'format'), address);
  const length = intOption(args, Math.max(1, (option(args, 'value') ?? option(args, 'values') ?? '').length), 'length');
  const client = createClient(args);

  try {
    let written: unknown;
    if (dataType === 'bool') {
      const values = boolValues(args);
      if (address.bitIndex !== undefined) {
        await writeDbit(client, addressText, values[0]);
      } else {
        await client.writeBits(writableBitArea(address.area), address.index, values);
      }
      written = values;
    } else if (dataType === 'word' || dataType === 'int16') {
      const values = numberValues(args);
      await client.writeWords(wordArea(address.area), address.index, values);
      written = values.map(value => ({value, hex: hexWord(value)}));
    } else if (dataType === 'int32') {
      const values = numberValues(args);
      await client.writeDIntArray(address.index, values);
      written = values;
    } else if (dataType === 'float') {
      const values = numberValues(args);
      await client.writeFloatArray(address.index, values);
      written = values;
    } else if (dataType === 'bcd') {
      const values = numberValues(args);
      await client.writeBCD(address.index, values[0]);
      written = [values[0]];
    } else {
      const value = parseValues(args).join(' ');
      await writeString(client, addressText, value, length);
      written = [value];
    }

    console.log(JSON.stringify({
      operation: 'write',
      connection: option(args, 'connection', 'type', 'plc') ?? 'TcpAS',
      address: addressText.toUpperCase(),
      dataType,
      written,
      ok: true,
    }, null, 2));
  } finally {
    client.disconnect();
  }
};

const printProfiles = (): void => {
  console.log(`Supported Delta connection mappings

TcpAS:
  Word/register: D0-D29999, T0-T2047, C0-C1023, SR0-SR511, HC0-HC255, E0-E255
  Bit:           M0-M8191, X0-X1023 (read-only), Y0-Y1023, S0-S1023, T0-T2047, C0-C1023
  Typed aliases: DI<n> = DInt at D<n>, R<n> = Float at D<n>, D<n>.<bit> = D-bit

TcpDVP:
  Word/register: D0-D4999, T0-T255, C0-C255
  Bit:           M0-M4095, X0-X255 (read-only), Y0-Y255, S0-S999, T0-T255, C0-C255
  Typed aliases: DI<n> = DInt at D<n>, R<n> = Float at D<n>, D<n>.<bit> = D-bit

Write is blocked for X because Delta X inputs are read-only.`);
};

const main = async (): Promise<void> => {
  const args = parseArgs(process.argv.slice(2));
  if (args.command === 'help') {
    console.log(usage);
    return;
  }
  if (args.command === 'profiles') {
    printProfiles();
    return;
  }
  if (args.command === 'read') {
    await runRead(args);
    return;
  }
  await runWrite(args);
};

main().catch(error => {
  console.error(error instanceof Error ? error.message : String(error));
  process.exitCode = 1;
});
