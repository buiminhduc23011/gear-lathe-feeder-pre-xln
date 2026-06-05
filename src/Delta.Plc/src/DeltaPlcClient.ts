import type {DeltaConnectionType, PlcDataType, PlcSnapshot, PlcTag} from './types';
import {createBinding, createBindingLookup, type PlcArea, type PlcTagBinding} from './addressing';
import {defaultDeltaConnectionType, getDeltaModbusAddress, type DeltaMemoryArea} from './deltaAddressMap';
import {
  buildReadCoilsRequest,
  buildReadDiscreteInputsRequest,
  buildReadHoldingRegistersRequest,
  buildWriteMultipleCoilsRequest,
  buildWriteMultipleRegistersRequest,
  buildWriteSingleCoilRequest,
  buildWriteSingleRegisterRequest,
  parseReadCoilsResponse,
  parseReadDiscreteInputsResponse,
  parseReadHoldingRegistersResponse,
  parseWriteResponse,
} from './modbusCodec';
import type {PlcTransport, PlcTransportFactory, TcpPlcEndpoint} from './plcTransport';
import {buildReadRanges} from './readRanges';
import {
  floatToWords,
  int32ToWords,
  normalizeWord,
  stringToWords,
  toSignedInt16,
  wordsToFloat,
  wordsToInt32,
  wordsToString,
} from './wordCodecs';

export interface DeltaTcpClientConfig extends TcpPlcEndpoint {
  slaveId: number;
  connectionType?: DeltaConnectionType;
}

type DeltaGenericArea = DeltaMemoryArea | 'DI' | 'R';
export type DeltaWordArea = Extract<DeltaMemoryArea, 'D' | 'T' | 'C' | 'SR' | 'HC' | 'E'>;
export type DeltaBitArea = Extract<DeltaMemoryArea, 'M' | 'X' | 'Y' | 'S' | 'T' | 'C'>;
export type DeltaWritableBitArea = Exclude<DeltaBitArea, 'X'>;

export class DeltaPlcClient {
  private readonly transport: PlcTransport;
  private readonly connectionType: DeltaConnectionType;
  private transactionId = 1;

  constructor(
    private readonly config: DeltaTcpClientConfig,
    transportFactory: PlcTransportFactory<DeltaTcpClientConfig>,
  ) {
    this.connectionType = config.connectionType ?? defaultDeltaConnectionType;
    this.transport = transportFactory({...config, connectionType: this.connectionType});
  }

  get isConnected(): boolean {
    return this.transport.isConnected;
  }

  async connect(): Promise<void> {
    await this.transport.connect();
  }

  disconnect(): void {
    this.transport.disconnect();
  }

  async readD(start: number, count: number): Promise<number[]> {
    const words = await this.readHoldingRegistersAt(this.address('D', start), count);
    return words.map(toSignedInt16);
  }

  async writeD(start: number, values: number[]): Promise<void> {
    await this.writeHoldingRegistersAt(this.address('D', start), values.map(normalizeWord));
  }

  async readDInt(start: number): Promise<number> {
    const words = await this.readHoldingRegistersAt(this.address('D', start), 2);
    return wordsToInt32(words[0], words[1]);
  }

  async readDIntArray(start: number, count: number): Promise<number[]> {
    const words = await this.readHoldingRegistersAt(this.address('D', start), count * 2);
    return Array.from({length: count}, (_, index) => wordsToInt32(words[index * 2], words[index * 2 + 1]));
  }

  async writeDInt(start: number, value: number): Promise<void> {
    await this.writeHoldingRegistersAt(this.address('D', start), int32ToWords(value));
  }

  async writeDIntArray(start: number, values: number[]): Promise<void> {
    await this.writeHoldingRegistersAt(this.address('D', start), values.flatMap(value => int32ToWords(value)));
  }

  async readFloat(start: number): Promise<number> {
    const words = await this.readHoldingRegistersAt(this.address('D', start), 2);
    return wordsToFloat(words[0], words[1]);
  }

  async readFloatArray(start: number, count: number): Promise<number[]> {
    const words = await this.readHoldingRegistersAt(this.address('D', start), count * 2);
    return Array.from({length: count}, (_, index) => wordsToFloat(words[index * 2], words[index * 2 + 1]));
  }

  async writeFloat(start: number, value: number): Promise<void> {
    await this.writeHoldingRegistersAt(this.address('D', start), floatToWords(value));
  }

  async writeFloatArray(start: number, values: number[]): Promise<void> {
    await this.writeHoldingRegistersAt(this.address('D', start), values.flatMap(value => floatToWords(value)));
  }

  async readM(start: number, count: number): Promise<boolean[]> {
    return this.readBitsAt('coil', this.address('M', start), count);
  }

  async writeM(start: number, values: boolean[]): Promise<void> {
    await this.writeCoilsAt(this.address('M', start), values);
  }

  async readX(start: number, count: number): Promise<boolean[]> {
    return this.readBitsAt('discrete', this.address('X', start), count);
  }

  async readY(start: number, count: number): Promise<boolean[]> {
    return this.readBitsAt('coil', this.address('Y', start), count);
  }

  async writeY(start: number, values: boolean[]): Promise<void> {
    await this.writeCoilsAt(this.address('Y', start), values);
  }

  async readS(start: number, count: number): Promise<boolean[]> {
    return this.readBitsAt('coil', this.address('S', start), count);
  }

  async writeS(start: number, values: boolean[]): Promise<void> {
    await this.writeCoilsAt(this.address('S', start), values);
  }

  async readC(start: number, count: number): Promise<number[]> {
    const words = await this.readHoldingRegistersAt(this.address('C', start), count);
    return words.map(toSignedInt16);
  }

  async writeC(start: number, values: number[]): Promise<void> {
    await this.writeHoldingRegistersAt(this.address('C', start), values.map(normalizeWord));
  }

  async readCStatus(start: number, count: number): Promise<boolean[]> {
    return this.readBitsAt('coil', this.address('C', start), count);
  }

  async readT(start: number, count: number): Promise<number[]> {
    const words = await this.readHoldingRegistersAt(this.address('T', start), count);
    return words.map(toSignedInt16);
  }

  async writeT(start: number, values: number[]): Promise<void> {
    await this.writeHoldingRegistersAt(this.address('T', start), values.map(normalizeWord));
  }

  async readTStatus(start: number, count: number): Promise<boolean[]> {
    return this.readBitsAt('coil', this.address('T', start), count);
  }

  async readBCD(dRegister: number): Promise<number> {
    const words = await this.readHoldingRegistersAt(this.address('D', dRegister), 1);
    return bcdToInt(words[0]);
  }

  async writeBCD(dRegister: number, value: number): Promise<void> {
    await this.writeHoldingRegistersAt(this.address('D', dRegister), [intToBcd(value)]);
  }

  async readWords(area: DeltaWordArea, start: number, count: number): Promise<number[]> {
    return this.readHoldingRegistersAt(this.address(area, start), count);
  }

  async writeWords(area: DeltaWordArea, start: number, values: number[]): Promise<void> {
    await this.writeHoldingRegistersAt(this.address(area, start), values);
  }

  async readBits(area: DeltaBitArea, start: number, count: number): Promise<boolean[]> {
    return this.readBitsAt(area === 'X' ? 'discrete' : 'coil', this.address(area, start), count);
  }

  async writeBits(area: DeltaWritableBitArea, start: number, values: boolean[]): Promise<void> {
    await this.writeCoilsAt(this.address(area, start), values);
  }

  async read(address: string, count = 1): Promise<unknown> {
    const parsed = parseGenericAddress(address);
    switch (parsed.area) {
      case 'D':
        return count === 1 ? (await this.readD(parsed.index, 1))[0] : this.readD(parsed.index, count);
      case 'DI':
        return count === 1 ? this.readDInt(parsed.index) : this.readDIntArray(parsed.index, count);
      case 'R':
        return count === 1 ? this.readFloat(parsed.index) : this.readFloatArray(parsed.index, count);
      case 'M':
        return this.readM(parsed.index, count);
      case 'X':
        return this.readX(parsed.index, count);
      case 'Y':
        return this.readY(parsed.index, count);
      case 'S':
        return this.readS(parsed.index, count);
      case 'C':
        return this.readC(parsed.index, count);
      case 'T':
        return this.readT(parsed.index, count);
      case 'SR':
      case 'HC':
      case 'E': {
        const words = await this.readHoldingRegistersAt(this.address(parsed.area, parsed.index), count);
        return count === 1 ? toSignedInt16(words[0]) : words.map(toSignedInt16);
      }
      default:
        throw new Error(`Unsupported Delta PLC area: ${parsed.area}.`);
    }
  }

  async write(address: string, value: unknown): Promise<void> {
    const parsed = parseGenericAddress(address);
    switch (parsed.area) {
      case 'D':
        await this.writeD(parsed.index, toNumberArray(value));
        return;
      case 'DI':
        await this.writeDIntArray(parsed.index, toNumberArray(value));
        return;
      case 'R':
        await this.writeFloatArray(parsed.index, toNumberArray(value));
        return;
      case 'M':
        await this.writeM(parsed.index, toBooleanArray(value));
        return;
      case 'Y':
        await this.writeY(parsed.index, toBooleanArray(value));
        return;
      case 'S':
        await this.writeS(parsed.index, toBooleanArray(value));
        return;
      case 'C':
        await this.writeC(parsed.index, toNumberArray(value));
        return;
      case 'T':
        await this.writeT(parsed.index, toNumberArray(value));
        return;
      case 'SR':
      case 'HC':
      case 'E':
        await this.writeHoldingRegistersAt(this.address(parsed.area, parsed.index), toNumberArray(value).map(normalizeWord));
        return;
      case 'X':
        throw new Error('Delta X discrete inputs are read-only.');
      default:
        throw new Error(`Unsupported Delta PLC area: ${parsed.area}.`);
    }
  }

  async readTags(tags: PlcTag[]): Promise<PlcSnapshot> {
    const bindingLookup = createBindingLookup(tags, this.connectionType);
    const ranges = buildReadRanges([...bindingLookup.values()]);
    const coilValues = new Map<number, boolean>();
    const discreteValues = new Map<number, boolean>();
    const holdingValues = new Map<number, number>();

    for (const range of ranges) {
      if (range.area === 'coil') {
        const values = await this.readBitsAt('coil', range.startAddress, range.count);
        values.forEach((value, index) => coilValues.set(range.startAddress + index, value));
      } else if (range.area === 'discrete') {
        const values = await this.readBitsAt('discrete', range.startAddress, range.count);
        values.forEach((value, index) => discreteValues.set(range.startAddress + index, value));
      } else {
        const values = await this.readHoldingRegistersAt(range.startAddress, range.count);
        values.forEach((value, index) => holdingValues.set(range.startAddress + index, value));
      }
    }

    const snapshot: PlcSnapshot = {};
    for (const binding of bindingLookup.values()) {
      snapshot[binding.tag.name] = readTagValue(binding, coilValues, discreteValues, holdingValues);
    }
    return snapshot;
  }

  async writeTag(tag: PlcTag, value: unknown): Promise<unknown> {
    if (!tag.writable) {
      throw new Error(`PLC tag '${tag.name}' is read-only and cannot be written.`);
    }

    const binding = createBinding(tag, this.connectionType);
    const normalized = normalizeTagValue(tag, value);
    if (tag.dataType === 'Bool') {
      await this.writeBoolBinding(binding, normalized as boolean);
    } else {
      await this.writeHoldingRegistersAt(binding.startAddress, tagValueToWords(tag, normalized));
    }
    return normalized;
  }

  private address(area: DeltaMemoryArea, index: number): number {
    return getDeltaModbusAddress(area, index, this.connectionType);
  }

  private async readHoldingRegistersAt(startAddress: number, count: number): Promise<number[]> {
    validateCount(count, 125, 'register count');
    const tx = this.nextTransactionId();
    const frame = buildReadHoldingRegistersRequest(tx, this.config.slaveId, startAddress, count);
    const response = await this.transport.request(frame);
    return parseReadHoldingRegistersResponse(response, count, tx);
  }

  private async writeHoldingRegistersAt(startAddress: number, values: number[]): Promise<void> {
    validateValues(values, 125, 'register values');
    const tx = this.nextTransactionId();
    const normalized = values.map(normalizeWord);
    const frame =
      normalized.length === 1
        ? buildWriteSingleRegisterRequest(tx, this.config.slaveId, startAddress, normalized[0])
        : buildWriteMultipleRegistersRequest(tx, this.config.slaveId, startAddress, normalized);
    parseWriteResponse(await this.transport.request(frame), tx);
  }

  private async readBitsAt(area: Extract<PlcArea, 'coil' | 'discrete'>, startAddress: number, count: number): Promise<boolean[]> {
    validateCount(count, 2000, 'coil count');
    const tx = this.nextTransactionId();
    const frame =
      area === 'coil'
        ? buildReadCoilsRequest(tx, this.config.slaveId, startAddress, count)
        : buildReadDiscreteInputsRequest(tx, this.config.slaveId, startAddress, count);
    const response = await this.transport.request(frame);
    return area === 'coil'
      ? parseReadCoilsResponse(response, count, tx)
      : parseReadDiscreteInputsResponse(response, count, tx);
  }

  private async writeCoilsAt(startAddress: number, values: boolean[]): Promise<void> {
    validateValues(values, 2000, 'coil values');
    const tx = this.nextTransactionId();
    const frame =
      values.length === 1
        ? buildWriteSingleCoilRequest(tx, this.config.slaveId, startAddress, values[0])
        : buildWriteMultipleCoilsRequest(tx, this.config.slaveId, startAddress, values);
    parseWriteResponse(await this.transport.request(frame), tx);
  }

  private async writeBoolBinding(binding: PlcTagBinding, value: boolean): Promise<void> {
    if (binding.area === 'discrete') {
      throw new Error(`PLC tag '${binding.tag.name}' is mapped to a read-only discrete input.`);
    }
    if (binding.area === 'coil') {
      await this.writeCoilsAt(binding.startAddress, [value]);
      return;
    }
    if (binding.bitIndex === undefined) {
      await this.writeHoldingRegistersAt(binding.startAddress, [value ? 1 : 0]);
      return;
    }

    const currentWord = (await this.readHoldingRegistersAt(binding.startAddress, 1))[0];
    const nextWord = value
      ? currentWord | (1 << binding.bitIndex)
      : currentWord & ~(1 << binding.bitIndex);
    await this.writeHoldingRegistersAt(binding.startAddress, [nextWord]);
  }

  private nextTransactionId(): number {
    const current = this.transactionId;
    this.transactionId = this.transactionId >= 0xffff ? 1 : this.transactionId + 1;
    return current;
  }
}

const parseGenericAddress = (address: string): {area: DeltaGenericArea; index: number} => {
  const match = /^([A-Z]+)(\d+)$/.exec(address.trim().toUpperCase());
  if (!match) {
    throw new Error(`Delta PLC address '${address}' is invalid.`);
  }
  return {area: match[1] as DeltaGenericArea, index: Number.parseInt(match[2], 10)};
};

const validateCount = (count: number, max: number, label: string): void => {
  if (!Number.isInteger(count) || count <= 0 || count > max) {
    throw new Error(`Delta PLC ${label} must be between 1 and ${max}.`);
  }
};

const validateValues = <T>(values: T[], max: number, label: string): void => {
  if (!Array.isArray(values) || values.length === 0 || values.length > max) {
    throw new Error(`Delta PLC ${label} must contain between 1 and ${max} values.`);
  }
};

const toNumberArray = (value: unknown): number[] => {
  const values = Array.isArray(value) ? value : [value];
  return values.map(item => {
    const next = Number(item);
    if (Number.isNaN(next)) {
      throw new Error(`Delta PLC value '${String(item)}' is not a number.`);
    }
    return next;
  });
};

const toBooleanArray = (value: unknown): boolean[] => {
  const values = Array.isArray(value) ? value : [value];
  return values.map(item => {
    if (typeof item === 'boolean') {
      return item;
    }
    if (typeof item === 'string') {
      return item === 'true' || item === '1';
    }
    return Number(item) !== 0;
  });
};

const readTagValue = (
  binding: PlcTagBinding,
  coilValues: ReadonlyMap<number, boolean>,
  discreteValues: ReadonlyMap<number, boolean>,
  holdingValues: ReadonlyMap<number, number>,
): unknown => {
  if (binding.tag.dataType === 'Bool') {
    if (binding.area === 'coil') {
      return coilValues.get(binding.startAddress) ?? false;
    }
    if (binding.area === 'discrete') {
      return discreteValues.get(binding.startAddress) ?? false;
    }
    const word = holdingValues.get(binding.startAddress) ?? 0;
    return binding.bitIndex === undefined ? word !== 0 : (word & (1 << binding.bitIndex)) !== 0;
  }

  const words = Array.from({length: binding.span}, (_, index) => holdingValues.get(binding.startAddress + index) ?? 0);
  switch (binding.tag.dataType) {
    case 'Int16':
      return toSignedInt16(words[0]);
    case 'Int32':
      return wordsToInt32(words[0], words[1]);
    case 'Float':
      return wordsToFloat(words[0], words[1]);
    case 'String':
      return wordsToString(words, binding.tag.length ?? binding.span * 2);
    default:
      return defaultTagValue(binding.tag.dataType);
  }
};

const normalizeTagValue = (tag: PlcTag, value: unknown): unknown => {
  switch (tag.dataType) {
    case 'Bool':
      return toBooleanArray(value)[0];
    case 'Int16':
    case 'Int32':
    case 'Float':
      return toNumberArray(value)[0];
    case 'String':
      return String(value).slice(0, tag.length ?? String(value).length);
    default:
      return value;
  }
};

const tagValueToWords = (tag: PlcTag, value: unknown): number[] => {
  switch (tag.dataType) {
    case 'Int16':
      return [normalizeWord(Number(value))];
    case 'Int32':
      return int32ToWords(Number(value));
    case 'Float':
      return floatToWords(Number(value));
    case 'String':
      return stringToWords(String(value), tag.length ?? String(value).length);
    default:
      return [0];
  }
};

const defaultTagValue = (dataType: PlcDataType): unknown => {
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

const bcdToInt = (word: number): number => {
  let result = 0;
  let multiplier = 1;
  let current = word & 0xffff;
  while (current > 0) {
    const digit = current & 0x0f;
    if (digit > 9) {
      throw new Error(`Delta PLC BCD word 0x${word.toString(16)} contains an invalid digit.`);
    }
    result += digit * multiplier;
    multiplier *= 10;
    current >>= 4;
  }
  return result;
};

const intToBcd = (value: number): number => {
  if (!Number.isInteger(value) || value < 0 || value > 9999) {
    throw new Error('Delta PLC BCD value must be an integer between 0 and 9999.');
  }
  let current = value;
  let shift = 0;
  let result = 0;
  do {
    result |= (current % 10) << shift;
    current = Math.floor(current / 10);
    shift += 4;
  } while (current > 0);
  return result;
};
